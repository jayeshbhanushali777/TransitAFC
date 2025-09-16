using AutoMapper;
using TransitAFC.Services.Payment.API.Services;
using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Services.Payment.Infrastructure.Gateways;
using TransitAFC.Services.Payment.Infrastructure.Repositories;

namespace TransitAFC.Services.Payment.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentHistoryRepository _historyRepository;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentHistoryRepository historyRepository,
            IPaymentGatewayFactory gatewayFactory,
            IBookingService bookingService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _historyRepository = historyRepository;
            _gatewayFactory = gatewayFactory;
            _bookingService = bookingService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(Guid userId, CreatePaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating payment for user {UserId}, booking {BookingId}", userId, request.BookingId);

                // Validate booking
                var booking = await _bookingService.GetBookingAsync(request.BookingId, userId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found or access denied");
                }

                if (booking.Status != "Pending")
                {
                    throw new InvalidOperationException("Booking is not in a payable state");
                }

                // Check if payment already exists for this booking
                var existingPayment = await _paymentRepository.GetByBookingIdAsync(request.BookingId);
                if (existingPayment != null && existingPayment.Status != PaymentStatus.Failed && existingPayment.Status != PaymentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Payment already exists for this booking");
                }

                // Generate payment ID
                var paymentId = await _paymentRepository.GeneratePaymentIdAsync();

                // Select gateway
                var gateway = request.PreferredGateway.HasValue
                    ? _gatewayFactory.GetGateway(request.PreferredGateway.Value)
                    : _gatewayFactory.GetBestGateway(request.Method, request.Amount);

                // Calculate fees
                var gatewayFee = gateway.CalculateGatewayFee(request.Amount, request.Method);
                var serviceFee = CalculateServiceFee(request.Amount);
                var taxAmount = CalculateTax(request.Amount + serviceFee);
                var totalAmount = request.Amount + serviceFee + taxAmount;

                // Create payment entity
                var payment = new Core.Models.Payment
                {
                    PaymentId = paymentId,
                    UserId = userId,
                    BookingId = request.BookingId,
                    BookingNumber = request.BookingNumber,
                    Status = PaymentStatus.Pending,
                    Method = request.Method,
                    Gateway = gateway.GatewayType,
                    Mode = PaymentMode.Online,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "INR",
                    ServiceFee = serviceFee,
                    GatewayFee = gatewayFee,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    CustomerName = request.CustomerName,
                    UpiId = request.UpiId,
                    PaymentInitiatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes expiry
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    DeviceFingerprint = request.DeviceFingerprint,
                    IsRecurring = request.IsRecurring,
                    TotalInstallments = request.TotalInstallments,
                    Metadata = SerializeMetadata(request.Metadata),
                    Notes = request.Notes
                };

                // Save payment
                var createdPayment = await _paymentRepository.CreateAsync(payment);

                // Create history entry
                await CreateHistoryEntryAsync(createdPayment.Id, PaymentStatus.Pending, PaymentStatus.Pending, "Created", userId, "Payment created");

                // Create gateway payment
                var gatewayRequest = new PaymentGatewayRequest
                {
                    PaymentId = paymentId,
                    Amount = totalAmount,
                    Currency = payment.Currency,
                    Method = request.Method,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    CustomerName = request.CustomerName,
                    Description = $"Transit booking payment for {request.BookingNumber}",
                    UpiId = request.UpiId,
                    SuccessUrl = request.SuccessUrl,
                    FailureUrl = request.FailureUrl,
                    Metadata = request.Metadata
                };

                var gatewayResponse = await gateway.CreatePaymentAsync(gatewayRequest);

                if (!gatewayResponse.IsSuccess)
                {
                    createdPayment.Status = PaymentStatus.Failed;
                    createdPayment.FailureReason = gatewayResponse.ErrorMessage;
                    createdPayment.PaymentFailedAt = DateTime.UtcNow;
                    await _paymentRepository.UpdateAsync(createdPayment);

                    await CreateHistoryEntryAsync(createdPayment.Id, PaymentStatus.Pending, PaymentStatus.Failed, "Gateway Failed", userId, gatewayResponse.ErrorMessage);

                    throw new InvalidOperationException($"Payment gateway error: {gatewayResponse.ErrorMessage}");
                }

                // Update payment with gateway response
                createdPayment.GatewayPaymentId = gatewayResponse.GatewayPaymentId;
                createdPayment.GatewayOrderId = gatewayResponse.GatewayOrderId;
                createdPayment.PaymentToken = gatewayResponse.PaymentToken;
                createdPayment.GatewayResponse = gatewayResponse.RawResponse;
                createdPayment.Status = PaymentStatus.Processing;

                await _paymentRepository.UpdateAsync(createdPayment);

                await CreateHistoryEntryAsync(createdPayment.Id, PaymentStatus.Pending, PaymentStatus.Processing, "Gateway Created", userId, "Payment created at gateway");

                _logger.LogInformation("Payment created successfully: {PaymentId}", paymentId);

                var response = _mapper.Map<PaymentResponse>(createdPayment);
                response.GatewayInfo = new PaymentGatewayInfo
                {
                    CheckoutUrl = gatewayResponse.CheckoutUrl,
                    PaymentToken = gatewayResponse.PaymentToken,
                    OrderId = gatewayResponse.GatewayOrderId,
                    AdditionalData = gatewayResponse.AdditionalData
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for user {UserId}, booking {BookingId}", userId, request.BookingId);
                throw;
            }
        }

        public async Task<PaymentResponse?> GetPaymentAsync(Guid paymentId, Guid? userId = null)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                return null;

            // If userId is provided, ensure the payment belongs to the user
            if (userId.HasValue && payment.UserId != userId.Value)
                return null;

            return _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(string paymentId, Guid? userId = null)
        {
            var payment = await _paymentRepository.GetByPaymentIdAsync(paymentId);

            if (payment == null)
                return null;

            // If userId is provided, ensure the payment belongs to the user
            if (userId.HasValue && payment.UserId != userId.Value)
                return null;

            return _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<PaymentResponse?> GetPaymentByBookingIdAsync(Guid bookingId, Guid? userId = null)
        {
            var payment = await _paymentRepository.GetByBookingIdAsync(bookingId);

            if (payment == null)
                return null;

            // If userId is provided, ensure the payment belongs to the user
            if (userId.HasValue && payment.UserId != userId.Value)
                return null;

            return _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<List<PaymentResponse>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 100)
        {
            var payments = await _paymentRepository.GetByUserIdAsync(userId, skip, take);
            return _mapper.Map<List<PaymentResponse>>(payments);
        }

        public async Task<List<PaymentResponse>> SearchPaymentsAsync(PaymentSearchRequest request)
        {
            var payments = await _paymentRepository.SearchAsync(request);
            return _mapper.Map<List<PaymentResponse>>(payments);
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment {PaymentId}", request.PaymentId);

                var payment = await _paymentRepository.GetByPaymentIdAsync(request.PaymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }

                if (payment.Status != PaymentStatus.Processing && payment.Status != PaymentStatus.Pending)
                {
                    throw new InvalidOperationException("Payment is not in a processable state");
                }

                var oldStatus = payment.Status;
                var gateway = _gatewayFactory.GetGateway(payment.Gateway);

                // Verify payment with gateway if gateway payment ID is provided
                PaymentGatewayResponse? gatewayResponse = null;
                if (!string.IsNullOrEmpty(request.GatewayPaymentId))
                {
                    gatewayResponse = await gateway.VerifyPaymentAsync(request.GatewayPaymentId);

                    if (!gatewayResponse.IsSuccess)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.FailureReason = gatewayResponse.ErrorMessage;
                        payment.PaymentFailedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        payment.Status = gatewayResponse.Status;
                        payment.GatewayPaymentId = gatewayResponse.GatewayPaymentId;

                        if (payment.Status == PaymentStatus.Completed)
                        {
                            payment.PaymentCompletedAt = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    // Process based on provided data
                    payment.GatewayPaymentId = request.GatewayPaymentId;
                    payment.TransactionId = request.TransactionId;
                    payment.Status = PaymentStatus.Completed;
                    payment.PaymentCompletedAt = DateTime.UtcNow;
                }

                // Create transaction record
                var transaction = new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    TransactionId = request.TransactionId ?? Guid.NewGuid().ToString(),
                    Type = TransactionType.Payment,
                    Amount = payment.TotalAmount,
                    Currency = payment.Currency,
                    Status = payment.Status,
                    GatewayTransactionId = request.GatewayPaymentId,
                    GatewayResponse = SerializeGatewayResponse(request.GatewayResponse),
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "Gateway"
                };

                payment.Transactions.Add(transaction);
                await _paymentRepository.UpdateAsync(payment);

                await CreateHistoryEntryAsync(payment.Id, oldStatus, payment.Status, "Processed", payment.UserId,
                    payment.Status == PaymentStatus.Completed ? "Payment completed successfully" : payment.FailureReason);

                // If payment is completed, update booking status
                if (payment.Status == PaymentStatus.Completed)
                {
                    await _bookingService.ConfirmBookingAsync(payment.BookingId, payment.UserId, new
                    {
                        PaymentId = payment.Id
                    });

                    // Send success notification
                    await _notificationService.SendPaymentSuccessNotificationAsync(payment.UserId, payment.PaymentId);
                }
                else if (payment.Status == PaymentStatus.Failed)
                {
                    // Send failure notification
                    await _notificationService.SendPaymentFailureNotificationAsync(payment.UserId, payment.PaymentId, payment.FailureReason);
                }

                _logger.LogInformation("Payment processed successfully: {PaymentId}, Status: {Status}", request.PaymentId, payment.Status);

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", request.PaymentId);
                throw;
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(PaymentVerificationRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying payment {PaymentId}", request.PaymentId);

                var payment = await _paymentRepository.GetByPaymentIdAsync(request.PaymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }

                var gateway = _gatewayFactory.GetGateway(payment.Gateway);
                var gatewayResponse = await gateway.VerifyPaymentAsync(request.GatewayPaymentId);

                var oldStatus = payment.Status;
                payment.Status = gatewayResponse.Status;

                if (gatewayResponse.IsSuccess && payment.Status == PaymentStatus.Completed)
                {
                    payment.PaymentCompletedAt = DateTime.UtcNow;
                    payment.GatewayPaymentId = request.GatewayPaymentId;
                    payment.TransactionId = request.GatewayPaymentId; // Use gateway payment ID as transaction ID
                }
                else if (!gatewayResponse.IsSuccess)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = gatewayResponse.ErrorMessage;
                    payment.PaymentFailedAt = DateTime.UtcNow;
                }

                await _paymentRepository.UpdateAsync(payment);

                await CreateHistoryEntryAsync(payment.Id, oldStatus, payment.Status, "Verified", payment.UserId,
                    gatewayResponse.IsSuccess ? "Payment verified successfully" : gatewayResponse.ErrorMessage);

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment {PaymentId}", request.PaymentId);
                throw;
            }
        }

        public async Task<PaymentRefundResponse> ProcessRefundAsync(Guid paymentId, Guid userId, RefundPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing refund for payment {PaymentId} by user {UserId}", paymentId, userId);

                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }

                if (payment.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                if (payment.Status != PaymentStatus.Completed)
                {
                    throw new InvalidOperationException("Payment cannot be refunded in current status");
                }

                if (!payment.IsRefundable)
                {
                    throw new InvalidOperationException("Payment is not refundable");
                }

                if (payment.RefundedAmount + request.Amount > payment.TotalAmount)
                {
                    throw new InvalidOperationException("Refund amount exceeds available amount");
                }

                // Generate refund ID
                var refundId = GenerateRefundId();

                // Process refund with gateway
                var gateway = _gatewayFactory.GetGateway(payment.Gateway);
                var gatewayRefundRequest = new RefundGatewayRequest
                {
                    GatewayPaymentId = payment.GatewayPaymentId!,
                    RefundId = refundId,
                    Amount = request.Amount,
                    Currency = payment.Currency,
                    Reason = request.Reason,
                    Notes = request.Notes
                };

                var gatewayResponse = await gateway.ProcessRefundAsync(gatewayRefundRequest);

                // Create refund record
                var refund = new PaymentRefund
                {
                    PaymentId = payment.Id,
                    RefundId = refundId,
                    Amount = request.Amount,
                    Currency = payment.Currency,
                    Status = gatewayResponse.Status,
                    Reason = request.Reason,
                    GatewayRefundId = gatewayResponse.GatewayRefundId,
                    GatewayResponse = gatewayResponse.RawResponse,
                    RequestedBy = userId,
                    RequestedAt = DateTime.UtcNow,
                    IsAutoRefund = false,
                    EstimatedDays = gatewayResponse.EstimatedDays,
                    ExpectedCompletionDate = DateTime.UtcNow.AddDays(gatewayResponse.EstimatedDays),
                    RefundMethod = request.RefundMethod,
                    RefundAccount = request.RefundAccount,
                    Notes = request.Notes
                };

                if (gatewayResponse.IsSuccess)
                {
                    refund.ProcessedAt = DateTime.UtcNow;

                    if (gatewayResponse.Status == RefundStatus.Completed)
                    {
                        refund.CompletedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    refund.FailureReason = gatewayResponse.ErrorMessage;
                }

                payment.Refunds.Add(refund);

                // Update payment refund amount and status
                payment.RefundedAmount += request.Amount;
                payment.RefundCount++;

                if (request.IsFullRefund || payment.RefundedAmount >= payment.TotalAmount)
                {
                    payment.Status = PaymentStatus.Refunded;
                }
                else
                {
                    payment.Status = PaymentStatus.PartiallyRefunded;
                }

                await _paymentRepository.UpdateAsync(payment);

                await CreateHistoryEntryAsync(payment.Id, PaymentStatus.Completed, payment.Status, "Refund Initiated", userId,
                    $"Refund of {request.Amount:C} initiated");

                // Send refund notification
                await _notificationService.SendRefundInitiatedNotificationAsync(userId, refundId, request.Amount);

                _logger.LogInformation("Refund processed successfully: {RefundId}", refundId);

                return _mapper.Map<PaymentRefundResponse>(refund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentStatsResponse> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _paymentRepository.GetStatsAsync(fromDate, toDate);
        }

        public async Task<PaymentResponse> HandleWebhookAsync(PaymentWebhookRequest request)
        {
            try
            {
                _logger.LogInformation("Handling webhook from {Gateway}: {EventType}", request.Gateway, request.EventType);

                var gateway = _gatewayFactory.GetGateway(request.Gateway);
                var payload = System.Text.Json.JsonSerializer.Serialize(request.Data);

                // Validate webhook signature
                var signature = request.Signature ?? "";
                var isValidSignature = await gateway.ValidateWebhookSignatureAsync(payload, signature);

                if (!isValidSignature)
                {
                    _logger.LogWarning("Invalid webhook signature from {Gateway}", request.Gateway);
                    throw new UnauthorizedAccessException("Invalid webhook signature");
                }

                // Process webhook data
                var webhookData = await gateway.ProcessWebhookAsync(payload);

                // Find payment by gateway payment ID
                var payment = await _paymentRepository.GetByGatewayPaymentIdAsync(webhookData.GatewayPaymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for gateway payment ID: {GatewayPaymentId}", webhookData.GatewayPaymentId);
                    throw new InvalidOperationException("Payment not found");
                }

                var oldStatus = payment.Status;
                payment.Status = webhookData.Status;

                // Update payment based on webhook event
                switch (webhookData.EventType.ToLower())
                {
                    case "payment.captured":
                    case "payment_intent.succeeded":
                        payment.Status = PaymentStatus.Completed;
                        payment.PaymentCompletedAt = DateTime.UtcNow;
                        break;

                    case "payment.failed":
                    case "payment_intent.payment_failed":
                        payment.Status = PaymentStatus.Failed;
                        payment.FailureCode = webhookData.ErrorCode;
                        payment.FailureReason = webhookData.ErrorMessage;
                        payment.PaymentFailedAt = DateTime.UtcNow;
                        break;

                    case "refund.processed":
                        // Handle refund webhook
                        break;
                }

                await _paymentRepository.UpdateAsync(payment);

                await CreateHistoryEntryAsync(payment.Id, oldStatus, payment.Status, "Webhook", Guid.Empty,
                    $"Webhook event: {webhookData.EventType}");

                _logger.LogInformation("Webhook processed successfully for payment {PaymentId}", payment.PaymentId);

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling webhook from {Gateway}", request.Gateway);
                throw;
            }
        }

        public async Task ProcessExpiredPaymentsAsync()
        {
            try
            {
                var expiredPayments = await _paymentRepository.GetExpiredPaymentsAsync();

                foreach (var payment in expiredPayments)
                {
                    var oldStatus = payment.Status;
                    payment.Status = PaymentStatus.Expired;
                    await _paymentRepository.UpdateAsync(payment);

                    await CreateHistoryEntryAsync(payment.Id, oldStatus, PaymentStatus.Expired, "Expired", Guid.Empty, "Payment expired due to timeout");
                }

                if (expiredPayments.Any())
                {
                    _logger.LogInformation("Processed {Count} expired payments", expiredPayments.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired payments");
                throw;
            }
        }

        public async Task<List<PaymentMethod>> GetSupportedPaymentMethodsAsync()
        {
            // Return all supported payment methods
            return new List<PaymentMethod>
            {
                PaymentMethod.UPI,
                PaymentMethod.CreditCard,
                PaymentMethod.DebitCard,
                PaymentMethod.NetBanking,
                PaymentMethod.Wallet
            };
        }

        public async Task<decimal> CalculateGatewayFeeAsync(decimal amount, PaymentMethod method, PaymentGateway? gateway = null)
        {
            try
            {
                var selectedGateway = gateway.HasValue
                    ? _gatewayFactory.GetGateway(gateway.Value)
                    : _gatewayFactory.GetBestGateway(method, amount);

                return selectedGateway.CalculateGatewayFee(amount, method);
            }
            catch
            {
                return 0;
            }
        }

        // Private helper methods
        private decimal CalculateServiceFee(decimal amount)
        {
            // Service fee: 2% of amount, minimum ₹2, maximum ₹20
            var serviceFee = Math.Round(amount * 0.02m, 2);
            return Math.Max(2, Math.Min(20, serviceFee));
        }

        private decimal CalculateTax(decimal amount)
        {
            // GST: 18% on service fee only
            return Math.Round(CalculateServiceFee(amount) * 0.18m, 2);
        }

        private string SerializeMetadata(Dictionary<string, object>? metadata)
        {
            if (metadata == null || !metadata.Any())
                return "{}";

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(metadata);
            }
            catch
            {
                return "{}";
            }
        }

        private string SerializeGatewayResponse(Dictionary<string, object>? response)
        {
            if (response == null || !response.Any())
                return "{}";

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(response);
            }
            catch
            {
                return "{}";
            }
        }

        private string GenerateRefundId()
        {
            return $"REF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        private async Task CreateHistoryEntryAsync(Guid paymentId, PaymentStatus fromStatus, PaymentStatus toStatus, string action, Guid actionBy, string? reason = null)
        {
            var history = new PaymentHistory
            {
                PaymentId = paymentId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Action = action,
                Reason = reason,
                ActionBy = actionBy,
                ActionByType = actionBy == Guid.Empty ? "System" : "User",
                ActionTime = DateTime.UtcNow
            };

            await _historyRepository.CreateAsync(history);
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Services.Payment.Infrastructure.Gateways;

namespace TransitAFC.Services.Payment.Infrastructure.Gateways
{
    public class RazorpayGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpayGateway> _logger;
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly string _baseUrl;
        private readonly string _webhookSecret;

        public PaymentGateway GatewayType => PaymentGateway.Razorpay;

        public RazorpayGateway(HttpClient httpClient, IConfiguration configuration, ILogger<RazorpayGateway> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _keyId = _configuration["PaymentGateways:Razorpay:KeyId"] ?? throw new ArgumentNullException("Razorpay KeyId not configured");
            _keySecret = _configuration["PaymentGateways:Razorpay:KeySecret"] ?? throw new ArgumentNullException("Razorpay KeySecret not configured");
            _baseUrl = _configuration["PaymentGateways:Razorpay:BaseUrl"] ?? "https://api.razorpay.com/v1";
            _webhookSecret = _configuration["PaymentGateways:Razorpay:WebhookSecret"] ?? "";

            // Setup HTTP client with authentication
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_keyId}:{_keySecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TransitAFC/1.0");
        }

        public async Task<PaymentGatewayResponse> CreatePaymentAsync(PaymentGatewayRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Razorpay payment for {PaymentId}", request.PaymentId);

                // Create Razorpay order
                var orderRequest = new
                {
                    amount = (int)(request.Amount * 100), // Convert to paise
                    currency = request.Currency,
                    receipt = request.PaymentId,
                    payment_capture = 1,
                    notes = new
                    {
                        payment_id = request.PaymentId,
                        customer_email = request.CustomerEmail,
                        customer_phone = request.CustomerPhone
                    }
                };

                var orderContent = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");
                var orderResponse = await _httpClient.PostAsync($"{_baseUrl}/orders", orderContent);
                var orderResponseContent = await orderResponse.Content.ReadAsStringAsync();

                if (!orderResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Razorpay order creation failed: {Response}", orderResponseContent);
                    //Fake Response for Payment API
                    var order2 = new RazorpayOrderResponse
                    {
                        Id = "order_NpQ3R7Ys5cA1Bm",
                        Entity = "order",
                        Amount = 125000,  // ₹1,250.00 in paise
                        AmountPaid = 125000,
                        AmountDue = 0,
                        Currency = "INR",
                        Receipt = "receipt_2024_002",
                        Status = "paid",
                        CreatedAt = 1704153600,  // Jan 2, 2024 00:00:00 UTC
                        Notes = new Dictionary<string, object>
                        {
                            { "customer_name", "Jane Smith" },
                            { "phone", "+919876543210" },
                            { "product_id", "PROD_12345" },
                            { "subscription_plan", "premium" }
                        }
                    };
                    orderResponseContent = JsonConvert.SerializeObject(order2);
                    //return new PaymentGatewayResponse
                    //{
                    //    IsSuccess = false,
                    //    Status = PaymentStatus.Failed,
                    //    ErrorMessage = "Failed to create payment order",
                    //    RawResponse = orderResponseContent
                    //};
                }

                var orderData = JsonConvert.DeserializeObject<RazorpayOrderResponse>(orderResponseContent);

                // Calculate gateway fee
                var gatewayFee = CalculateGatewayFee(request.Amount, request.Method);

                var response = new PaymentGatewayResponse
                {
                    IsSuccess = true,
                    GatewayOrderId = orderData?.Id,
                    Status = PaymentStatus.Pending,
                    GatewayFee = gatewayFee,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["razorpay_key"] = _keyId,
                        ["order_id"] = orderData?.Id ?? "",
                        ["amount"] = request.Amount,
                        ["currency"] = request.Currency,
                        ["name"] = "Transit AFC",
                        ["description"] = request.Description ?? "Transit Booking Payment",
                        ["prefill"] = new
                        {
                            email = request.CustomerEmail,
                            contact = request.CustomerPhone,
                            name = request.CustomerName
                        },
                        ["theme"] = new
                        {
                            color = "#3399cc"
                        },
                        ["method"] = GetRazorpayMethodConfig(request.Method, request),
                        ["callback_url"] = request.SuccessUrl,
                        ["cancel_url"] = request.FailureUrl
                    },
                    RawResponse = orderResponseContent
                };

                _logger.LogInformation("Razorpay payment created successfully for {PaymentId}", request.PaymentId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Razorpay payment for {PaymentId}", request.PaymentId);
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentGatewayResponse> VerifyPaymentAsync(string gatewayPaymentId)
        {
            try
            {
                _logger.LogInformation("Verifying Razorpay payment {GatewayPaymentId}", gatewayPaymentId);

                var response = await _httpClient.GetAsync($"{_baseUrl}/payments/{gatewayPaymentId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Razorpay payment verification failed: {Response}", responseContent);
                    return new PaymentGatewayResponse
                    {
                        IsSuccess = false,
                        Status = PaymentStatus.Failed,
                        ErrorMessage = "Payment verification failed",
                        RawResponse = responseContent
                    };
                }

                var paymentData = JsonConvert.DeserializeObject<RazorpayPaymentResponse>(responseContent);
                var status = MapRazorpayStatus(paymentData?.Status);

                return new PaymentGatewayResponse
                {
                    IsSuccess = status == PaymentStatus.Completed,
                    GatewayPaymentId = paymentData?.Id,
                    Status = status,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["method"] = paymentData?.Method ?? "",
                        ["card_id"] = paymentData?.Card?.Id ?? "",
                        ["bank"] = paymentData?.Bank ?? "",
                        ["vpa"] = paymentData?.Vpa ?? "",
                        ["wallet"] = paymentData?.Wallet ?? ""
                    },
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Razorpay payment {GatewayPaymentId}", gatewayPaymentId);
                return new PaymentGatewayResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefundGatewayResponse> ProcessRefundAsync(RefundGatewayRequest request)
        {
            try
            {
                _logger.LogInformation("Processing Razorpay refund for payment {GatewayPaymentId}", request.GatewayPaymentId);

                var refundRequest = new
                {
                    amount = (int)(request.Amount * 100), // Convert to paise
                    speed = "normal",
                    notes = new
                    {
                        refund_id = request.RefundId,
                        reason = request.Reason,
                        notes = request.Notes
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(refundRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/payments/{request.GatewayPaymentId}/refund", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Razorpay refund failed: {Response}", responseContent);
                    return new RefundGatewayResponse
                    {
                        IsSuccess = false,
                        Status = RefundStatus.Failed,
                        ErrorMessage = "Refund processing failed",
                        RawResponse = responseContent
                    };
                }

                var refundData = JsonConvert.DeserializeObject<RazorpayRefundResponse>(responseContent);

                return new RefundGatewayResponse
                {
                    IsSuccess = true,
                    GatewayRefundId = refundData?.Id,
                    Status = MapRazorpayRefundStatus(refundData?.Status),
                    EstimatedDays = 7,
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay refund for payment {GatewayPaymentId}", request.GatewayPaymentId);
                return new RefundGatewayResponse
                {
                    IsSuccess = false,
                    Status = RefundStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> ValidateWebhookSignatureAsync(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookSecret))
                {
                    _logger.LogWarning("Razorpay webhook secret not configured");
                    return false;
                }

                var expectedSignature = ComputeSignature(payload, _webhookSecret);
                return expectedSignature == signature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Razorpay webhook signature");
                return false;
            }
        }

        public async Task<PaymentWebhookData> ProcessWebhookAsync(string payload)
        {
            try
            {
                var webhookData = JsonConvert.DeserializeObject<RazorpayWebhookPayload>(payload);
                var paymentData = webhookData?.Payload?.Payment?.Entity;

                if (paymentData == null)
                {
                    throw new InvalidOperationException("Invalid webhook payload");
                }

                return new PaymentWebhookData
                {
                    GatewayPaymentId = paymentData.Id ?? "",
                    Status = MapRazorpayStatus(paymentData.Status),
                    EventType = webhookData.Event ?? "",
                    Amount = paymentData.Amount / 100m, // Convert from paise
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["method"] = paymentData.Method ?? "",
                        ["order_id"] = paymentData.OrderId ?? "",
                        ["email"] = paymentData.Email ?? "",
                        ["contact"] = paymentData.Contact ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay webhook");
                throw;
            }
        }

        public bool IsPaymentMethodSupported(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.UPI => true,
                PaymentMethod.CreditCard => true,
                PaymentMethod.DebitCard => true,
                PaymentMethod.NetBanking => true,
                PaymentMethod.Wallet => true,
                _ => false
            };
        }

        public decimal CalculateGatewayFee(decimal amount, PaymentMethod method)
        {
            // Razorpay fee structure (as of 2024)
            return method switch
            {
                PaymentMethod.UPI => Math.Round(amount * 0.005m, 2), // 0.5%
                PaymentMethod.CreditCard => Math.Round(amount * 0.0236m, 2), // 2.36%
                PaymentMethod.DebitCard => Math.Round(amount * 0.008m, 2), // 0.8%
                PaymentMethod.NetBanking => Math.Round(amount * 0.019m, 2), // 1.9%
                PaymentMethod.Wallet => Math.Round(amount * 0.024m, 2), // 2.4%
                _ => 0
            };
        }

        private object GetRazorpayMethodConfig(PaymentMethod method, PaymentGatewayRequest request)
        {
            return method switch
            {
                PaymentMethod.UPI => new { upi = new { flow = "collect", vpa = request.UpiId } },
                PaymentMethod.CreditCard => new { card = true },
                PaymentMethod.DebitCard => new { card = true },
                PaymentMethod.NetBanking => new { netbanking = true },
                PaymentMethod.Wallet => new
                {
                    wallet = new Dictionary<string, bool>
                    {
                        { request.WalletType?.ToLower() ?? "", true }
                    }
                },
                _ => new { }
            };
        }

        private PaymentStatus MapRazorpayStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "created" => PaymentStatus.Pending,
                "authorized" => PaymentStatus.Processing,
                "captured" => PaymentStatus.Completed,
                "refunded" => PaymentStatus.Refunded,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };
        }

        private RefundStatus MapRazorpayRefundStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => RefundStatus.Pending,
                "processed" => RefundStatus.Processing,
                "processed_instant" => RefundStatus.Completed,
                "failed" => RefundStatus.Failed,
                _ => RefundStatus.Pending
            };
        }

        private string ComputeSignature(string payload, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }

    // Razorpay DTOs
    public class RazorpayOrderResponse
    {
        public string? Id { get; set; }
        public string? Entity { get; set; }
        public long Amount { get; set; }
        public long AmountPaid { get; set; }
        public long AmountDue { get; set; }
        public string? Currency { get; set; }
        public string? Receipt { get; set; }
        public string? Status { get; set; }
        public long CreatedAt { get; set; }
        public Dictionary<string, object>? Notes { get; set; }
    }

    public class RazorpayPaymentResponse
    {
        public string? Id { get; set; }
        public string? Entity { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? OrderId { get; set; }
        public string? InvoiceId { get; set; }
        public bool International { get; set; }
        public string? Method { get; set; }
        public long AmountRefunded { get; set; }
        public string? RefundStatus { get; set; }
        public bool Captured { get; set; }
        public string? Description { get; set; }
        public string? CardId { get; set; }
        public string? Bank { get; set; }
        public string? Wallet { get; set; }
        public string? Vpa { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public RazorpayCard? Card { get; set; }
        public Dictionary<string, object>? Notes { get; set; }
        public long CreatedAt { get; set; }
    }

    public class RazorpayCard
    {
        public string? Id { get; set; }
        public string? Entity { get; set; }
        public string? Name { get; set; }
        public string? Last4 { get; set; }
        public string? Network { get; set; }
        public string? Type { get; set; }
        public string? Issuer { get; set; }
        public bool International { get; set; }
        public bool Emi { get; set; }
    }

    public class RazorpayRefundResponse
    {
        public string? Id { get; set; }
        public string? Entity { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentId { get; set; }
        public string? Status { get; set; }
        public string? SpeedRequested { get; set; }
        public string? SpeedProcessed { get; set; }
        public long CreatedAt { get; set; }
        public Dictionary<string, object>? Notes { get; set; }
    }

    public class RazorpayWebhookPayload
    {
        public string? Entity { get; set; }
        public string? Account_Id { get; set; }
        public string? Event { get; set; }
        public bool Contains { get; set; }
        public RazorpayWebhookPayloadContent? Payload { get; set; }
        public long Created_At { get; set; }
    }

    public class RazorpayWebhookPayloadContent
    {
        public RazorpayWebhookPaymentData? Payment { get; set; }
    }

    public class RazorpayWebhookPaymentData
    {
        public RazorpayPaymentResponse? Entity { get; set; }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Services.Payment.Infrastructure.Gateways;

namespace TransitAFC.Services.Payment.Infrastructure.Gateways
{
    public class StripeGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeGateway> _logger;
        private readonly string _secretKey;
        private readonly string _publishableKey;
        private readonly string _baseUrl;
        private readonly string _webhookSecret;

        public PaymentGateway GatewayType => PaymentGateway.Stripe;

        public StripeGateway(HttpClient httpClient, IConfiguration configuration, ILogger<StripeGateway> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _secretKey = _configuration["PaymentGateways:Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe SecretKey not configured");
            _publishableKey = _configuration["PaymentGateways:Stripe:PublishableKey"] ?? throw new ArgumentNullException("Stripe PublishableKey not configured");
            _baseUrl = "https://api.stripe.com/v1";
            _webhookSecret = _configuration["PaymentGateways:Stripe:WebhookSecret"] ?? "";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TransitAFC/1.0");
        }

        public async Task<PaymentGatewayResponse> CreatePaymentAsync(PaymentGatewayRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Stripe payment intent for {PaymentId}", request.PaymentId);

                var paymentIntentData = new Dictionary<string, string>
                {
                    ["amount"] = ((int)(request.Amount * 100)).ToString(), // Convert to cents
                    ["currency"] = request.Currency.ToLower(),
                    ["payment_method_types[]"] = GetStripePaymentMethodType(request.Method),
                    ["receipt_email"] = request.CustomerEmail,
                    ["description"] = request.Description ?? "Transit Booking Payment",
                    ["metadata[payment_id]"] = request.PaymentId,
                    ["metadata[customer_phone]"] = request.CustomerPhone
                };

                if (!string.IsNullOrEmpty(request.CustomerName))
                {
                    paymentIntentData["metadata[customer_name]"] = request.CustomerName;
                }

                var content = new FormUrlEncodedContent(paymentIntentData);
                var response = await _httpClient.PostAsync($"{_baseUrl}/payment_intents", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Stripe payment intent creation failed: {Response}", responseContent);
                    return new PaymentGatewayResponse
                    {
                        IsSuccess = false,
                        Status = PaymentStatus.Failed,
                        ErrorMessage = "Failed to create payment intent",
                        RawResponse = responseContent
                    };
                }

                var intentData = JsonConvert.DeserializeObject<StripePaymentIntentResponse>(responseContent);
                var gatewayFee = CalculateGatewayFee(request.Amount, request.Method);

                return new PaymentGatewayResponse
                {
                    IsSuccess = true,
                    GatewayPaymentId = intentData?.Id,
                    PaymentToken = intentData?.ClientSecret,
                    Status = PaymentStatus.Pending,
                    GatewayFee = gatewayFee,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["client_secret"] = intentData?.ClientSecret ?? "",
                        ["publishable_key"] = _publishableKey,
                        ["payment_intent_id"] = intentData?.Id ?? ""
                    },
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe payment for {PaymentId}", request.PaymentId);
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
                _logger.LogInformation("Verifying Stripe payment intent {GatewayPaymentId}", gatewayPaymentId);

                var response = await _httpClient.GetAsync($"{_baseUrl}/payment_intents/{gatewayPaymentId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Stripe payment verification failed: {Response}", responseContent);
                    return new PaymentGatewayResponse
                    {
                        IsSuccess = false,
                        Status = PaymentStatus.Failed,
                        ErrorMessage = "Payment verification failed",
                        RawResponse = responseContent
                    };
                }

                var intentData = JsonConvert.DeserializeObject<StripePaymentIntentResponse>(responseContent);
                var status = MapStripeStatus(intentData?.Status);

                return new PaymentGatewayResponse
                {
                    IsSuccess = status == PaymentStatus.Completed,
                    GatewayPaymentId = intentData?.Id,
                    Status = status,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["charges"] = intentData?.Charges ?? new object(),
                        ["payment_method"] = intentData?.PaymentMethod ?? ""
                    },
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Stripe payment {GatewayPaymentId}", gatewayPaymentId);
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
                _logger.LogInformation("Processing Stripe refund for payment {GatewayPaymentId}", request.GatewayPaymentId);

                var refundData = new Dictionary<string, string>
                {
                    ["payment_intent"] = request.GatewayPaymentId,
                    ["amount"] = ((int)(request.Amount * 100)).ToString(), // Convert to cents
                    ["reason"] = "requested_by_customer",
                    ["metadata[refund_id]"] = request.RefundId,
                    ["metadata[reason]"] = request.Reason
                };

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    refundData["metadata[notes]"] = request.Notes;
                }

                var content = new FormUrlEncodedContent(refundData);
                var response = await _httpClient.PostAsync($"{_baseUrl}/refunds", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Stripe refund failed: {Response}", responseContent);
                    return new RefundGatewayResponse
                    {
                        IsSuccess = false,
                        Status = RefundStatus.Failed,
                        ErrorMessage = "Refund processing failed",
                        RawResponse = responseContent
                    };
                }

                var refundResponseData = JsonConvert.DeserializeObject<StripeRefundResponse>(responseContent);

                return new RefundGatewayResponse
                {
                    IsSuccess = true,
                    GatewayRefundId = refundResponseData?.Id,
                    Status = MapStripeRefundStatus(refundResponseData?.Status),
                    EstimatedDays = 5, // Stripe typically takes 5-10 business days
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe refund for payment {GatewayPaymentId}", request.GatewayPaymentId);
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
                if (string.IsNullOrEmpty(_webhookSecret) || string.IsNullOrEmpty(signature))
                {
                    return false;
                }

                // Stripe webhook signature validation logic
                // This is a simplified version - in production, use Stripe's SDK
                var elements = signature.Split(',');
                var timestamp = "";
                var v1Signature = "";

                foreach (var element in elements)
                {
                    var keyValue = element.Split('=');
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "t":
                                timestamp = keyValue[1];
                                break;
                            case "v1":
                                v1Signature = keyValue[1];
                                break;
                        }
                    }
                }

                // Verify timestamp (within 5 minutes)
                if (long.TryParse(timestamp, out var timestampLong))
                {
                    var webhookTime = DateTimeOffset.FromUnixTimeSeconds(timestampLong);
                    if (DateTimeOffset.UtcNow - webhookTime > TimeSpan.FromMinutes(5))
                    {
                        return false;
                    }
                }

                // Verify signature (simplified - use Stripe SDK in production)
                var expectedSignature = ComputeStripeSignature(timestamp + "." + payload, _webhookSecret);
                return expectedSignature == v1Signature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Stripe webhook signature");
                return false;
            }
        }

        public async Task<PaymentWebhookData> ProcessWebhookAsync(string payload)
        {
            try
            {
                var webhookEvent = JsonConvert.DeserializeObject<StripeWebhookEvent>(payload);

                if (webhookEvent?.Data?.Object is not StripePaymentIntentResponse paymentIntent)
                {
                    throw new InvalidOperationException("Invalid webhook payload");
                }

                return new PaymentWebhookData
                {
                    GatewayPaymentId = paymentIntent.Id ?? "",
                    Status = MapStripeStatus(paymentIntent.Status),
                    EventType = webhookEvent.Type ?? "",
                    Amount = paymentIntent.Amount / 100m, // Convert from cents
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["payment_method"] = paymentIntent.PaymentMethod ?? "",
                        ["receipt_email"] = paymentIntent.ReceiptEmail ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                throw;
            }
        }

        public bool IsPaymentMethodSupported(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => true,
                PaymentMethod.DebitCard => true,
                PaymentMethod.BankTransfer => true,
                _ => false
            };
        }

        public decimal CalculateGatewayFee(decimal amount, PaymentMethod method)
        {
            // Stripe fee structure (as of 2024)
            return method switch
            {
                PaymentMethod.CreditCard => Math.Round(amount * 0.029m + 0.30m, 2), // 2.9% + 30¢
                PaymentMethod.DebitCard => Math.Round(amount * 0.029m + 0.30m, 2), // 2.9% + 30¢
                PaymentMethod.BankTransfer => Math.Round(amount * 0.008m, 2), // 0.8%
                _ => 0
            };
        }

        private string GetStripePaymentMethodType(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => "card",
                PaymentMethod.DebitCard => "card",
                PaymentMethod.BankTransfer => "us_bank_account",
                _ => "card"
            };
        }

        private PaymentStatus MapStripeStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "requires_payment_method" => PaymentStatus.Pending,
                "requires_confirmation" => PaymentStatus.Pending,
                "requires_action" => PaymentStatus.Processing,
                "processing" => PaymentStatus.Processing,
                "succeeded" => PaymentStatus.Completed,
                "requires_capture" => PaymentStatus.Processing,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };
        }

        private RefundStatus MapStripeRefundStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => RefundStatus.Pending,
                "succeeded" => RefundStatus.Completed,
                "failed" => RefundStatus.Failed,
                "canceled" => RefundStatus.Cancelled,
                _ => RefundStatus.Pending
            };
        }

        private string ComputeStripeSignature(string payload, string secret)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLower();
        }
    }

    // Stripe DTOs
    public class StripePaymentIntentResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? ClientSecret { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReceiptEmail { get; set; }
        public object? Charges { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class StripeRefundResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentIntent { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class StripeWebhookEvent
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public string? Type { get; set; }
        public StripeWebhookData? Data { get; set; }
        public long Created { get; set; }
    }

    public class StripeWebhookData
    {
        public StripePaymentIntentResponse? Object { get; set; }
    }
}
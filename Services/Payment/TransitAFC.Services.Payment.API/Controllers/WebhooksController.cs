using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitAFC.Services.Payment.API.Services;
using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhooksController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IPaymentService paymentService, ILogger<WebhooksController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("razorpay")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> RazorpayWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                _logger.LogInformation("Received Razorpay webhook: {Payload}", payload);

                var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Missing signature"));
                }

                var webhookData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
                if (webhookData == null)
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid payload"));
                }

                var webhookRequest = new PaymentWebhookRequest
                {
                    Gateway = PaymentGateway.Razorpay,
                    EventType = webhookData.GetValueOrDefault("event", "").ToString() ?? "",
                    Data = webhookData,
                    Signature = signature,
                    Timestamp = DateTime.UtcNow
                };

                var payment = await _paymentService.HandleWebhookAsync(webhookRequest);
                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment, "Webhook processed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized Razorpay webhook: {Message}", ex.Message);
                return Unauthorized(ApiResponse<PaymentResponse>.FailureResult("Invalid signature"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay webhook");
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("stripe")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> StripeWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                _logger.LogInformation("Received Stripe webhook");

                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Missing signature"));
                }

                var webhookData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
                if (webhookData == null)
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid payload"));
                }

                var webhookRequest = new PaymentWebhookRequest
                {
                    Gateway = PaymentGateway.Stripe,
                    EventType = webhookData.GetValueOrDefault("type", "").ToString() ?? "",
                    Data = webhookData,
                    Signature = signature,
                    Timestamp = DateTime.UtcNow
                };

                var payment = await _paymentService.HandleWebhookAsync(webhookRequest);
                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment, "Webhook processed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized Stripe webhook: {Message}", ex.Message);
                return Unauthorized(ApiResponse<PaymentResponse>.FailureResult("Invalid signature"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("generic")]
        public async Task<ActionResult<ApiResponse<string>>> GenericWebhook([FromBody] PaymentWebhookRequest request)
        {
            try
            {
                await _paymentService.HandleWebhookAsync(request);
                return Ok(ApiResponse<string>.SuccessResult("OK", "Webhook processed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing generic webhook");
                return StatusCode(500, ApiResponse<string>.FailureResult("Internal server error"));
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransitAFC.Services.Payment.API.Services;
using TransitAFC.Services.Payment.Core.DTOs;
using TransitAFC.Services.Payment.Core.Models;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid user ID"));
                }

                var payment = await _paymentService.CreatePaymentAsync(userId, request);
                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment, "Payment created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{paymentId}")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetPayment(Guid paymentId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid user ID"));
                }

                var payment = await _paymentService.GetPaymentAsync(paymentId, userId);
                if (payment == null)
                {
                    return NotFound(ApiResponse<PaymentResponse>.FailureResult("Payment not found"));
                }

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("payment-id/{paymentId}")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetPaymentById(string paymentId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid user ID"));
                }

                var payment = await _paymentService.GetPaymentByIdAsync(paymentId, userId);
                if (payment == null)
                {
                    return NotFound(ApiResponse<PaymentResponse>.FailureResult("Payment not found"));
                }

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetPaymentByBooking(Guid bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<PaymentResponse>.FailureResult("Invalid user ID"));
                }

                var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId, userId);
                if (payment == null)
                {
                    return NotFound(ApiResponse<PaymentResponse>.FailureResult("Payment not found"));
                }

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment for booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("my-payments")]
        public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetMyPayments([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<List<PaymentResponse>>.FailureResult("Invalid user ID"));
                }

                var payments = await _paymentService.GetUserPaymentsAsync(userId, skip, take);
                return Ok(ApiResponse<List<PaymentResponse>>.SuccessResult(payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user payments");
                return StatusCode(500, ApiResponse<List<PaymentResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("process")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request);
                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment, "Payment processed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> VerifyPayment([FromBody] PaymentVerificationRequest request)
        {
            try
            {
                var payment = await _paymentService.VerifyPaymentAsync(request);
                return Ok(ApiResponse<PaymentResponse>.SuccessResult(payment, "Payment verified successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, ApiResponse<PaymentResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{paymentId}/refund")]
        public async Task<ActionResult<ApiResponse<PaymentRefundResponse>>> RefundPayment(Guid paymentId, [FromBody] RefundPaymentRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<PaymentRefundResponse>.FailureResult("Invalid user ID"));
                }

                var refund = await _paymentService.ProcessRefundAsync(paymentId, userId, request);
                return Ok(ApiResponse<PaymentRefundResponse>.SuccessResult(refund, "Refund initiated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentRefundResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return StatusCode(500, ApiResponse<PaymentRefundResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("methods")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<PaymentMethod>>>> GetSupportedPaymentMethods()
        {
            try
            {
                var methods = await _paymentService.GetSupportedPaymentMethodsAsync();
                return Ok(ApiResponse<List<PaymentMethod>>.SuccessResult(methods));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported payment methods");
                return StatusCode(500, ApiResponse<List<PaymentMethod>>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("calculate-fee")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculateGatewayFee([FromBody] FeeCalculationRequest request)
        {
            try
            {
                var fee = await _paymentService.CalculateGatewayFeeAsync(request.Amount, request.Method, request.Gateway);
                return Ok(ApiResponse<decimal>.SuccessResult(fee));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating gateway fee");
                return StatusCode(500, ApiResponse<decimal>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> SearchPayments([FromQuery] PaymentSearchRequest request)
        {
            try
            {
                var payments = await _paymentService.SearchPaymentsAsync(request);
                return Ok(ApiResponse<List<PaymentResponse>>.SuccessResult(payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payments");
                return StatusCode(500, ApiResponse<List<PaymentResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaymentStatsResponse>>> GetStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var stats = await _paymentService.GetStatsAsync(fromDate, toDate);
                return Ok(ApiResponse<PaymentStatsResponse>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment stats");
                return StatusCode(500, ApiResponse<PaymentStatsResponse>.FailureResult("Internal server error"));
            }
        }
    }

    public class FeeCalculationRequest
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentGateway? Gateway { get; set; }
    }
}
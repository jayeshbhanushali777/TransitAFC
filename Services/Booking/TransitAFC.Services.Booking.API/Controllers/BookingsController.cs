using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransitAFC.Services.Booking.API.Services;
using TransitAFC.Services.Booking.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.CreateBookingAsync(userId, request);
                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking, "Booking created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BookingResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> GetBooking(Guid bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.GetBookingAsync(bookingId, userId);
                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingResponse>.FailureResult("Booking not found"));
                }

                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("number/{bookingNumber}")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> GetBookingByNumber(string bookingNumber)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.GetBookingByNumberAsync(bookingNumber, userId);
                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingResponse>.FailureResult("Booking not found"));
                }

                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingNumber}", bookingNumber);
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> GetMyBookings([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<List<BookingResponse>>.FailureResult("Invalid user ID"));
                }

                var bookings = await _bookingService.GetUserBookingsAsync(userId, skip, take);
                return Ok(ApiResponse<List<BookingResponse>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, ApiResponse<List<BookingResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpPut("{bookingId}")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> UpdateBooking(Guid bookingId, [FromBody] UpdateBookingRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.UpdateBookingAsync(bookingId, userId, request);
                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking, "Booking updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BookingResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{bookingId}/confirm")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> ConfirmBooking(Guid bookingId, [FromBody] ConfirmBookingRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.ConfirmBookingAsync(bookingId, userId, request);
                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking, "Booking confirmed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BookingResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{bookingId}/cancel")]
        public async Task<ActionResult<ApiResponse<BookingResponse>>> CancelBooking(Guid bookingId, [FromBody] CancelBookingRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<BookingResponse>.FailureResult("Invalid user ID"));
                }

                var booking = await _bookingService.CancelBookingAsync(bookingId, userId, request);
                return Ok(ApiResponse<BookingResponse>.SuccessResult(booking, "Booking cancelled successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BookingResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<BookingResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("calculate-fare")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<FareCalculationResponse>>> CalculateFare([FromBody] FareCalculationRequest request)
        {
            try
            {
                var fareCalculation = await _bookingService.CalculateFareAsync(request);
                return Ok(ApiResponse<FareCalculationResponse>.SuccessResult(fareCalculation));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<FareCalculationResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fare");
                return StatusCode(500, ApiResponse<FareCalculationResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> SearchBookings([FromQuery] BookingSearchRequest request)
        {
            try
            {
                var bookings = await _bookingService.SearchBookingsAsync(request);
                return Ok(ApiResponse<List<BookingResponse>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bookings");
                return StatusCode(500, ApiResponse<List<BookingResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<BookingStatsResponse>>> GetStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var stats = await _bookingService.GetStatsAsync(fromDate, toDate);
                return Ok(ApiResponse<BookingStatsResponse>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking stats");
                return StatusCode(500, ApiResponse<BookingStatsResponse>.FailureResult("Internal server error"));
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransitAFC.Services.Ticket.API.Services;
using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Ticket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> CreateTicket([FromBody] CreateTicketRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.CreateTicketAsync(userId, request);
                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket, "Ticket created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TicketResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{ticketId}")]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> GetTicket(Guid ticketId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.GetTicketAsync(ticketId, userId);
                if (ticket == null)
                {
                    return NotFound(ApiResponse<TicketResponse>.FailureResult("Ticket not found"));
                }

                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("number/{ticketNumber}")]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> GetTicketByNumber(string ticketNumber)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.GetTicketByNumberAsync(ticketNumber, userId);
                if (ticket == null)
                {
                    return NotFound(ApiResponse<TicketResponse>.FailureResult("Ticket not found"));
                }

                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketNumber}", ticketNumber);
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> GetTicketByBooking(Guid bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.GetTicketByBookingIdAsync(bookingId, userId);
                if (ticket == null)
                {
                    return NotFound(ApiResponse<TicketResponse>.FailureResult("Ticket not found"));
                }

                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket for booking {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("my-tickets")]
        public async Task<ActionResult<ApiResponse<List<TicketResponse>>>> GetMyTickets([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<List<TicketResponse>>.FailureResult("Invalid user ID"));
                }

                var tickets = await _ticketService.GetUserTicketsAsync(userId, skip, take);
                return Ok(ApiResponse<List<TicketResponse>>.SuccessResult(tickets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tickets");
                return StatusCode(500, ApiResponse<List<TicketResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{ticketId}/activate")]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> ActivateTicket(Guid ticketId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.ActivateTicketAsync(ticketId, userId);
                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket, "Ticket activated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TicketResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{ticketId}/cancel")]
        public async Task<ActionResult<ApiResponse<TicketResponse>>> CancelTicket(Guid ticketId, [FromBody] CancelTicketRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketResponse>.FailureResult("Invalid user ID"));
                }

                var ticket = await _ticketService.CancelTicketAsync(ticketId, userId, request);
                return Ok(ApiResponse<TicketResponse>.SuccessResult(ticket, "Ticket cancelled successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TicketResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<TicketResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("{ticketId}/regenerate-qr")]
        public async Task<ActionResult<ApiResponse<TicketQRCodeResponse>>> RegenerateQRCode(Guid ticketId, [FromBody] RegenerateQRRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<TicketQRCodeResponse>.FailureResult("Invalid user ID"));
                }

                var qrCode = await _ticketService.RegenerateQRCodeAsync(ticketId, userId, request);
                return Ok(ApiResponse<TicketQRCodeResponse>.SuccessResult(qrCode, "QR code regenerated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TicketQRCodeResponse>.FailureResult(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating QR code for ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<TicketQRCodeResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{ticketId}/qr-code")]
        public async Task<IActionResult> GetQRCodeImage(Guid ticketId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest("Invalid user ID");
                }

                var qrImageBytes = await _ticketService.GetQRCodeImageAsync(ticketId, userId);
                return File(qrImageBytes, "image/png");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QR code image for ticket {TicketId}", ticketId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{ticketId}/transfer")]
        public async Task<ActionResult<ApiResponse<TicketTransferResponse>>> ProcessTransfer(Guid ticketId, [FromBody] TicketTransferRequest request)
        {
            try
            {
                var transfer = await _ticketService.ProcessTransferAsync(ticketId, request);
                return Ok(ApiResponse<TicketTransferResponse>.SuccessResult(transfer, "Transfer processed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TicketTransferResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer for ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<TicketTransferResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<TicketResponse>>>> SearchTickets([FromQuery] TicketSearchRequest request)
        {
            try
            {
                var tickets = await _ticketService.SearchTicketsAsync(request);
                return Ok(ApiResponse<List<TicketResponse>>.SuccessResult(tickets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets");
                return StatusCode(500, ApiResponse<List<TicketResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TicketStatsResponse>>> GetStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var stats = await _ticketService.GetStatsAsync(fromDate, toDate);
                return Ok(ApiResponse<TicketStatsResponse>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket stats");
                return StatusCode(500, ApiResponse<TicketStatsResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("bulk-operation")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<TicketResponse>>>> BulkOperation([FromBody] BulkTicketOperation operation)
        {
            try
            {
                var results = await _ticketService.BulkOperationAsync(operation);
                return Ok(ApiResponse<List<TicketResponse>>.SuccessResult(results, "Bulk operation completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation");
                return StatusCode(500, ApiResponse<List<TicketResponse>>.FailureResult("Internal server error"));
            }
        }
    }
}
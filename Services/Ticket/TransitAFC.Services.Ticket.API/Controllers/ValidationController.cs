using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitAFC.Services.Ticket.API.Services;
using TransitAFC.Services.Ticket.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Ticket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ValidationController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<ValidationController> _logger;

        public ValidationController(ITicketService ticketService, ILogger<ValidationController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<TicketValidationResult>>> ValidateTicket([FromBody] ValidateTicketRequest request)
        {
            try
            {
                var result = await _ticketService.ValidateTicketAsync(request);

                var message = result.IsValid ? "Ticket validated successfully" : result.Message;
                var apiResponse = result.IsValid
                    ? ApiResponse<TicketValidationResult>.SuccessResult(result, message)
                    : ApiResponse<TicketValidationResult>.FailureResult(message);

                apiResponse.Data = result;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ticket");
                return StatusCode(500, ApiResponse<TicketValidationResult>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("bulk-validate")]
        [Authorize(Roles = "Operator,Admin")]
        public async Task<ActionResult<ApiResponse<List<TicketValidationResult>>>> BulkValidateTickets([FromBody] List<ValidateTicketRequest> requests)
        {
            try
            {
                var results = new List<TicketValidationResult>();

                foreach (var request in requests)
                {
                    try
                    {
                        var result = await _ticketService.ValidateTicketAsync(request);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error validating ticket in bulk operation");
                        results.Add(new TicketValidationResult
                        {
                            IsValid = false,
                            Result = Core.Models.ValidationResult.Invalid,
                            Message = "Validation error occurred",
                            ValidationTime = DateTime.UtcNow
                        });
                    }
                }

                return Ok(ApiResponse<List<TicketValidationResult>>.SuccessResult(results, "Bulk validation completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk ticket validation");
                return StatusCode(500, ApiResponse<List<TicketValidationResult>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("can-use/{ticketId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CanUseTicket(Guid ticketId)
        {
            try
            {
                var canUse = await _ticketService.CanUseTicketAsync(ticketId);
                return Ok(ApiResponse<bool>.SuccessResult(canUse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if ticket can be used");
                return StatusCode(500, ApiResponse<bool>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("remaining-usage/{ticketId}")]
        public async Task<ActionResult<ApiResponse<int>>> GetRemainingUsage(Guid ticketId)
        {
            try
            {
                var remainingUsage = await _ticketService.GetRemainingUsageAsync(ticketId);
                return Ok(ApiResponse<int>.SuccessResult(remainingUsage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining usage for ticket");
                return StatusCode(500, ApiResponse<int>.FailureResult("Internal server error"));
            }
        }
    }
}
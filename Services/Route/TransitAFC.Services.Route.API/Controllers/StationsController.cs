using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitAFC.Services.Route.API.Services;
using TransitAFC.Services.Route.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Route.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;
        private readonly ILogger<StationsController> _logger;

        public StationsController(IStationService stationService, ILogger<StationsController> logger)
        {
            _stationService = stationService;
            _logger = logger;
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<ApiResponse<List<StationInfo>>>> GetNearbyStations([FromQuery] NearbyStationsRequest request)
        {
            try
            {
                var response = await _stationService.GetNearbyStationsAsync(request);
                return Ok(ApiResponse<List<StationInfo>>.SuccessResult(response, "Nearby stations found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby stations");
                return StatusCode(500, ApiResponse<List<StationInfo>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<StationResponse>>>> SearchStations([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<List<StationResponse>>.FailureResult("Search term is required"));
                }

                var response = await _stationService.SearchStationsAsync(searchTerm);
                return Ok(ApiResponse<List<StationResponse>>.SuccessResult(response, "Stations found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching stations");
                return StatusCode(500, ApiResponse<List<StationResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{stationCode}")]
        public async Task<ActionResult<ApiResponse<StationResponse>>> GetStation(string stationCode)
        {
            try
            {
                var response = await _stationService.GetStationByCodeAsync(stationCode);
                if (response == null)
                {
                    return NotFound(ApiResponse<StationResponse>.FailureResult("Station not found"));
                }

                return Ok(ApiResponse<StationResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting station {StationCode}", stationCode);
                return StatusCode(500, ApiResponse<StationResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StationResponse>>>> GetAllStations([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var response = await _stationService.GetAllStationsAsync(skip, take);
                return Ok(ApiResponse<List<StationResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stations");
                return StatusCode(500, ApiResponse<List<StationResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("by-transport-mode/{transportModeId}")]
        public async Task<ActionResult<ApiResponse<List<StationResponse>>>> GetStationsByTransportMode(Guid transportModeId)
        {
            try
            {
                var response = await _stationService.GetStationsByTransportModeAsync(transportModeId);
                return Ok(ApiResponse<List<StationResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stations by transport mode {TransportModeId}", transportModeId);
                return StatusCode(500, ApiResponse<List<StationResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<StationResponse>>> CreateStation([FromBody] CreateStationRequest request)
        {
            try
            {
                var response = await _stationService.CreateStationAsync(request);
                return CreatedAtAction(
                    nameof(GetStation),
                    new { stationCode = response.Code },
                    ApiResponse<StationResponse>.SuccessResult(response, "Station created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<StationResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating station");
                return StatusCode(500, ApiResponse<StationResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPut("{stationId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<StationResponse>>> UpdateStation(Guid stationId, [FromBody] UpdateStationRequest request)
        {
            try
            {
                var response = await _stationService.UpdateStationAsync(stationId, request);
                return Ok(ApiResponse<StationResponse>.SuccessResult(response, "Station updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<StationResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating station {StationId}", stationId);
                return StatusCode(500, ApiResponse<StationResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpDelete("{stationId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteStation(Guid stationId)
        {
            try
            {
                var result = await _stationService.DeleteStationAsync(stationId);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.FailureResult("Station not found"));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Station deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting station {StationId}", stationId);
                return StatusCode(500, ApiResponse<bool>.FailureResult("Internal server error"));
            }
        }
    }
}
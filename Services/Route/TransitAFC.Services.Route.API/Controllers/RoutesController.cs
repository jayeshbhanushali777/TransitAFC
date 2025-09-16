using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitAFC.Services.Route.API.Services;
using TransitAFC.Services.Route.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.Route.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly ILogger<RoutesController> _logger;

        public RoutesController(IRouteService routeService, ILogger<RoutesController> logger)
        {
            _routeService = routeService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<RouteSearchResponse>>> SearchRoutes([FromQuery] RouteSearchRequest request)
        {
            try
            {
                var response = await _routeService.SearchRoutesAsync(request);
                return Ok(ApiResponse<RouteSearchResponse>.SuccessResult(response, "Routes found successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RouteSearchResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching routes");
                return StatusCode(500, ApiResponse<RouteSearchResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("smart-search")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RouteSearchResponse>>> SmartRouteSearch([FromQuery] SmartRouteRequest request)
        {
            try
            {
                var response = await _routeService.SmartRouteSearchAsync(request);
                return Ok(ApiResponse<RouteSearchResponse>.SuccessResult(response, "Smart routes found successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RouteSearchResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in smart route search");
                return StatusCode(500, ApiResponse<RouteSearchResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("{routeCode}")]
        public async Task<ActionResult<ApiResponse<RouteDetailsResponse>>> GetRouteDetails(string routeCode, [FromQuery] DateTime? date = null)
        {
            try
            {
                var response = await _routeService.GetRouteDetailsAsync(routeCode, date);
                if (response == null)
                {
                    return NotFound(ApiResponse<RouteDetailsResponse>.FailureResult("Route not found"));
                }

                return Ok(ApiResponse<RouteDetailsResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route details for {RouteCode}", routeCode);
                return StatusCode(500, ApiResponse<RouteDetailsResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RouteDetailsResponse>>>> GetAllRoutes([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var response = await _routeService.GetAllRoutesAsync(skip, take);
                return Ok(ApiResponse<List<RouteDetailsResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all routes");
                return StatusCode(500, ApiResponse<List<RouteDetailsResponse>>.FailureResult("Internal server error"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<RouteDetailsResponse>>> CreateRoute([FromBody] CreateRouteRequest request)
        {
            try
            {
                var response = await _routeService.CreateRouteAsync(request);
                return Ok(ApiResponse<RouteDetailsResponse>.SuccessResult(response, "Route created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RouteDetailsResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                return StatusCode(500, ApiResponse<RouteDetailsResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPut("{routeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<RouteDetailsResponse>>> UpdateRoute(Guid routeId, [FromBody] UpdateRouteRequest request)
        {
            try
            {
                var response = await _routeService.UpdateRouteAsync(routeId, request);
                return Ok(ApiResponse<RouteDetailsResponse>.SuccessResult(response, "Route updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RouteDetailsResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route {RouteId}", routeId);
                return StatusCode(500, ApiResponse<RouteDetailsResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpDelete("{routeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRoute(Guid routeId)
        {
            try
            {
                var result = await _routeService.DeleteRouteAsync(routeId);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Route deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route {RouteId}", routeId);
                return StatusCode(500, ApiResponse<bool>.FailureResult("Internal server error"));
            }
        }
    }
}
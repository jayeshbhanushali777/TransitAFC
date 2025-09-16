using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransitAFC.Services.User.API.Services;
using TransitAFC.Services.User.Core.DTOs;
using TransitAFC.Shared.Common.DTOs;

namespace TransitAFC.Services.User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                var user = await _userService.RegisterAsync(request);
                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "User registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _userService.LoginAsync(request);
                return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<LoginResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<UserResponse>.FailureResult("Invalid user ID"));
                }

                var user = await _userService.GetProfileAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponse>.FailureResult("User not found"));
                }

                return Ok(ApiResponse<UserResponse>.SuccessResult(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<UserResponse>.FailureResult("Invalid user ID"));
                }

                var user = await _userService.UpdateProfileAsync(userId, request);
                return Ok(ApiResponse<UserResponse>.SuccessResult(user, "Profile updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponse>.FailureResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserResponse>.FailureResult("Internal server error"));
            }
        }

        [HttpDelete("account")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAccount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(ApiResponse<bool>.FailureResult("Invalid user ID"));
                }

                var result = await _userService.DeleteAccountAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.FailureResult("Internal server error"));
            }
        }
    }
}
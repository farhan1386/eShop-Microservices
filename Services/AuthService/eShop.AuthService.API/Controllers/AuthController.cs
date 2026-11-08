using eShop.AuthService.BusinessLogic.Interfaces;
using eShop.AuthService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _authService.RegisterAsync(request);
                if (!success)
                {
                    return BadRequest(new { error = "Registration execution failed." });
                }

                return Ok(new { message = "User credentials provisioned successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            if (response is null)
            {
                return Unauthorized(new { error = "Invalid user credentials or account lockout triggered." });
            }

            return Ok(response);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RefreshTokenAsync(request);
            if (response is null)
            {
                return Unauthorized(new { error = "Invalid or expired rotation refresh token structural context." });
            }

            return Ok(response);
        }

        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
        {
            var success = await _authService.RevokeTokenAsync(request.RefreshToken);
            if (!success)
            {
                return NotFound(new { error = "Target context refresh token reference not verified." });
            }

            return NoContent();
        }

        [HttpPost("roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
            var role = await _authService.CreateRoleAsync(request);
            return Ok(role);
        }

        [HttpPost("users/assign-role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request)
        {
            await _authService.AssignRoleToUserAsync(request);
            return NoContent();
        }

        [HttpGet("users/{id:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            var roles = await _authService.GetUserRolesAsync(id);
            return Ok(roles);
        }
    }
}
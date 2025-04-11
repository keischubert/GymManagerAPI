using System.Security.Claims;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;
        private readonly JwtService jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            this.authService = authService;
            this.jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO userLoginDTO)
        {
            var result = await authService.AuthenticateUserAsync(userLoginDTO);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var jwtToken = result.Data.AccessToken;

            // Set new refresh token cookie
            Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            });

            return Ok(jwtToken);
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var refreshToken = Request.Cookies["refreshToken"];

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid cast of user id");
            }

            var result = await authService.RevocateRefreshTokenAsync(refreshToken);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return NoContent();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            var clientRefreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(clientRefreshToken))
            {
                return Unauthorized("Expired refresh token");
            }

            var refreshToken = await authService.GetRefreshToken(clientRefreshToken);

            if (refreshToken == null || refreshToken.ExpirationDate < DateTime.UtcNow || refreshToken.IsRevoked)
            {
                return BadRequest("Invalid refresh token");
            }

            var jwtToken = jwtService.GenerateAccessToken(refreshToken.User);

            return Ok(new { Message = "User relogged in successfully.", JwtToken = jwtToken });
        }
    }
}

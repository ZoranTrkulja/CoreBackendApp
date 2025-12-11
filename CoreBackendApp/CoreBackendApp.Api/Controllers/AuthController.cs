using CoreBackendApp.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CoreBackendApp.Api.Controllers
{
    [ApiController]
    [Route("api/controller")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}

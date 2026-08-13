using Helpdesk.Api.Dtos;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register(
            [FromBody] RegisterDto dto,
            CancellationToken ct)
        {
            var result = await _auth.RegisterAsync(dto.Email, dto.Password, dto.FullName, ct);

            if (result.Auth is null)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);

                return ValidationProblem(ModelState);
            }

            // 201 without a Location header: a user was created, but there is no
            // endpoint that serves one, so there is nothing to point at.
            return StatusCode(StatusCodes.Status201Created, result.Auth.ToAuthResponseDto());
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginDto dto,
            CancellationToken ct)
        {
            var auth = await _auth.LoginAsync(dto.Email, dto.Password, ct);

            // One message for both "no such account" and "wrong password", so the
            // response cannot be used to work out which emails are registered.
            if (auth is null)
                return Problem(
                    detail: "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized);

            return Ok(auth.ToAuthResponseDto());
        }
    }
}

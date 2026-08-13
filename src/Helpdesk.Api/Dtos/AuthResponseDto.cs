using Helpdesk.Domain.Enums;
using Helpdesk.Infrastructure.Services.Interfaces;

namespace Helpdesk.Api.Dtos
{
    // What register and login return
    public record AuthResponseDto(
        string Token,
        DateTime ExpiresAtUtc,
        int UserId,
        string Email,
        string FullName,
        UserRole Role);

    public static class AuthMappings
    {
        public static AuthResponseDto ToAuthResponseDto(this AuthResult r) => new(
            r.Token, 
            r.ExpiresAtUtc, 
            r.User.Id, 
            r.User.Email!, 
            r.User.FullName, 
            r.User.Role);

    }
}

using Helpdesk.Domain.Enums;
using Helpdesk.Domain.Entities;

namespace Helpdesk.Infrastructure.Services.Interfaces
{
    public sealed record AuthResult(AppUser User, string Token, DateTime ExpiresAtUtc);

    public sealed record RegisterResult(AuthResult? Auth, IReadOnlyList<string> Errors);

    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(
            string email,
            string password,
            string fullName,
            CancellationToken ct = default);
            
        Task<AuthResult?> LoginAsync(string email, string password, CancellationToken ct = default);
    }
}

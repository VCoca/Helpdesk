using System.Security.Claims;
using System.Text;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Helpdesk.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _users;
        private readonly JwtOptions _jwt;

        public AuthService(UserManager<AppUser> users, IOptions<JwtOptions> jwt)
        {
            _users = users;
            _jwt = jwt.Value;
        }

        public async Task<RegisterResult> RegisterAsync(
            string email,
            string password,
            string fullName,
            CancellationToken ct = default)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,

                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
            };

            var created = await _users.CreateAsync(user, password);

            if (!created.Succeeded)
                return new RegisterResult(null, created.Errors.Select(e => e.Description).ToList());

            await _users.AddToRoleAsync(user, UserRole.User.ToString());

            return new RegisterResult(CreateToken(user), []);
        }

        public async Task<AuthResult?> LoginAsync(
            string email,
            string password,
            CancellationToken ct = default)
        {
            var user = await _users.FindByEmailAsync(email);

            if (user is null)
                return null;

            if (!await _users.CheckPasswordAsync(user, password))
                return null;

            return CreateToken(user);
        }

        private AuthResult CreateToken(AppUser user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);

            return new AuthResult(
                user,
                token,
                expiresAt);
        }
    }
}

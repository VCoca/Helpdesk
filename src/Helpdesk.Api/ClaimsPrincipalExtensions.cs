using Helpdesk.Domain.Enums;
using System.Security.Claims;

namespace Helpdesk.Api
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            if (!int.TryParse(value, out var id))
                throw new InvalidOperationException("Authenticated request has no usable user id claim.");

            return id;
        }

        public static bool IsAgent(this ClaimsPrincipal user) =>
            user.IsInRole(nameof(UserRole.Agent));
    }
}

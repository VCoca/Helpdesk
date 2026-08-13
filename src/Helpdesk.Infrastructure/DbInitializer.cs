using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure
{
    public static class DbInitializer
    {
        public const string SeedPassword = "Password123!";

        public static async Task SeedAsync(
            HelpdeskDbContext db,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            CancellationToken ct = default)
        {
            if (await db.Tickets.AnyAsync(ct))
                return;

            await SeedRolesAsync(roleManager);

            var agents = new[]
            {
                await CreateUserAsync(userManager, "ana.kovac@helpdesk.local", "Ana Kovac", UserRole.Agent),
                await CreateUserAsync(userManager, "marko.ilic@helpdesk.local", "Marko Ilic", UserRole.Agent),
            };

            var users = new[]
            {
                await CreateUserAsync(userManager, "jelena.popovic@helpdesk.local", "Jelena Popovic", UserRole.User),
                await CreateUserAsync(userManager, "nikola.savic@helpdesk.local", "Nikola Savic", UserRole.User),
                await CreateUserAsync(userManager, "petar.ristic@helpdesk.local", "Petar Ristic", UserRole.User),
                await CreateUserAsync(userManager, "milica.todorovic@helpdesk.local", "Milica Todorovic", UserRole.User),
            };

            var now = DateTime.UtcNow;

            var tickets = Seeds
                .Select(s => BuildTicket(s, agents, users, now))
                .ToList();

            db.Tickets.AddRange(tickets);
            await db.SaveChangesAsync(ct);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in Enum.GetNames<UserRole>())
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        private static async Task<AppUser> CreateUserAsync(
            UserManager<AppUser> userManager,
            string email,
            string fullName,
            UserRole role)
        {
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user, SeedPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not seed user {email}: {errors}");
            }

            await userManager.AddToRoleAsync(user, role.ToString());

            return user;
        }

        private static Ticket BuildTicket(TicketSeed s, AppUser[] agents, AppUser[] users, DateTime now)
        {
            var createdAt = now.AddDays(-s.DaysAgo).AddHours(-s.HoursAgo);

            var isTerminal = s.Status is TicketStatus.Closed or TicketStatus.Rejected;
            var updatedAt = s.Status == TicketStatus.New
                ? createdAt
                : createdAt.AddHours(s.DaysAgo > 0 ? 6 : 1);

            return new Ticket
            {
                Title = s.Title,
                Description = s.Description,
                Status = s.Status,
                Priority = s.Priority,
                CategoryId = s.CategoryId,
                AuthorId = users[s.AuthorIndex].Id,
                AssignedAgentId = s.AgentIndex is int i ? agents[i].Id : null,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ClosedAt = isTerminal ? updatedAt : null,
            };
        }

        private sealed record TicketSeed(
            string Title,
            string Description,
            TicketStatus Status,
            Priority Priority,
            int CategoryId,
            int AuthorIndex,
            int? AgentIndex,
            int DaysAgo,
            int HoursAgo = 0);

        private static readonly TicketSeed[] Seeds =
        [
            new("Printer on the third floor is offline",
                "The shared printer shows a red light and nothing comes out. Restarting it did not help.",
                TicketStatus.New, Priority.Medium, 1, 0, null, 0, 2),

            new("Laptop battery drains in under an hour",
                "Battery health reads 61%. The laptop is about three years old.",
                TicketStatus.New, Priority.Low, 1, 1, null, 0, 5),

            new("Cannot connect to the office VPN",
                "Authentication succeeds but the tunnel drops after roughly ten seconds.",
                TicketStatus.New, Priority.High, 3, 2, null, 1, 3),

            new("Second monitor not detected",
                "Display works on another machine, so it is likely the dock or the cable.",
                TicketStatus.New, Priority.Low, 1, 3, null, 1, 8),

            new("Request access to the reporting dashboard",
                "I need read access to the sales reporting dashboard for the quarterly review.",
                TicketStatus.New, Priority.Medium, 4, 0, null, 2),

            new("Outlook keeps asking for my password",
                "The prompt reappears every few minutes even after entering the correct password.",
                TicketStatus.InProgress, Priority.High, 2, 1, 0, 2, 4),

            new("Shared drive is extremely slow",
                "Opening a 2 MB spreadsheet from the shared drive takes almost a minute.",
                TicketStatus.InProgress, Priority.Medium, 3, 2, 1, 3),

            new("Keyboard keys sticking",
                "The E and R keys need a hard press. Cleaning underneath did not fix it.",
                TicketStatus.InProgress, Priority.Low, 1, 3, 0, 3, 6),

            new("Payroll application crashes on export",
                "Exporting to PDF closes the application with no error message.",
                TicketStatus.InProgress, Priority.Critical, 2, 0, 1, 4),

            new("New starter needs an account",
                "A new colleague joins on Monday and needs email plus access to the project folder.",
                TicketStatus.InProgress, Priority.High, 4, 1, 0, 4, 7),

            new("Wi-Fi drops in the east meeting room",
                "The connection drops roughly every fifteen minutes during calls in that room only.",
                TicketStatus.InProgress, Priority.Medium, 3, 2, 1, 5),

            new("Cannot install the design software",
                "The installer reports that administrator rights are required.",
                TicketStatus.Resolved, Priority.Medium, 2, 3, 0, 6),

            new("Mouse double-clicks on a single click",
                "Replaced with a spare mouse from the store cupboard.",
                TicketStatus.Resolved, Priority.Low, 1, 0, 1, 7, 3),

            new("Email attachments over 10 MB are rejected",
                "Advised to use the file share for large attachments; limit is working as designed.",
                TicketStatus.Resolved, Priority.Medium, 2, 1, 0, 8),

            new("Locked out after too many password attempts",
                "Account unlocked and the password reset.",
                TicketStatus.Resolved, Priority.High, 4, 2, 1, 9, 5),

            new("Docking station does not charge the laptop",
                "Faulty power brick swapped out.",
                TicketStatus.Resolved, Priority.High, 1, 3, 0, 10),

            new("Screen flickers when brightness changes",
                "Graphics driver updated and the flicker stopped.",
                TicketStatus.Closed, Priority.Medium, 1, 0, 1, 12),

            new("Cannot open shared calendar",
                "Permissions were missing on the calendar; access granted and confirmed working.",
                TicketStatus.Closed, Priority.Low, 2, 1, 0, 14, 4),

            new("Phone system does not transfer calls",
                "Handset firmware was out of date. Updated and tested with the reception desk.",
                TicketStatus.Closed, Priority.High, 3, 2, 1, 16),

            new("Request a second monitor",
                "Monitor issued from stock and set up at the desk.",
                TicketStatus.Closed, Priority.Low, 1, 3, 0, 18, 6),

            new("Access to the finance folder",
                "Approved by the finance manager and access granted.",
                TicketStatus.Closed, Priority.Medium, 4, 0, 1, 20),

            new("Backup software reports a failed job",
                "Backup target was full. Old snapshots pruned and the job now completes.",
                TicketStatus.Closed, Priority.Critical, 2, 1, 0, 22, 2),

            new("Install a personal game on my work laptop",
                "Not permitted by the acceptable use policy.",
                TicketStatus.Rejected, Priority.Low, 5, 2, 1, 11),

            new("Admin rights on my workstation",
                "Local administrator rights are not granted to standard accounts. Raise a specific request instead.",
                TicketStatus.Rejected, Priority.Medium, 4, 3, 0, 13, 5),

            new("Reset a colleague's password for them",
                "Password resets must be requested by the account holder.",
                TicketStatus.Rejected, Priority.Medium, 4, 0, 1, 15),
        ];
    }
}

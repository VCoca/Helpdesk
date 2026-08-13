namespace Helpdesk.Api.Dtos
{
    /// <summary>
    /// Self-registration always creates a User. Role is deliberately absent —
    /// accepting it here would let anyone make themselves an Agent.
    /// </summary>
    public record RegisterDto(string Email, string Password, string FullName);
}

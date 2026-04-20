namespace PersonalFinance.Shared.Events.Events
{
    /// <summary>
    /// Triggered when a user updates their Gmail connection or refreshes tokens.
    /// </summary>
    public class UserGmailTokensUpdatedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Triggered when a user is deleted, requiring cleanup of their data in other services.
    /// </summary>
    public class UserDeletedEvent
    {
        public Guid UserId { get; set; }
    }
}
    
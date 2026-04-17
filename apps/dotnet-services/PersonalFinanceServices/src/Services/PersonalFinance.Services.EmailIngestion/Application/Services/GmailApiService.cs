using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

using MimeKit;

namespace PersonalFinance.Services.EmailIngestion.Application.Services
{
    public interface IGmailApiService
    {
        /// <summary>
        /// Fetches emails from Gmail matching the given query after the specified timestamp.
        /// </summary>
        Task<List<GmailEmailMessage>> FetchEmailsAsync(string accessToken, string refreshToken,
            string? query = null, DateTime? after = null, int maxResults = 50, CancellationToken ct = default);

        /// <summary>
        /// Refreshes a Gmail access token using the refresh token.
        /// </summary>
        Task<(string newAccessToken, DateTime expiresAt)> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default);
    }

    public class GmailEmailMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string ThreadId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class GmailApiService : IGmailApiService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailApiService> _logger;

        public GmailApiService(IConfiguration configuration, ILogger<GmailApiService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<GmailEmailMessage>> FetchEmailsAsync(string accessToken, string refreshToken,
            string? query = null, DateTime? after = null, int maxResults = 50, CancellationToken ct = default)
        {
            var results = new List<GmailEmailMessage>();

            try
            {
                var service = CreateGmailService(accessToken, refreshToken);

                // Build query string
                var queryParts = new List<string>();
                if (!string.IsNullOrEmpty(query))
                    queryParts.Add(query);
                if (after.HasValue)
                    queryParts.Add($"after:{after.Value:yyyy/MM/dd}");

                var fullQuery = string.Join(" ", queryParts);

                _logger.LogInformation("Fetching Gmail messages with query: {Query}, maxResults: {MaxResults}", fullQuery, maxResults);

                var listRequest = service.Users.Messages.List("me");
                listRequest.Q = fullQuery;
                listRequest.MaxResults = maxResults;
                listRequest.LabelIds = "INBOX";

                var listResponse = await listRequest.ExecuteAsync(ct);

                if (listResponse.Messages == null || !listResponse.Messages.Any())
                {
                    _logger.LogInformation("No new messages found matching query");
                    return results;
                }

                _logger.LogInformation("Found {Count} messages to process", listResponse.Messages.Count);

                foreach (var messageRef in listResponse.Messages)
                {
                    try
                    {
                        var getRequest = service.Users.Messages.Get("me", messageRef.Id);
                        getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;

                        var message = await getRequest.ExecuteAsync(ct);
                        var emailMessage = ParseRawMessage(message);

                        if (emailMessage != null)
                            results.Add(emailMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch message {MessageId}", messageRef.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Gmail messages");
                throw;
            }

            return results;
        }

        public async Task<(string newAccessToken, DateTime expiresAt)> RefreshAccessTokenAsync(
            string refreshToken, CancellationToken ct = default)
        {
            var clientId = _configuration["GoogleSettings:ClientId"]
                ?? throw new InvalidOperationException("GoogleSettings:ClientId not configured");
            var clientSecret = _configuration["GoogleSettings:ClientSecret"]
                ?? throw new InvalidOperationException("GoogleSettings:ClientSecret not configured");

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { GmailService.Scope.GmailReadonly }
            });

            var tokenResponse = await flow.RefreshTokenAsync("user", refreshToken, ct);

            var expiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);

            _logger.LogInformation("Gmail access token refreshed, expires at {ExpiresAt}", expiresAt);

            return (tokenResponse.AccessToken, expiresAt);
        }

        private GmailService CreateGmailService(string accessToken, string refreshToken)
        {
            var clientId = _configuration["GoogleSettings:ClientId"] ?? "";
            var clientSecret = _configuration["GoogleSettings:ClientSecret"] ?? "";

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { GmailService.Scope.GmailReadonly }
            });

            var credential = new UserCredential(flow, "user", tokenResponse);

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PersonalFinance EmailIngestion"
            });
        }

        private GmailEmailMessage? ParseRawMessage(Message message)
        {
            try
            {
                if (string.IsNullOrEmpty(message.Raw)) return null;

                // Decode base64url-encoded raw message
                var rawBytes = Convert.FromBase64String(
                    message.Raw.Replace('-', '+').Replace('_', '/'));

                using var stream = new MemoryStream(rawBytes);
                var mimeMessage = MimeMessage.Load(stream);

                return new GmailEmailMessage
                {
                    MessageId = message.Id,
                    ThreadId = message.ThreadId,
                    Subject = mimeMessage.Subject ?? string.Empty,
                    SenderEmail = mimeMessage.From?.Mailboxes?.FirstOrDefault()?.Address ?? string.Empty,
                    Body = mimeMessage.TextBody ?? string.Empty,
                    HtmlBody = mimeMessage.HtmlBody ?? string.Empty,
                    Date = mimeMessage.Date.UtcDateTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse raw message {MessageId}", message.Id);
                return null;
            }
        }
    }
}

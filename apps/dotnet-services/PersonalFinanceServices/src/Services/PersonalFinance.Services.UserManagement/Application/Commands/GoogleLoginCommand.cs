using AutoMapper;

using Google.Apis.Auth;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Domain.Entities;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class GoogleLoginCommand : IRequest<ApiResponse<LoginResponse>>
    {
        public string AuthorizationCode { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class GoogleLoginCommandHandler : BaseCommandHandler<GoogleLoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public GoogleLoginCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<GoogleLoginCommandHandler> logger,
            IPublishEndpoint publishEndpoint) : base(context, logger, mapper, passwordHasher)
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<ApiResponse<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Attempting Google login via authorization code");

                var clientId = _configuration["GoogleSettings:ClientId"] ?? "";
                var clientSecret = _configuration["GoogleSettings:ClientSecret"] ?? "";
                var redirectUri = _configuration["GoogleSettings:RedirectUri"] ?? "postmessage";

                var flow = new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow(
                    new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        Scopes = new[] { "openid", "email", "profile", "https://www.googleapis.com/auth/gmail.readonly" }
                    });

                var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", request.AuthorizationCode, redirectUri, cancellationToken);
                
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Failed to exchange authorization code for tokens.");
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken, settings);

                if (payload == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid Google token payload.");
                }

                var user = await Context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email.Value == payload.Email.ToLower() || u.GoogleId == payload.Subject, cancellationToken);

                bool isNewUser = false;

                if (user == null)
                {
                    // Register new user
                    isNewUser = true;
                    user = new User(payload.Email, payload.Email, payload.GivenName ?? "", payload.FamilyName ?? "");
                    user.SetGoogleId(payload.Subject);
                    user.ConfirmEmail();

                    var userRole = await Context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
                    if (userRole != null)
                    {
                        user.AddRole(userRole);
                    }

                    Context.Users.Add(user);
                }
                else if (string.IsNullOrEmpty(user.GoogleId))
                {
                    // Link existing user to Google
                    user.SetGoogleId(payload.Subject);
                }

                // Store or update Gmail Tokens
                if (!string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    var tokenExpiresAt = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
                    // Retain the existing refresh token if Google didn't return a new one
                    var refreshTokenToStore = !string.IsNullOrEmpty(tokenResponse.RefreshToken) 
                        ? tokenResponse.RefreshToken 
                        : user.GmailRefreshToken;
                        
                    user.SetGmailTokens(tokenResponse.AccessToken, refreshTokenToStore ?? "", tokenExpiresAt);

                    // Publish the token update event for other microservices
                    await _publishEndpoint.Publish(new PersonalFinance.Shared.Events.Events.UserGmailTokensUpdatedEvent
                    {
                        UserId = user.Id,
                        Email = user.Email.Value,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = refreshTokenToStore ?? "",
                        ExpiresAt = tokenExpiresAt
                    }, cancellationToken);
                }

                await Context.SaveChangesAsync(cancellationToken);

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var appAccessToken = _tokenService.CreateToken(user, roles);
                var appRefreshToken = _tokenService.GenerateRefreshToken();

                var refreshTokenEntity = new Domain.Entities.RefreshToken(appRefreshToken, DateTime.UtcNow.AddDays(7), user.Id, request.IpAddress);
                Context.RefreshTokens.Add(refreshTokenEntity);
                await Context.SaveChangesAsync(cancellationToken);

                var loginResponse = new LoginResponse
                {
                    User = user.ToDto(Mapper),
                    AccessToken = appAccessToken,
                    RefreshToken = appRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "5")),
                    Permissions = roles
                };

                Logger.LogInformation("Google login successful for email: {Email}", payload.Email);

                return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during Google login");
                return ApiResponse<LoginResponse>.ErrorResult($"An error occurred during Google login: {ex.Message}");
            }
        }
    }
}

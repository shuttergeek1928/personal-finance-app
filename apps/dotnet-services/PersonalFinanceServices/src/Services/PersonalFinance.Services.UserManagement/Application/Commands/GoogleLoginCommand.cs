using AutoMapper;

using Google.Apis.Auth;

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
        public string IdToken { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class GoogleLoginCommandHandler : BaseCommandHandler<GoogleLoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public GoogleLoginCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<GoogleLoginCommandHandler> logger) : base(context, logger, mapper, passwordHasher)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public override async Task<ApiResponse<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Attempting Google login");

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["GoogleSettings:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                if (payload == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid Google token");
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
                    await Context.SaveChangesAsync(cancellationToken);
                }
                else if (string.IsNullOrEmpty(user.GoogleId))
                {
                    // Link existing user to Google
                    user.SetGoogleId(payload.Subject);
                    await Context.SaveChangesAsync(cancellationToken);
                }

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var accessToken = _tokenService.CreateToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Insert refresh token directly — bypasses User's RowVersion concurrency check
                var refreshTokenEntity = new Domain.Entities.RefreshToken(refreshToken, DateTime.UtcNow.AddDays(7), user.Id, request.IpAddress);
                Context.RefreshTokens.Add(refreshTokenEntity);
                await Context.SaveChangesAsync(cancellationToken);

                var loginResponse = new LoginResponse
                {
                    User = user.ToDto(Mapper),
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
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

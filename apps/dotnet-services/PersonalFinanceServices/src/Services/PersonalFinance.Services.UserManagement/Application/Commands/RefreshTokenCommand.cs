using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class RefreshTokenCommand : IRequest<ApiResponse<LoginResponse>>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class RefreshTokenCommandHandler : BaseCommandHandler<RefreshTokenCommand, ApiResponse<LoginResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<RefreshTokenCommandHandler> logger) : base(context, logger, mapper, passwordHasher)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public override async Task<ApiResponse<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Attempting token refresh");

                var user = await Context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken), cancellationToken);

                if (user == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid refresh token");
                }

                var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

                if (!refreshToken.IsFullyActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Refresh token is inactive or expired");
                }

                // Token Rotation: Revoke current and issue new
                var newRefreshTokenString = _tokenService.GenerateRefreshToken();
                user.RevokeRefreshToken(request.RefreshToken, request.IpAddress, newRefreshTokenString);
                user.AddRefreshToken(newRefreshTokenString, DateTime.UtcNow.AddDays(7), request.IpAddress);
                user.RemoveOldRefreshTokens();

                await Context.SaveChangesAsync(cancellationToken);

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var newAccessToken = _tokenService.CreateToken(user, roles);

                var loginResponse = new LoginResponse
                {
                    User = user.ToDto(Mapper),
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshTokenString,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "5")),
                    Permissions = roles
                };

                Logger.LogInformation("Token refresh successful for email: {Email}", user.Email.Value);

                return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during token refresh");
                return ApiResponse<LoginResponse>.ErrorResult($"An error occurred during token refresh: {ex.Message}");
            }
        }
    }
}

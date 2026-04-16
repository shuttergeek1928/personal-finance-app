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
    public class LoginUserCommand : IRequest<ApiResponse<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginUserCommandHandler : BaseCommandHandler<LoginUserCommand, ApiResponse<LoginResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public LoginUserCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<LoginUserCommandHandler> logger) : base(context, logger, mapper, passwordHasher)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public override async Task<ApiResponse<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Attempting login for email: {Email}", request.Email);

                // Load user as read-only — we only need it for verification and token generation
                var user = await Context.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email.Value == request.Email.ToLower(), cancellationToken);

                if (user == null)
                {
                    Logger.LogWarning("Login failed for email {Email}: User not found", request.Email);
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                if (!await ComparePassword(request.Password, user.PasswordHash, cancellationToken))
                {
                    Logger.LogWarning("Login failed for email {Email}: Invalid password", request.Email);
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var accessToken = _tokenService.CreateToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Insert refresh token directly — bypasses User's RowVersion concurrency check
                var refreshTokenEntity = new Domain.Entities.RefreshToken(refreshToken, DateTime.UtcNow.AddDays(7), user.Id);
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

                Logger.LogInformation("Login successful for email: {Email}", request.Email);

                return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ApiResponse<LoginResponse>.ErrorResult($"An error occurred during login: {ex.Message}");
            }
        }
    }
}

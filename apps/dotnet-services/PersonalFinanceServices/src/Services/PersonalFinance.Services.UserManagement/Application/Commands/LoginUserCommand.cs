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

        public LoginUserCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<LoginUserCommandHandler> logger) : base(context, logger, mapper, passwordHasher)
        {
            _tokenService = tokenService;
        }

        public override async Task<ApiResponse<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Attempting login for email: {Email}", request.Email);

                var user = await Context.Users
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
                var token = _tokenService.CreateToken(user, roles);

                var loginResponse = new LoginResponse
                {
                    User = user.ToDto(Mapper),
                    AccessToken = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Configured in settings but matching here for response
                    Permissions = roles // In this context, roles are treated as permissions or base for them
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

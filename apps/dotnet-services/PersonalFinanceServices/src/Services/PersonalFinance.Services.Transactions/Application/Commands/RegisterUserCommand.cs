// PersonalFinance.Services.Transactions/Application/Commands/RegisterUserCommand.cs
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Mappings;
using PersonalFinance.Services.Transactions.Application.Services;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;

namespace PersonalFinance.Services.Transactions.Application.Commands
{
    public class RegisterUserCommand : IRequest<ApiResponse<UserTransferObject>>
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class RegisterUserCommandHandler : BaseCommandHandler<RegisterUserCommand, ApiResponse<UserTransferObject>>
    {
        public RegisterUserCommandHandler(
            UserManagementDbContext context,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            ILogger<RegisterUserCommandHandler> logger) : base(context, logger, mapper, passwordHasher)
        {
        }

        public override async Task<ApiResponse<UserTransferObject>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting user registration for email: {Email}", request.Email);

                if (await EmailExistAsync(request.Email))
                {
                    return ApiResponse<UserTransferObject>.ErrorResult("A user with this email address already exists");
                }

                if (await UsernameExistAsync(request.UserName))
                {
                    return ApiResponse<UserTransferObject>.ErrorResult("A username already exist, use some other username");
                }

                // Validate password strength (you can implement more complex rules)
                var validationResults = await IsPasswordValidAsync(request.Password);
                if (!validationResults.IsValid)
                {
                    return ApiResponse<UserTransferObject>.ErrorResult(validationResults.Errors);
                }

                // 2. Create new user entity
                var user = new User(request.Email, request.UserName, request.FirstName, request.LastName);
                user.SetPasswordHash(_passwordHasher.HashPassword(request.Password));

                // 3. Add phone number if provided
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
                }

                // 4. Assign default role
                var userRole = await GetRoleByNameExistAsync("User");
                if (userRole != null)
                {
                    user.AddRole(userRole);
                }

                // 5. Create default profile
                var profile = new UserProfile(user.Id);

                // 6. Save to database
                Context.Users.Add(user);
                Context.UserProfiles.Add(profile);
                await Context.SaveChangesAsync(cancellationToken);

                // 7. Map to DTO and return
                var UserTransferObject = user.ToDto(Mapper);

                Logger.LogInformation("User registered successfully: {Email}", request.Email);

                return ApiResponse<UserTransferObject>.SuccessResult(UserTransferObject, "User registered successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error registering user: {Email}", request.Email);
                return ApiResponse<UserTransferObject>.ErrorResult($"An error occurred while registering the user, {ex.Message}");
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
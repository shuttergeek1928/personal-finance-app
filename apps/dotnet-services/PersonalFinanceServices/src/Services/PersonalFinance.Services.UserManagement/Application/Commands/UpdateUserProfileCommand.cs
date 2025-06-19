using AutoMapper;
using MediatR;
using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class UpdateUserProfileCommand : IRequest<ApiResponse<UserTransferObject>>
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
    }

    public class UpdateUserProfileCommandHandler : BaseRequestHandler<UpdateUserProfileCommand, ApiResponse<UserTransferObject>>
    {
        public UpdateUserProfileCommandHandler(
            UserManagementDbContext context,
            ILogger<UpdateUserProfileCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<UserTransferObject>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Verify the request model
                var validationResult = await VerifyProfileUpdateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<UserTransferObject>.ErrorResult(validationResult.Errors);
                }

                //Find the user exist or not
                var user = await UserExistAsync(request.UserId, cancellationToken: cancellationToken);

                if (user == null)
                {
                    Logger.LogError("User with ID {UserId} not found", request.UserId);
                    return ApiResponse<UserTransferObject>.ErrorResult("User not found");
                }

                //Update user's basic info
                user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);

                //Update or create profile
                if (user.Profile == null)
                {
                    user.CreateProfile(new Domain.Entities.UserProfile(user.Id, request.DateOfBirth, request.Currency));
                    user.RecordLogin();
                }
                else
                {
                    user.Profile.UpdatePreferences(request.Currency, request.TimeZone, request.Language);
                    user.RecordLogin();
                }

                await Context.SaveChangesAsync(cancellationToken);
                
                //Map to DTO and return
                var userResponse = Mapper.Map<UserTransferObject>(user);
                Logger.LogInformation("User profile updated successfully: {UserId}", request.UserId);
                return ApiResponse<UserTransferObject>.SuccessResult(userResponse, "User profile updated successfully");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating user: {UserId} : {Name}", request.UserId, $"{request.FirstName} {request.LastName}");
                return ApiResponse<UserTransferObject>.ErrorResult("An error occurred while updating the user profile.");
            }
            
        }

        private async Task<ValidationResult> VerifyProfileUpdateAsync(UpdateUserProfileCommand request, CancellationToken token)
        {
            var errors = new List<string>();

            if (request.PhoneNumber.Length != 10)
                errors.Add("Phone number must be 10 digits long.");

            return new ValidationResult()
            {
                IsValid = !errors.Any(),
                Errors = errors
            };

        }
    }
}

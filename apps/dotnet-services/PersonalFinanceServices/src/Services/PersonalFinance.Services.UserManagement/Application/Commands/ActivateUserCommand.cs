using AutoMapper;

using MediatR;

using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class ActivateUserCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public ActivateUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }

    public class ActivateUserHandler : BaseRequestHandler<ActivateUserCommand, ApiResponse<bool>>
    {
        public ActivateUserHandler(UserManagementDbContext context, ILogger<ActivateUserHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<bool>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var user = await UserExistAsync(request.UserId, cancellationToken: cancellationToken, ignoreQueryfilter: true);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("User not found");
                }

                user.Activate("User activated by request");
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("User with ID {UserId} activated successfully", request.UserId);
                return ApiResponse<bool>.SuccessResult(true, "User activated successfully");
            }
            catch (Exception ex)
            {
                Logger.LogInformation("User with ID {UserId} not activated!", request.UserId);
                return ApiResponse<bool>.ErrorResult($"An error occurred while activating the user: {ex.Message}");
            }
        }
    }
}

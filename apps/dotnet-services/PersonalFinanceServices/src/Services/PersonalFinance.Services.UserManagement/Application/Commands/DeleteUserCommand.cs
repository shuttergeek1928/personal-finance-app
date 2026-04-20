using AutoMapper;

using MediatR;

using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using MassTransit;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class DeleteUserCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public DeleteUserCommand(Guid userId)
        {
            UserId = userId;
        }
    }

    public class DeleteUserCommandHandler : BaseRequestHandler<DeleteUserCommand, ApiResponse<bool>>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public DeleteUserCommandHandler(UserManagementDbContext context, ILogger<DeleteUserCommandHandler> logger, IMapper mapper, IPublishEndpoint publishEndpoint) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var user = await UserExistAsync(request.UserId, cancellationToken: cancellationToken);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("User not found");
                }

                // Publish the event to other services before deleting the user
                await _publishEndpoint.Publish(new UserDeletedEvent
                {
                    UserId = request.UserId
                }, cancellationToken);

                Context.Users.Remove(user);
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("User with ID {UserId} and all related data deletion triggered successfully", request.UserId);
                return ApiResponse<bool>.SuccessResult(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                Logger.LogInformation("User with ID {UserId} not deleted!", request.UserId);
                return ApiResponse<bool>.ErrorResult($"An error occurred while deleting the user: {ex.Message}");
            }
        }
    }
}

using AutoMapper;
using MediatR;
using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Infrastructure.Data;

namespace PersonalFinance.Services.Transactions.Application.Commands
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
        public DeleteUserCommandHandler(UserManagementDbContext context, ILogger<DeleteUserCommandHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
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

                user.Deactivate("User deleted by request");
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("User with ID {UserId} deleted successfully", request.UserId);
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

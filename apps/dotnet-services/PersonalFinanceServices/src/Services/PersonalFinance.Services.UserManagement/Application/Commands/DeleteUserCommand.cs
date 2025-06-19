using MediatR;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

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

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
    {
        private readonly UserManagementDbContext _context;
        private readonly ILogger<DeleteUserCommandHandler> _logger;
        public DeleteUserCommandHandler(UserManagementDbContext context, ILogger<DeleteUserCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var user = await _context.Users.FindAsync(request.UserId);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("User not found");
                }

                user.Deactivate("User deleted by request");
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User with ID {UserId} deleted successfully", request.UserId);
                return ApiResponse<bool>.SuccessResult(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("User with ID {UserId} not deleted!", request.UserId);
                return ApiResponse<bool>.ErrorResult($"An error occurred while deleting the user: {ex.Message}");
            }
        }
    }
}

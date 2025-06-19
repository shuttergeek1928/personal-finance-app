using MediatR;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class ConfirmEmailCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public ConfirmEmailCommand(Guid userId)
        {
            UserId = userId;
        }
    }

    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, ApiResponse<bool>>
    {
        private readonly UserManagementDbContext _context;
        private ILogger<ConfirmEmailHandler> _logger;
        public ConfirmEmailHandler(UserManagementDbContext context, ILogger<ConfirmEmailHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(ConfirmEmailCommand request, CancellationToken token)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if(user == null)
                return ApiResponse<bool>.ErrorResult("User not found");

            user.ConfirmEmail();
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Email confirmed for user: {UserId}", request.UserId);

            return ApiResponse<bool>.SuccessResult(true, "Email confirmed successfully");
        }
    }
}

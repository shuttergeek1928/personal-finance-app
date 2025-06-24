using AutoMapper;
using MediatR;
using PersonalFinance.Services.UserManagement.Application.Common;
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

    public class ConfirmEmailHandler : BaseRequestHandler<ConfirmEmailCommand, ApiResponse<bool>>
    {
        public ConfirmEmailHandler(UserManagementDbContext context, ILogger<ConfirmEmailHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<bool>> Handle(ConfirmEmailCommand request, CancellationToken token)
        {
            var user = await Context.Users.FindAsync(request.UserId);

            if (user == null)
            {
                Logger.LogError("User with ID {UserId} not found", request.UserId);
                return ApiResponse<bool>.ErrorResult("User not found");
            }

            user.ConfirmEmail();
            await Context.SaveChangesAsync();

            Logger.LogInformation("Email confirmed for user: {UserId}", request.UserId);
            return ApiResponse<bool>.SuccessResult(true, "Email confirmed successfully");
        }
    }
}

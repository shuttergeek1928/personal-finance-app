using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.Common;

namespace PersonalFinance.Services.UserManagement.Application.Queries
{
    public class GetUserByIdQuery : IRequest<ApiResponse<UserTransferObject>>
    {
        public Guid UserId { get; set; }

        public GetUserByIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetUserByIdQueryHandler : BaseQueryHandler<GetUserByIdQuery, ApiResponse<UserTransferObject>>
    {
        public GetUserByIdQueryHandler(UserManagementDbContext context, ILogger<GetUserByIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<UserTransferObject>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await Context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                Logger.LogError("User with ID {UserId} not found", request.UserId);
                return ApiResponse<UserTransferObject>.ErrorResult("User not found");
            }

            var UserTransferObject = user.ToDto(Mapper);
            return ApiResponse<UserTransferObject>.SuccessResult(UserTransferObject);
        }
    }
}
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.UserManagement.Application.Common;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.UserManagement.Application.Queries
{
    public class GetUserByEmailQuery : IRequest<ApiResponse<UserTransferObject>>
    {
        public Email Email { get; set; }
        public GetUserByEmailQuery(Email email)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email), "Email cannot be null");
        }
    }

    public class GetUserByEmailQueryHandler : BaseQueryHandler<GetUserByEmailQuery, ApiResponse<UserTransferObject>>
    {
        public GetUserByEmailQueryHandler(UserManagementDbContext context, ILogger<GetUserByEmailQueryHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
        }
        public override async Task<ApiResponse<UserTransferObject>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await Context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email.Value, cancellationToken);
            if (user == null)
            {
                Logger.LogError("User with email {Email} not found", request.Email.Value);
                return ApiResponse<UserTransferObject>.ErrorResult("User not found");
            }
            var userDto = user.ToDto(Mapper);
            return ApiResponse<UserTransferObject>.SuccessResult(userDto);
        }
    }
}

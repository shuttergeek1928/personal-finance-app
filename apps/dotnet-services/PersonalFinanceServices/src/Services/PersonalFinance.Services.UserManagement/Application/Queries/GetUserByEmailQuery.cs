using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ApiResponse<UserTransferObject>>
    {
        private readonly UserManagementDbContext _context;
        private readonly IMapper _mapper;
        public GetUserByEmailQueryHandler(UserManagementDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "UserManagementDbContext cannot be null");
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper), "Mapper cannot be null");
        }
        public async Task<ApiResponse<UserTransferObject>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email.Value, cancellationToken);
            if (user == null)
            {
                return ApiResponse<UserTransferObject>.ErrorResult("User not found");
            }
            var userDto = user.ToDto(_mapper);
            return ApiResponse<UserTransferObject>.SuccessResult(userDto);
        }
    }
}

using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;

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

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserTransferObject>>
    {
        private readonly UserManagementDbContext _context;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(UserManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UserTransferObject>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserTransferObject>.ErrorResult("User not found");
            }

            var UserTransferObject = user.ToDto(_mapper);
            return ApiResponse<UserTransferObject>.SuccessResult(UserTransferObject);
        }
    }
}
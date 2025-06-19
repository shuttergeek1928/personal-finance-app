using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.UserManagement.Application.Commands;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Queries
{
    public class GetUsersQuery : IRequest<ApiResponse<PaginatedResult<UserTransferObject>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public GetUsersQuery(int pageNumber = 1, int pageSize = 20)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
        }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ApiResponse<PaginatedResult<UserTransferObject>>>
    {
        private readonly UserManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUsersQueryHandler> _logger;

        public GetUsersQueryHandler(UserManagementDbContext context, IMapper mapper, ILogger<GetUsersQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResult<UserTransferObject>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var totalCount = await _context.Users.CountAsync(cancellationToken);

                //Get user with pagination
                var users = await _context.Users
                    .Include(u => u.Profile)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .OrderBy(u => u.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var userDtos = _mapper.Map<List<UserTransferObject>>(users);

                var result = new PaginatedResult<UserTransferObject>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PaginatedResult<UserTransferObject>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return ApiResponse<PaginatedResult<UserTransferObject>>.ErrorResult($"An error occurred while retrieving users: {ex.Message}");
            }
        }
    }
}

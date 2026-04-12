using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;

namespace PersonalFinance.Services.UserManagement.Application.Commands
{
    public class AssignUserRolesCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AssignUserRolesCommandHandler : IRequestHandler<AssignUserRolesCommand, ApiResponse<bool>>
    {
        private readonly UserManagementDbContext _context;
        private readonly ILogger<AssignUserRolesCommandHandler> _logger;

        public AssignUserRolesCommandHandler(UserManagementDbContext context, ILogger<AssignUserRolesCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Assigning roles to user {UserId}: {Roles}", request.UserId, string.Join(", ", request.Roles));

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResult("User not found");
                }

                // Get valid roles from database
                var dbRoles = await _context.Roles
                    .Where(r => request.Roles.Contains(r.Name))
                    .ToListAsync(cancellationToken);

                // Remove old roles that are not in the new list
                var currentRoles = user.UserRoles.ToList();
                foreach (var userRole in currentRoles)
                {
                    if (!request.Roles.Contains(userRole.Role.Name))
                    {
                        _context.UserRoles.Remove(userRole);
                    }
                }

                // Add new roles
                foreach (var roleName in request.Roles)
                {
                    if (!user.UserRoles.Any(ur => ur.Role.Name == roleName))
                    {
                        var role = dbRoles.FirstOrDefault(r => r.Name == roleName);
                        if (role != null)
                        {
                            user.AddRole(role);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.SuccessResult(true, "Roles updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user {UserId}", request.UserId);
                return ApiResponse<bool>.ErrorResult("An error occurred while updating roles");
            }
        }
    }
}

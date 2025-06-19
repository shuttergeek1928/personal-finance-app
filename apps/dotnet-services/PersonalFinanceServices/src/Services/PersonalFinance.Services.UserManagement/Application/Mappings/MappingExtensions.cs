using AutoMapper;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Application.Mappings
{
    public static class MappingExtensions
    {
        // Extension methods for common mapping scenarios
        public static UserTransferObject ToDto(this User user, IMapper mapper)
        {
            return mapper.Map<UserTransferObject>(user);
        }

        public static List<UserTransferObject> ToDto(this IEnumerable<User> users, IMapper mapper)
        {
            return mapper.Map<List<UserTransferObject>>(users);
        }

        public static UserProfileTransferObject ToDto(this UserProfile profile, IMapper mapper)
        {
            return mapper.Map<UserProfileTransferObject>(profile);
        }

        public static RoleTransferObject ToDto(this Role role, IMapper mapper)
        {
            return mapper.Map<RoleTransferObject>(role);
        }
    }
}

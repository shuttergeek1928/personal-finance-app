using AutoMapper;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Domain.Entities;

namespace PersonalFinance.Services.Transactions.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // User Entity <-> UserDto
            CreateMap<User, UserTransferObject>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role!.Name).ToList()))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            // UserProfile Entity <-> UserProfileDto
            CreateMap<UserProfile, UserProfileTransferObject>();

            // Role Entity <-> RoleDto
            CreateMap<Role, RoleTransferObject>();

            // Request DTOs to Domain Objects
            CreateMap<RegisterUserRequest, User>()
                .ConstructUsing(src => new User(src.Email, src.UserName, src.FirstName, src.LastName))
                .ForAllMembers(opt => opt.Ignore()); // Ignore all members since we use constructor

            CreateMap<UpdateUserProfileRequest, UserProfile>()
                .ForAllMembers(opt => opt.Ignore()); // Handle updates through methods
        }
    }
}
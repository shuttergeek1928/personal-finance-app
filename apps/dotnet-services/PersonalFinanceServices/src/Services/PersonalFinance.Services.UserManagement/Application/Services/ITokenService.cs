using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Application.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, IEnumerable<string> roles);
    }
}

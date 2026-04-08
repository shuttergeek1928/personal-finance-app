using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.Services.UserManagement.Application.DataTransferObjects
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

namespace PersonalFinance.Services.UserManagement.Application.DataTransferObjects
{
    public class RoleTransferObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}

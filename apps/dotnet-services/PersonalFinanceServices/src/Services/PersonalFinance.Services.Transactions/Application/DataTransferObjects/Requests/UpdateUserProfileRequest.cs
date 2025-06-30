namespace PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests
{
    public class UpdateUserProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Currency { get; set; } = "INR";
        public string TimeZone { get; set; } = "UTC";
        public string Language { get; set; } = "en-US";
    }
}

namespace PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects
{
    public class ParsedTransactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProcessedEmailId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string TransactionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? MerchantName { get; set; }
        public string? ReferenceNumber { get; set; }
        public Guid? SuggestedAccountId { get; set; }
        public string Status { get; set; } = string.Empty;
        public float ConfidenceScore { get; set; }
        public string Source { get; set; } = "Gmail";
        public Guid? ConfirmedTransactionId { get; set; }

        // From the linked ProcessedEmail
        public string? EmailSubject { get; set; }
        public string? EmailSender { get; set; }
        public DateTime? EmailDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class SyncStatusDto
    {
        public Guid UserId { get; set; }
        public bool IsGmailConnected { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public int TotalEmailsProcessed { get; set; }
        public int TotalTransactionsParsed { get; set; }
        public int TotalTransactionsConfirmed { get; set; }
        public int PendingReviewCount { get; set; }
        public List<CategorySyncInfo> CategorySyncInfo { get; set; } = new();
    }

    public class CategorySyncInfo
    {
        public string Category { get; set; } = string.Empty;
        public DateTime? LastSyncAt { get; set; }
        public int EmailsProcessed { get; set; }
        public int TransactionsParsed { get; set; }
    }

    public class EmailSyncResultDto
    {
        public int EmailsFetched { get; set; }
        public int EmailsProcessed { get; set; }
        public int TransactionsParsed { get; set; }
        public int Duplicates { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
    }

    public class ConfirmTransactionRequest
    {
        public Guid AccountId { get; set; }
    }

    public class UpdateParsedTransactionRequest
    {
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime? TransactionDate { get; set; }
        public Guid? AccountId { get; set; }
    }

    public class BulkConfirmRequest
    {
        public float MinConfidenceScore { get; set; } = 0.9f;
        public Guid AccountId { get; set; }
    }
}

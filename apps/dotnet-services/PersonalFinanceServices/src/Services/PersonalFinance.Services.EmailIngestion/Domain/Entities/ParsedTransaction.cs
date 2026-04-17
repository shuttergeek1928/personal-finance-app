using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.EmailIngestion.Domain.Entities
{
    /// <summary>
    /// Staging entity for parsed transactions before user confirms them.
    /// All email-parsed transactions land here first for review.
    /// </summary>
    public class ParsedTransaction : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid ProcessedEmailId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "INR";
        public string TransactionType { get; private set; } = string.Empty; // Income, Expense, Transfer
        public string Category { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }
        public string? MerchantName { get; private set; }
        public string? ReferenceNumber { get; private set; }
        public Guid? SuggestedAccountId { get; private set; }
        public ParsedTransactionStatus Status { get; private set; }

        /// <summary>
        /// Confidence score from the parsing rule (0.0 - 1.0).
        /// UI uses this for the "Confirm All >90%" bulk action.
        /// </summary>
        public float ConfidenceScore { get; private set; }

        /// <summary>
        /// Source of the transaction data.
        /// </summary>
        public string Source { get; private set; } = "Gmail";

        /// <summary>
        /// The TransactionId from the Transactions microservice after confirmation.
        /// </summary>
        public Guid? ConfirmedTransactionId { get; private set; }

        // Navigation property
        public ProcessedEmail? ProcessedEmail { get; private set; }

        private ParsedTransaction() { }

        public ParsedTransaction(Guid userId, Guid processedEmailId, decimal amount,
            string currency, string transactionType, string category, string description,
            DateTime transactionDate, float confidenceScore, string? merchantName = null,
            string? referenceNumber = null, Guid? suggestedAccountId = null)
        {
            UserId = userId;
            ProcessedEmailId = processedEmailId;
            Amount = amount;
            Currency = currency ?? "INR";
            TransactionType = transactionType ?? throw new ArgumentNullException(nameof(transactionType));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Description = description ?? string.Empty;
            TransactionDate = transactionDate;
            ConfidenceScore = confidenceScore;
            MerchantName = merchantName;
            ReferenceNumber = referenceNumber;
            SuggestedAccountId = suggestedAccountId;
            Status = ParsedTransactionStatus.Pending;
        }

        public void Confirm(Guid transactionId)
        {
            Status = ParsedTransactionStatus.Confirmed;
            ConfirmedTransactionId = transactionId;
        }

        public void Reject()
        {
            Status = ParsedTransactionStatus.Rejected;
        }

        public void UpdateDetails(decimal? amount, string? category, string? description,
            DateTime? transactionDate, Guid? accountId)
        {
            if (amount.HasValue) Amount = amount.Value;
            if (category != null) Category = category;
            if (description != null) Description = description;
            if (transactionDate.HasValue) TransactionDate = transactionDate.Value;
            if (accountId.HasValue) SuggestedAccountId = accountId;
        }
    }

    public enum ParsedTransactionStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}

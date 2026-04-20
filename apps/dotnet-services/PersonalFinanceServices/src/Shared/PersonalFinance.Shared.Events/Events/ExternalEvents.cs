namespace PersonalFinance.Shared.Events.Events
{
    /// <summary>
    /// Triggered when a transaction is confirmed from an external source (like Gmail)
    /// and needs to be processed by both Transactions and Accounts services.
    /// </summary>
    public class ExternalTransactionConfirmedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid ExternalId { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string TransactionType { get; set; } = string.Empty; // Income, Expense
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    /// <summary>
    /// Triggered when a previously confirmed transaction is reset back to pending.
    /// Leads to removal of the transaction from the main ledger if it was already created.
    /// </summary>
    public class ExternalTransactionResetEvent
    {
        public Guid ExternalId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ConfirmedTransactionId { get; set; }
    }
}

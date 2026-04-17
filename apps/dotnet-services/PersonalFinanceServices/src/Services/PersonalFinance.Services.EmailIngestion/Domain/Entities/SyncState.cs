using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.EmailIngestion.Domain.Entities
{
    /// <summary>
    /// Tracks the last sync timestamp per user per category for adaptive polling.
    /// </summary>
    public class SyncState : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Category { get; set; } = "General";
        public DateTime? LastSyncAt { get; set; }
        public long? LastHistoryId { get; set; }
        public int TotalEmailsProcessed { get; set; }
        public int TotalTransactionsParsed { get; set; }
        public int TotalTransactionsConfirmed { get; set; }

        public SyncState() { }

        public SyncState(Guid userId, string category)
        {
            UserId = userId;
            Category = category ?? "General";
        }

        public void RecordSync(DateTime syncTime, int emailsProcessed, int transactionsParsed, long? historyId = null)
        {
            LastSyncAt = syncTime;
            TotalEmailsProcessed += emailsProcessed;
            TotalTransactionsParsed += transactionsParsed;
            if (historyId.HasValue)
                LastHistoryId = historyId;
        }
    }
}

using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.EmailIngestion.Domain.Entities
{
    /// <summary>
    /// Tracks which Gmail messages have been processed to prevent duplicate ingestion.
    /// </summary>
    public class ProcessedEmail : AggregateRoot
    {
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gmail's unique message ID — used for deduplication.
        /// </summary>
        public string GmailMessageId { get; private set; } = string.Empty;

        /// <summary>
        /// Gmail thread ID for grouping related emails.
        /// </summary>
        public string ThreadId { get; private set; } = string.Empty;

        public string Subject { get; private set; } = string.Empty;
        public string SenderEmail { get; private set; } = string.Empty;
        public DateTime EmailDate { get; private set; }
        public ProcessingStatus Status { get; private set; }
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Links to the Transaction that was created from this email (if any).
        /// </summary>
        public Guid? LinkedTransactionId { get; private set; }

        private ProcessedEmail() { }

        public ProcessedEmail(Guid userId, string gmailMessageId, string threadId,
            string subject, string senderEmail, DateTime emailDate)
        {
            UserId = userId;
            GmailMessageId = gmailMessageId ?? throw new ArgumentNullException(nameof(gmailMessageId));
            ThreadId = threadId ?? throw new ArgumentNullException(nameof(threadId));
            Subject = subject ?? string.Empty;
            SenderEmail = senderEmail ?? string.Empty;
            EmailDate = emailDate;
            Status = ProcessingStatus.Processing;
        }

        public void MarkParsed()
        {
            Status = ProcessingStatus.Parsed;
        }

        public void MarkSkipped(string reason)
        {
            Status = ProcessingStatus.Skipped;
            ErrorMessage = reason;
        }

        public void MarkFailed(string errorMessage)
        {
            Status = ProcessingStatus.Failed;
            ErrorMessage = errorMessage;
        }

        public void LinkTransaction(Guid transactionId)
        {
            LinkedTransactionId = transactionId;
            Status = ProcessingStatus.Confirmed;
        }
    }

    public enum ProcessingStatus
    {
        Processing,
        Parsed,
        Confirmed,
        Skipped,
        Failed
    }
}

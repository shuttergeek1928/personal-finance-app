using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Strategy pattern interface for parsing transaction data from emails.
    /// Each email source (bank, Swiggy, UPI, etc.) has its own implementation.
    /// </summary>
    public interface IEmailParsingRule
    {
        /// <summary>
        /// Determines if this rule can parse the given email.
        /// </summary>
        bool CanParse(string senderEmail, string subject, string body);

        /// <summary>
        /// Parses the email and returns a ParsedTransactionDto, or null if parsing fails.
        /// </summary>
        ParsedTransactionDto? Parse(string senderEmail, string subject, string body, DateTime emailDate);

        /// <summary>
        /// The confidence score of this rule (0.0 - 1.0).
        /// Higher scores indicate more reliable parsing.
        /// </summary>
        float ConfidenceScore { get; }

        /// <summary>
        /// The category this rule targets for adaptive polling scheduling.
        /// </summary>
        string SyncCategory { get; }
    }
}

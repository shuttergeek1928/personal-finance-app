using System.Text.RegularExpressions;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Parses Swiggy order/payment receipt emails.
    /// </summary>
    public class SwiggyParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.95f;
        public string SyncCategory => "FoodDelivery";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sender = senderEmail.ToLower();
            return sender.Contains("swiggy.in") || sender.Contains("swiggy.com") ||
                   sender.Contains("noreply@swiggy");
        }

        public ParsedTransactionDto? Parse(string senderEmail, string subject, string body, DateTime emailDate)
        {
            try
            {
                var fullText = $"{subject} {body}";
                var amountMatch = Regex.Match(fullText, @"(?:₹|Rs\.?|INR)\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
                if (!amountMatch.Success) return null;

                var amount = decimal.Parse(amountMatch.Groups[1].Value.Replace(",", ""));
                if (amount <= 0) return null;

                // Extract order ID
                var orderIdMatch = Regex.Match(fullText, @"(?:order\s*(?:id|#|no)?[\s:#]*)?([A-Z0-9]{8,})", RegexOptions.IgnoreCase);
                var orderId = orderIdMatch.Success ? orderIdMatch.Groups[1].Value : null;

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Expense",
                    Category = "Food & Dining",
                    Description = $"Swiggy Order - {subject.Trim()}",
                    MerchantName = "Swiggy",
                    ReferenceNumber = orderId,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Parses Zomato order/payment receipt emails.
    /// </summary>
    public class ZomatoParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.95f;
        public string SyncCategory => "FoodDelivery";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sender = senderEmail.ToLower();
            return sender.Contains("zomato.com") || sender.Contains("noreply@zomato");
        }

        public ParsedTransactionDto? Parse(string senderEmail, string subject, string body, DateTime emailDate)
        {
            try
            {
                var fullText = $"{subject} {body}";
                var amountMatch = Regex.Match(fullText, @"(?:₹|Rs\.?|INR)\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
                if (!amountMatch.Success) return null;

                var amount = decimal.Parse(amountMatch.Groups[1].Value.Replace(",", ""));
                if (amount <= 0) return null;

                var orderIdMatch = Regex.Match(fullText, @"(?:order\s*(?:id|#|no)?[\s:#]*)([A-Z0-9-]{6,})", RegexOptions.IgnoreCase);

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Expense",
                    Category = "Food & Dining",
                    Description = $"Zomato Order - {subject.Trim()}",
                    MerchantName = "Zomato",
                    ReferenceNumber = orderIdMatch.Success ? orderIdMatch.Groups[1].Value : null,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }
}

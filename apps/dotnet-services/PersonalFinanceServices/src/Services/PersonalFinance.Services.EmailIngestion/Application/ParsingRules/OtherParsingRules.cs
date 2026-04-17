using System.Text.RegularExpressions;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Parses credit card transaction alert emails.
    /// </summary>
    public class CreditCardParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.91f;
        public string SyncCategory => "CreditCard";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sub = subject.ToLower();
            var bodyLower = body.ToLower();

            return (sub.Contains("credit card") || sub.Contains("card transaction") ||
                    sub.Contains("card ending") || bodyLower.Contains("credit card") ||
                    bodyLower.Contains("card ending") || bodyLower.Contains("card no")) &&
                   (sub.Contains("transaction") || sub.Contains("spent") || sub.Contains("charged") ||
                    bodyLower.Contains("transaction") || bodyLower.Contains("charged"));
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

                var cardMatch = Regex.Match(fullText, @"card\s*(?:ending|no\.?|number)?\s*(?:in|with|xx)?\s*(\d{4})", RegexOptions.IgnoreCase);
                var merchantMatch = Regex.Match(fullText, @"(?:at|merchant|to)\s+([A-Za-z0-9\s\-\.]+?)(?:\s+(?:on|for|ref|via))", RegexOptions.IgnoreCase);

                string cardSuffix = cardMatch.Success ? cardMatch.Groups[1].Value : "";
                string? merchant = merchantMatch.Success ? merchantMatch.Groups[1].Value.Trim() : null;

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Expense",
                    Category = "Credit Card",
                    Description = $"CC {cardSuffix} - {merchant ?? subject.Trim()}",
                    MerchantName = merchant,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Parses salary credit notification emails.
    /// </summary>
    public class SalaryParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.88f;
        public string SyncCategory => "Salary";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sub = subject.ToLower();
            var bodyLower = body.ToLower();

            return (sub.Contains("salary") || sub.Contains("payroll") || sub.Contains("salary credited") ||
                    bodyLower.Contains("salary credit") || bodyLower.Contains("payroll")) &&
                   (sub.Contains("credit") || bodyLower.Contains("credited"));
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

                var refMatch = Regex.Match(fullText, @"(?:ref|UTR|reference)[\s:#]*([A-Za-z0-9]+)", RegexOptions.IgnoreCase);

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Income",
                    Category = "Salary",
                    Description = $"Salary Credit - {subject.Trim()}",
                    ReferenceNumber = refMatch.Success ? refMatch.Groups[1].Value : null,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// Parses subscription/recurring charge emails (Netflix, Spotify, AWS, etc.)
    /// </summary>
    public class SubscriptionParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.93f;
        public string SyncCategory => "Subscription";

        private static readonly Dictionary<string, string> SubscriptionSenders = new(StringComparer.OrdinalIgnoreCase)
        {
            { "netflix", "Netflix" }, { "spotify", "Spotify" }, { "amazon", "Amazon Prime" },
            { "hotstar", "Disney+ Hotstar" }, { "jiocinema", "JioCinema" },
            { "youtube", "YouTube Premium" }, { "google", "Google One" },
            { "apple", "Apple" }, { "aws", "AWS" }, { "azure", "Microsoft Azure" },
            { "github", "GitHub" }, { "notion", "Notion" }, { "figma", "Figma" },
            { "canva", "Canva" }, { "adobe", "Adobe" }, { "zoom", "Zoom" },
            { "slack", "Slack" }, { "dropbox", "Dropbox" }
        };

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sender = senderEmail.ToLower();
            var sub = subject.ToLower();

            bool isSubscriptionSender = SubscriptionSenders.Keys.Any(k => sender.Contains(k));
            bool isPaymentEmail = sub.Contains("payment") || sub.Contains("receipt") ||
                                  sub.Contains("invoice") || sub.Contains("subscription") ||
                                  sub.Contains("renewal") || sub.Contains("charge") ||
                                  sub.Contains("billing");

            return isSubscriptionSender && isPaymentEmail;
        }

        public ParsedTransactionDto? Parse(string senderEmail, string subject, string body, DateTime emailDate)
        {
            try
            {
                var fullText = $"{subject} {body}";
                var amountMatch = Regex.Match(fullText, @"(?:₹|Rs\.?|INR|\$|USD|EUR|€)\s*([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
                if (!amountMatch.Success) return null;

                var amount = decimal.Parse(amountMatch.Groups[1].Value.Replace(",", ""));
                if (amount <= 0) return null;

                // Determine currency
                string currency = "INR";
                if (Regex.IsMatch(fullText, @"[\$]|USD", RegexOptions.IgnoreCase)) currency = "USD";
                else if (Regex.IsMatch(fullText, @"[€]|EUR", RegexOptions.IgnoreCase)) currency = "EUR";

                // Identify the subscription service
                string merchant = "Subscription";
                foreach (var kvp in SubscriptionSenders)
                {
                    if (senderEmail.ToLower().Contains(kvp.Key) || subject.ToLower().Contains(kvp.Key))
                    {
                        merchant = kvp.Value;
                        break;
                    }
                }

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = currency,
                    TransactionType = "Expense",
                    Category = "Subscription",
                    Description = $"{merchant} Subscription - {subject.Trim()}",
                    MerchantName = merchant,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }
}

using System.Text.RegularExpressions;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Parses EMI deduction alert emails from banks and NBFCs.
    /// </summary>
    public class EmiParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.90f;
        public string SyncCategory => "EMI";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sub = subject.ToLower();
            var bodyLower = body.ToLower();

            return (sub.Contains("emi") || sub.Contains("instalment") || sub.Contains("installment") ||
                    sub.Contains("loan repayment") || bodyLower.Contains("emi deducted") ||
                    bodyLower.Contains("emi debited") || bodyLower.Contains("auto-debit")) &&
                   (sub.Contains("debit") || sub.Contains("deducted") || bodyLower.Contains("debited") ||
                    bodyLower.Contains("deducted") || bodyLower.Contains("emi"));
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

                // Determine EMI type
                var lowerText = fullText.ToLower();
                string category = "EMI";
                if (lowerText.Contains("home loan") || lowerText.Contains("housing loan"))
                    category = "Home Loan EMI";
                else if (lowerText.Contains("car loan") || lowerText.Contains("vehicle loan") || lowerText.Contains("auto loan"))
                    category = "Car Loan EMI";
                else if (lowerText.Contains("personal loan"))
                    category = "Personal Loan EMI";
                else if (lowerText.Contains("education loan") || lowerText.Contains("student loan"))
                    category = "Education Loan EMI";

                var refMatch = Regex.Match(fullText, @"(?:loan\s*(?:a/c|account|no)[\s:#]*)([A-Za-z0-9]+)", RegexOptions.IgnoreCase);

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Expense",
                    Category = category,
                    Description = $"EMI Payment - {subject.Trim()}",
                    MerchantName = ExtractLender(senderEmail, fullText),
                    ReferenceNumber = refMatch.Success ? refMatch.Groups[1].Value : null,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }

        private string? ExtractLender(string senderEmail, string text)
        {
            var sender = senderEmail.ToLower();
            if (sender.Contains("hdfc")) return "HDFC Bank";
            if (sender.Contains("sbi")) return "SBI";
            if (sender.Contains("icici")) return "ICICI Bank";
            if (sender.Contains("kotak")) return "Kotak Mahindra Bank";
            if (sender.Contains("axis")) return "Axis Bank";
            if (sender.Contains("bajaj")) return "Bajaj Finance";
            if (sender.Contains("tata")) return "Tata Capital";
            return null;
        }
    }

    /// <summary>
    /// Parses loan disbursement and repayment emails.
    /// </summary>
    public class LoanParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.85f;
        public string SyncCategory => "EMI";

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sub = subject.ToLower();
            var bodyLower = body.ToLower();

            return (sub.Contains("loan") || sub.Contains("disburs")) &&
                   (sub.Contains("approved") || sub.Contains("disbursed") || sub.Contains("sanction") ||
                    bodyLower.Contains("loan amount") || bodyLower.Contains("disbursement"));
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

                bool isDisbursement = fullText.ToLower().Contains("disburs") || fullText.ToLower().Contains("sanction");

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = isDisbursement ? "Income" : "Expense",
                    Category = "Loan",
                    Description = $"Loan - {subject.Trim()}",
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }
}

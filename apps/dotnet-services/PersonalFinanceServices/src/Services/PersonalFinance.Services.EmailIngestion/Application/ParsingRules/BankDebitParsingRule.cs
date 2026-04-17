using System.Text.RegularExpressions;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Parses bank debit/credit alert emails from major Indian banks (HDFC, SBI, ICICI, Kotak, Axis, etc.)
    /// </summary>
    public class BankDebitParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.92f;
        public string SyncCategory => "UPI";

        private static readonly string[] BankSenderPatterns = new[]
        {
            "alerts@hdfcbank", "noreply@hdfcbank", "alerts@icicibank",
            "alert@sbi", "donotreply@sbi", "alerts@kotak", "alerts@axisbank",
            "alerts@indusind", "alerts@yesbank", "alerts@pnb", "alerts@bob",
            "alerts@unionbank", "alerts@canarabank", "alerts@iob",
            "transaction@hdfcbank", "noreply@icicibank", "alerts@federalbank"
        };

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sender = senderEmail.ToLower();
            var sub = subject.ToLower();

            bool isBankSender = BankSenderPatterns.Any(p => sender.Contains(p));
            bool isTransactionEmail = sub.Contains("debited") || sub.Contains("credited") ||
                                      sub.Contains("transaction") || sub.Contains("withdrawn") ||
                                      sub.Contains("transfer") || sub.Contains("payment") ||
                                      body.ToLower().Contains("debited") || body.ToLower().Contains("credited");

            return isBankSender && isTransactionEmail;
        }

        public ParsedTransactionDto? Parse(string senderEmail, string subject, string body, DateTime emailDate)
        {
            try
            {
                var fullText = $"{subject} {body}".Replace("\n", " ").Replace("\r", " ");

                // Extract amount using multiple patterns
                decimal? amount = ExtractAmount(fullText);
                if (!amount.HasValue || amount.Value <= 0) return null;

                // Determine if it's debit or credit
                var lowerText = fullText.ToLower();
                bool isDebit = lowerText.Contains("debited") || lowerText.Contains("withdrawn") ||
                               lowerText.Contains("paid") || lowerText.Contains("purchase");
                bool isCredit = lowerText.Contains("credited") || lowerText.Contains("received") ||
                                lowerText.Contains("deposit");

                string transactionType = isCredit && !isDebit ? "Income" : "Expense";

                // Extract reference number
                string? referenceNumber = ExtractReferenceNumber(fullText);

                // Extract merchant/payee
                string? merchant = ExtractMerchant(fullText);

                // Determine category
                string category = DetermineCategory(fullText, transactionType);

                // Build description
                string description = BuildDescription(subject, merchant, referenceNumber);

                return new ParsedTransactionDto
                {
                    Amount = amount.Value,
                    Currency = "INR",
                    TransactionType = transactionType,
                    Category = category,
                    Description = description,
                    MerchantName = merchant,
                    ReferenceNumber = referenceNumber,
                    TransactionDate = ExtractTransactionDate(fullText) ?? emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch
            {
                return null;
            }
        }

        private decimal? ExtractAmount(string text)
        {
            // Patterns: Rs. 1,234.56 / Rs 1234 / INR 1,234.56 / ₹1234.56 / Rs.1,234.56
            var patterns = new[]
            {
                @"(?:Rs\.?|INR|₹)\s*([\d,]+\.?\d*)",
                @"(?:amount|amt)[\s:]*(?:Rs\.?|INR|₹)?\s*([\d,]+\.?\d*)",
                @"(?:debited|credited|withdrawn|paid|received)[\s\w]*(?:Rs\.?|INR|₹)\s*([\d,]+\.?\d*)",
                @"(?:Rs\.?|INR|₹)\s*([\d,]+\.?\d*)[\s]*(?:has been|was|is)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var amount))
                {
                    return amount;
                }
            }
            return null;
        }

        private string? ExtractReferenceNumber(string text)
        {
            var patterns = new[]
            {
                @"(?:ref\.?\s*(?:no\.?|number)?|reference|UTR|UPI\s*ref|txn\s*(?:no|id)|transaction\s*(?:no|id))[\s:#]*([A-Za-z0-9]+)",
                @"(?:UPI)[\s/-]*([A-Za-z0-9]{12,})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }
            return null;
        }

        private string? ExtractMerchant(string text)
        {
            var patterns = new[]
            {
                @"(?:to|at|paid to|transferred to|sent to)\s+([A-Za-z0-9\s\-\.]+?)(?:\s+(?:on|ref|via|UPI|Ref|for))",
                @"(?:from|received from|credited by)\s+([A-Za-z0-9\s\-\.]+?)(?:\s+(?:on|ref|via|UPI|Ref))",
                @"VPA\s+([a-zA-Z0-9\.\-]+@[a-zA-Z]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var merchant = match.Groups[1].Value.Trim();
                    if (merchant.Length > 2 && merchant.Length < 100)
                        return merchant;
                }
            }
            return null;
        }

        private DateTime? ExtractTransactionDate(string text)
        {
            var patterns = new[]
            {
                @"(\d{2}[-/]\d{2}[-/]\d{4})",
                @"(\d{2}[-/]\d{2}[-/]\d{2})\b",
                @"(\d{4}[-/]\d{2}[-/]\d{2})",
                @"(\d{2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+\d{4})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var date))
                    return date;
            }
            return null;
        }

        private string DetermineCategory(string text, string transactionType)
        {
            var lowerText = text.ToLower();

            if (lowerText.Contains("upi")) return "UPI Payment";
            if (lowerText.Contains("neft") || lowerText.Contains("rtgs") || lowerText.Contains("imps"))
                return "Bank Transfer";
            if (lowerText.Contains("atm")) return "ATM Withdrawal";
            if (lowerText.Contains("salary") || lowerText.Contains("payroll")) return "Salary";
            if (lowerText.Contains("emi") || lowerText.Contains("instalment")) return "EMI";
            if (lowerText.Contains("insurance") || lowerText.Contains("premium")) return "Insurance";
            if (lowerText.Contains("interest")) return "Interest";
            if (lowerText.Contains("dividend")) return "Dividend";
            if (lowerText.Contains("refund")) return "Refund";

            return transactionType == "Income" ? "Bank Credit" : "Bank Debit";
        }

        private string BuildDescription(string subject, string? merchant, string? referenceNumber)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(subject))
                parts.Add(subject.Length > 100 ? subject[..100] : subject);
            if (!string.IsNullOrEmpty(merchant))
                parts.Add($"Merchant: {merchant}");
            if (!string.IsNullOrEmpty(referenceNumber))
                parts.Add($"Ref: {referenceNumber}");

            return string.Join(" | ", parts);
        }
    }
}

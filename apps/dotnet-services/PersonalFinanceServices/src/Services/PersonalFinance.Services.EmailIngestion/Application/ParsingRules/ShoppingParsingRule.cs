using System.Text.RegularExpressions;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;

namespace PersonalFinance.Services.EmailIngestion.Application.ParsingRules
{
    /// <summary>
    /// Parses e-commerce shopping receipt emails (Amazon, Flipkart, Myntra, etc.)
    /// </summary>
    public class ShoppingParsingRule : IEmailParsingRule
    {
        public float ConfidenceScore => 0.93f;
        public string SyncCategory => "General";

        private static readonly Dictionary<string, string> ShoppingSenders = new(StringComparer.OrdinalIgnoreCase)
        {
            { "amazon", "Amazon" }, { "flipkart", "Flipkart" }, { "myntra", "Myntra" },
            { "ajio", "AJIO" }, { "nykaa", "Nykaa" }, { "meesho", "Meesho" },
            { "snapdeal", "Snapdeal" }, { "bigbasket", "BigBasket" },
            { "blinkit", "Blinkit" }, { "zepto", "Zepto" }, { "instamart", "Swiggy Instamart" },
            { "jiomart", "JioMart" }, { "croma", "Croma" }, { "reliance", "Reliance Digital" }
        };

        public bool CanParse(string senderEmail, string subject, string body)
        {
            var sender = senderEmail.ToLower();
            var sub = subject.ToLower();

            bool isShoppingSender = ShoppingSenders.Keys.Any(k => sender.Contains(k));
            bool isOrderEmail = sub.Contains("order") || sub.Contains("receipt") ||
                                sub.Contains("payment") || sub.Contains("invoice") ||
                                sub.Contains("purchase") || sub.Contains("delivered");

            return isShoppingSender && isOrderEmail;
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

                string merchant = "Shopping";
                foreach (var kvp in ShoppingSenders)
                {
                    if (senderEmail.ToLower().Contains(kvp.Key))
                    {
                        merchant = kvp.Value;
                        break;
                    }
                }

                var orderMatch = Regex.Match(fullText, @"(?:order\s*(?:id|#|no\.?)?[\s:#]*)([A-Z0-9\-]{6,})", RegexOptions.IgnoreCase);

                // Differentiate grocery from shopping
                string category = "Shopping";
                var groceryMerchants = new[] { "bigbasket", "blinkit", "zepto", "instamart", "jiomart" };
                if (groceryMerchants.Any(g => senderEmail.ToLower().Contains(g)))
                    category = "Groceries";

                return new ParsedTransactionDto
                {
                    Amount = amount,
                    Currency = "INR",
                    TransactionType = "Expense",
                    Category = category,
                    Description = $"{merchant} - {subject.Trim()}",
                    MerchantName = merchant,
                    ReferenceNumber = orderMatch.Success ? orderMatch.Groups[1].Value : null,
                    TransactionDate = emailDate,
                    ConfidenceScore = ConfidenceScore
                };
            }
            catch { return null; }
        }
    }
}

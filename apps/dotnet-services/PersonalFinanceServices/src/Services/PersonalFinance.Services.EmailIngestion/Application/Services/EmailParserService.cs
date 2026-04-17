using HtmlAgilityPack;

using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Application.ParsingRules;

namespace PersonalFinance.Services.EmailIngestion.Application.Services
{
    public interface IEmailParserService
    {
        /// <summary>
        /// Runs a Gmail message through all parsing rules and returns the best match.
        /// </summary>
        ParsedTransactionDto? ParseEmail(GmailEmailMessage email);
    }

    public class EmailParserService : IEmailParserService
    {
        private readonly IEnumerable<IEmailParsingRule> _parsingRules;
        private readonly ILogger<EmailParserService> _logger;

        public EmailParserService(IEnumerable<IEmailParsingRule> parsingRules, ILogger<EmailParserService> logger)
        {
            _parsingRules = parsingRules ?? throw new ArgumentNullException(nameof(parsingRules));
            _logger = logger;
        }

        public ParsedTransactionDto? ParseEmail(GmailEmailMessage email)
        {
            try
            {
                // Extract clean text from HTML if plain text body is empty
                var body = !string.IsNullOrWhiteSpace(email.Body) ? email.Body : ExtractTextFromHtml(email.HtmlBody);

                if (string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(email.Subject))
                {
                    _logger.LogDebug("Skipping email {MessageId} — empty body and subject", email.MessageId);
                    return null;
                }

                ParsedTransactionDto? bestMatch = null;
                float bestScore = 0;

                foreach (var rule in _parsingRules)
                {
                    try
                    {
                        if (!rule.CanParse(email.SenderEmail, email.Subject, body))
                            continue;

                        var result = rule.Parse(email.SenderEmail, email.Subject, body, email.Date);

                        if (result != null && result.ConfidenceScore > bestScore)
                        {
                            bestMatch = result;
                            bestScore = result.ConfidenceScore;

                            _logger.LogDebug("Rule {RuleType} matched email {MessageId} with confidence {Score}",
                                rule.GetType().Name, email.MessageId, result.ConfidenceScore);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Parsing rule {RuleType} failed for email {MessageId}",
                            rule.GetType().Name, email.MessageId);
                    }
                }

                if (bestMatch != null)
                {
                    _logger.LogInformation("Parsed email {MessageId}: {Type} {Amount} {Currency} [{Category}] confidence={Score}",
                        email.MessageId, bestMatch.TransactionType, bestMatch.Amount,
                        bestMatch.Currency, bestMatch.Category, bestMatch.ConfidenceScore);
                }
                else
                {
                    _logger.LogDebug("No parsing rule matched email {MessageId} from {Sender}: {Subject}",
                        email.MessageId, email.SenderEmail, email.Subject);
                }

                return bestMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing email {MessageId}", email.MessageId);
                return null;
            }
        }

        /// <summary>
        /// Extracts plain text from HTML email body using HtmlAgilityPack.
        /// </summary>
        private string ExtractTextFromHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Remove script and style elements
                var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                        node.Remove();
                }

                return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText)
                    .Replace("\t", " ")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Trim();
            }
            catch
            {
                return html;
            }
        }
    }
}

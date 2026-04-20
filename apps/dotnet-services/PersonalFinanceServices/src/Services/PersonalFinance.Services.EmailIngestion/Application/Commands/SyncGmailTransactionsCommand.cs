using AutoMapper;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.EmailIngestion.Application.Common;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects.Response;
using PersonalFinance.Services.EmailIngestion.Application.Services;
using PersonalFinance.Services.EmailIngestion.Domain.Entities;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.EmailIngestion.Application.Commands
{
    /// <summary>
    /// Triggers a manual Gmail sync for the user.
    /// </summary>
    public class SyncGmailTransactionsCommand : IRequest<ApiResponse<EmailSyncResultDto>>
    {
        public Guid UserId { get; set; }
        public string GmailAccessToken { get; set; } = string.Empty;
        public string GmailRefreshToken { get; set; } = string.Empty;
        public string? CategoryFilter { get; set; }
    }

    public class SyncGmailTransactionsCommandHandler :
        BaseRequestHandler<SyncGmailTransactionsCommand, ApiResponse<EmailSyncResultDto>>
    {
        private readonly IGmailApiService _gmailService;
        private readonly IEmailParserService _parserService;

        public SyncGmailTransactionsCommandHandler(
            EmailIngestionDbContext context,
            ILogger<SyncGmailTransactionsCommandHandler> logger,
            IMapper mapper,
            IGmailApiService gmailService,
            IEmailParserService parserService) : base(context, logger, mapper)
        {
            _gmailService = gmailService;
            _parserService = parserService;
        }

        public override async Task<ApiResponse<EmailSyncResultDto>> Handle(
            SyncGmailTransactionsCommand request, CancellationToken cancellationToken)
        {
            var result = new EmailSyncResultDto();

            try
            {
                Logger.LogInformation("Starting Gmail sync for user {UserId}, category: {Category}",
                    request.UserId, request.CategoryFilter ?? "All");

                // Get stored tokens for the user if not provided in the request
                var accessToken = request.GmailAccessToken;
                var refreshToken = request.GmailRefreshToken;

                if (string.IsNullOrEmpty(accessToken) || accessToken.Contains("dummy"))
                {
                    var userToken = await Context.UserTokens
                        .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);
                    
                    if (userToken == null)
                    {
                        return ApiResponse<EmailSyncResultDto>.ErrorResult("Gmail access tokens not found. Please re-connect your Gmail account.");
                    }
                    
                    accessToken = userToken.AccessToken;
                    refreshToken = userToken.RefreshToken;
                }

                // Get the last sync time for this category
                var category = request.CategoryFilter ?? "General";
                var syncState = await Context.SyncStates
                    .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Category == category,
                        cancellationToken);

                DateTime? lastSync = syncState?.LastSyncAt;

                // Fetch emails from Gmail
                var fetchResult = await _gmailService.FetchEmailsAsync(
                    accessToken,
                    refreshToken,
                    query: BuildGmailQuery(request.CategoryFilter),
                    after: lastSync,
                    maxResults: 100,
                    ct: cancellationToken);

                // Persist refreshed tokens if any
                if (fetchResult.TokenWasRefreshed)
                {
                    var userToken = await Context.UserTokens
                        .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

                    if (userToken != null)
                    {
                        userToken.AccessToken = fetchResult.RefreshedAccessToken!;
                        userToken.ExpiresAt = fetchResult.RefreshedExpiresAt!.Value;
                        userToken.UpdatedAt = DateTime.UtcNow;
                        await Context.SaveChangesAsync(cancellationToken);
                        Logger.LogInformation("Saved refreshed Gmail access token to database for user {UserId}", request.UserId);
                    }
                }

                result.EmailsFetched = fetchResult.Messages.Count;

                foreach (var email in fetchResult.Messages)
                {
                    try
                    {
                        // Check for duplicate
                        var alreadyProcessed = await Context.ProcessedEmails
                            .AnyAsync(p => p.UserId == request.UserId && p.GmailMessageId == email.MessageId,
                                cancellationToken);

                        if (alreadyProcessed)
                        {
                            result.Duplicates++;
                            continue;
                        }

                        // Create ProcessedEmail record
                        var processedEmail = new ProcessedEmail(
                            request.UserId, email.MessageId, email.ThreadId,
                            email.Subject, email.SenderEmail, email.Date);

                        // Try to parse
                        var parsed = _parserService.ParseEmail(email);

                        if (parsed == null)
                        {
                            processedEmail.MarkSkipped("No parsing rule matched");
                            Context.ProcessedEmails.Add(processedEmail);
                            result.EmailsProcessed++;
                            continue;
                        }

                        processedEmail.MarkParsed();
                        Context.ProcessedEmails.Add(processedEmail);
                        await Context.SaveChangesAsync(cancellationToken);

                        // Check dedup by reference number + amount + date
                        bool isDuplicate = false;
                        if (!string.IsNullOrEmpty(parsed.ReferenceNumber))
                        {
                            isDuplicate = await Context.ParsedTransactions
                                .AnyAsync(t => t.UserId == request.UserId &&
                                               t.ReferenceNumber == parsed.ReferenceNumber &&
                                               t.Amount == parsed.Amount &&
                                               t.TransactionDate.Date == parsed.TransactionDate.Date,
                                    cancellationToken);
                        }

                        if (isDuplicate)
                        {
                            result.Duplicates++;
                            continue;
                        }

                        // Create ParsedTransaction in staging
                        var parsedTxn = new ParsedTransaction(
                            request.UserId,
                            processedEmail.Id,
                            parsed.Amount,
                            parsed.Currency,
                            parsed.TransactionType,
                            parsed.Category,
                            parsed.Description,
                            parsed.TransactionDate,
                            parsed.ConfidenceScore,
                            parsed.MerchantName,
                            parsed.ReferenceNumber);

                        Context.ParsedTransactions.Add(parsedTxn);
                        result.TransactionsParsed++;
                        result.EmailsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors++;
                        result.ErrorMessages.Add($"Email {email.MessageId}: {ex.Message}");
                        Logger.LogWarning(ex, "Error processing email {MessageId}", email.MessageId);
                    }
                }

                await Context.SaveChangesAsync(cancellationToken);

                // Update sync state
                if (syncState == null)
                {
                    syncState = new SyncState(request.UserId, category);
                    Context.SyncStates.Add(syncState);
                }
                syncState.RecordSync(DateTime.UtcNow, result.EmailsProcessed, result.TransactionsParsed);
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation(
                    "Gmail sync completed for user {UserId}: {Fetched} fetched, {Processed} processed, {Parsed} parsed, {Dupes} duplicates, {Errors} errors",
                    request.UserId, result.EmailsFetched, result.EmailsProcessed,
                    result.TransactionsParsed, result.Duplicates, result.Errors);

                return ApiResponse<EmailSyncResultDto>.SuccessResult(result, "Gmail sync completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during Gmail sync for user {UserId}", request.UserId);
                return ApiResponse<EmailSyncResultDto>.ErrorResult($"Gmail sync failed: {ex.Message}");
            }
        }

        private string? BuildGmailQuery(string? categoryFilter)
        {
            return categoryFilter?.ToLower() switch
            {
                "upi" => "subject:(debited OR credited OR UPI OR transaction)",
                "fooddelivery" => "from:(swiggy OR zomato)",
                "subscription" => "from:(netflix OR spotify OR amazon OR apple OR google OR youtube) subject:(payment OR receipt OR invoice OR subscription)",
                "emi" => "subject:(EMI OR instalment OR installment OR \"loan repayment\" OR \"auto debit\")",
                "salary" => "subject:(salary OR payroll) (credited)",
                "creditcard" => "subject:(\"credit card\" OR \"card transaction\" OR \"card ending\")",
                "general" or null => "subject:(debited OR credited OR payment OR receipt OR transaction OR salary OR EMI)",
                _ => null
            };
        }
    }
}

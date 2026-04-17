using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.EmailIngestion.Application.Common;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects.Response;
using PersonalFinance.Services.EmailIngestion.Domain.Entities;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;

namespace PersonalFinance.Services.EmailIngestion.Application.Queries
{
    /// <summary>
    /// Returns the sync status for a user (connection status, last sync, counts).
    /// </summary>
    public class GetSyncStatusQuery : IRequest<ApiResponse<SyncStatusDto>>
    {
        public Guid UserId { get; set; }
        public bool HasGmailAccess { get; set; }
    }

    public class GetSyncStatusQueryHandler :
        BaseRequestHandler<GetSyncStatusQuery, ApiResponse<SyncStatusDto>>
    {
        public GetSyncStatusQueryHandler(
            EmailIngestionDbContext context,
            ILogger<GetSyncStatusQueryHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<SyncStatusDto>> Handle(
            GetSyncStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var syncStates = await Context.SyncStates
                    .Where(s => s.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                var pendingCount = await Context.ParsedTransactions
                    .CountAsync(p => p.UserId == request.UserId && p.Status == ParsedTransactionStatus.Pending,
                        cancellationToken);

                var hasLocalTokens = await Context.UserTokens
                    .AnyAsync(t => t.UserId == request.UserId, cancellationToken);

                var dto = new SyncStatusDto
                {
                    UserId = request.UserId,
                    IsGmailConnected = request.HasGmailAccess || hasLocalTokens,
                    LastSyncAt = syncStates.Any() ? syncStates.Max(s => s.LastSyncAt) : null,
                    TotalEmailsProcessed = syncStates.Sum(s => s.TotalEmailsProcessed),
                    TotalTransactionsParsed = syncStates.Sum(s => s.TotalTransactionsParsed),
                    TotalTransactionsConfirmed = syncStates.Sum(s => s.TotalTransactionsConfirmed),
                    PendingReviewCount = pendingCount,
                    CategorySyncInfo = syncStates.Select(s => new CategorySyncInfo
                    {
                        Category = s.Category,
                        LastSyncAt = s.LastSyncAt,
                        EmailsProcessed = s.TotalEmailsProcessed,
                        TransactionsParsed = s.TotalTransactionsParsed
                    }).ToList()
                };

                return ApiResponse<SyncStatusDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching sync status for user {UserId}", request.UserId);
                return ApiResponse<SyncStatusDto>.ErrorResult($"Error fetching sync status: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Returns paginated parsed transactions for a user, filterable by status.
    /// </summary>
    public class GetParsedTransactionsQuery : IRequest<ApiResponse<PaginatedParsedTransactionsDto>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? StatusFilter { get; set; } // Pending, Confirmed, Rejected
        public float? MinConfidence { get; set; }
    }

    public class PaginatedParsedTransactionsDto
    {
        public List<ParsedTransactionDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetParsedTransactionsQueryHandler :
        BaseRequestHandler<GetParsedTransactionsQuery, ApiResponse<PaginatedParsedTransactionsDto>>
    {
        public GetParsedTransactionsQueryHandler(
            EmailIngestionDbContext context,
            ILogger<GetParsedTransactionsQueryHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<PaginatedParsedTransactionsDto>> Handle(
            GetParsedTransactionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = Context.ParsedTransactions
                    .Include(p => p.ProcessedEmail)
                    .Where(p => p.UserId == request.UserId)
                    .AsQueryable();

                // Status filter
                if (!string.IsNullOrEmpty(request.StatusFilter) && 
                    !request.StatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                    Enum.TryParse<ParsedTransactionStatus>(request.StatusFilter, true, out var status))
                {
                    query = query.Where(p => p.Status == status);
                }

                // Confidence filter
                if (request.MinConfidence.HasValue)
                {
                    query = query.Where(p => p.ConfidenceScore >= request.MinConfidence.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(p => p.TransactionDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ParsedTransactionDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        ProcessedEmailId = p.ProcessedEmailId,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        TransactionType = p.TransactionType,
                        Category = p.Category,
                        Description = p.Description,
                        TransactionDate = p.TransactionDate,
                        MerchantName = p.MerchantName,
                        ReferenceNumber = p.ReferenceNumber,
                        SuggestedAccountId = p.SuggestedAccountId,
                        Status = p.Status.ToString(),
                        ConfidenceScore = p.ConfidenceScore,
                        Source = p.Source,
                        ConfirmedTransactionId = p.ConfirmedTransactionId,
                        EmailSubject = p.ProcessedEmail != null ? p.ProcessedEmail.Subject : null,
                        EmailSender = p.ProcessedEmail != null ? p.ProcessedEmail.SenderEmail : null,
                        EmailDate = p.ProcessedEmail != null ? p.ProcessedEmail.EmailDate : null,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                var result = new PaginatedParsedTransactionsDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };

                return ApiResponse<PaginatedParsedTransactionsDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching parsed transactions for user {UserId}", request.UserId);
                return ApiResponse<PaginatedParsedTransactionsDto>.ErrorResult(
                    $"Error fetching transactions: {ex.Message}");
            }
        }
    }

    public class ResetConfirmedTransactionsCommand : IRequest<ApiResponse<int>>
    {
        public Guid UserId { get; set; }
    }

    public class ResetConfirmedTransactionsCommandHandler :
        BaseRequestHandler<ResetConfirmedTransactionsCommand, ApiResponse<int>>
    {
        public ResetConfirmedTransactionsCommandHandler(
            EmailIngestionDbContext context,
            ILogger<ResetConfirmedTransactionsCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<int>> Handle(
            ResetConfirmedTransactionsCommand request, CancellationToken cancellationToken)
        {
            var txns = await Context.ParsedTransactions
                .Where(p => p.UserId == request.UserId && 
                            p.Status == ParsedTransactionStatus.Confirmed)
                .ToListAsync(cancellationToken);

            int count = 0;
            foreach (var txn in txns)
            {
                txn.GetType().GetProperty("Status")?.SetValue(txn, ParsedTransactionStatus.Pending);
                count++;
            }

            await Context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.SuccessResult(count, $"{count} transactions reset to pending.");
        }
    }
}

using AutoMapper;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.EmailIngestion.Application.Common;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects.Response;
using PersonalFinance.Services.EmailIngestion.Domain.Entities;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.EmailIngestion.Application.Commands
{
    /// <summary>
    /// Confirms a single parsed transaction and publishes it to the Transactions service.
    /// </summary>
    public class ConfirmParsedTransactionCommand : IRequest<ApiResponse<ParsedTransactionDto>>
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
    }

    public class ConfirmParsedTransactionCommandHandler :
        BaseRequestHandler<ConfirmParsedTransactionCommand, ApiResponse<ParsedTransactionDto>>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ConfirmParsedTransactionCommandHandler(
            EmailIngestionDbContext context,
            ILogger<ConfirmParsedTransactionCommandHandler> logger,
            IMapper mapper,
            IPublishEndpoint publishEndpoint) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<ApiResponse<ParsedTransactionDto>> Handle(
            ConfirmParsedTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var parsedTxn = await Context.ParsedTransactions
                    .Include(p => p.ProcessedEmail)
                    .FirstOrDefaultAsync(p => p.Id == request.TransactionId && p.UserId == request.UserId,
                        cancellationToken);

                if (parsedTxn == null)
                    return ApiResponse<ParsedTransactionDto>.ErrorResult("Parsed transaction not found");

                if (parsedTxn.Status != ParsedTransactionStatus.Pending)
                    return ApiResponse<ParsedTransactionDto>.ErrorResult("Transaction has already been processed");

                if (request.AccountId == Guid.Empty)
                    return ApiResponse<ParsedTransactionDto>.ErrorResult("Please select a target account for this transaction.");

                var txnId = Guid.NewGuid();

                // Publish the external confirmation event
                await _publishEndpoint.Publish(new ExternalTransactionConfirmedEvent
                {
                    TransactionId = txnId,
                    ExternalId = parsedTxn.Id,
                    UserId = parsedTxn.UserId,
                    AccountId = request.AccountId,
                    Amount = parsedTxn.Amount,
                    Currency = parsedTxn.Currency,
                    TransactionType = parsedTxn.TransactionType,
                    Description = parsedTxn.Description,
                    Category = parsedTxn.Category,
                    TransactionDate = parsedTxn.TransactionDate
                }, cancellationToken);

                parsedTxn.Confirm(txnId);
                parsedTxn.UpdateDetails(null, null, null, null, request.AccountId);

                // Also link the processed email
                if (parsedTxn.ProcessedEmail != null)
                    parsedTxn.ProcessedEmail.LinkTransaction(parsedTxn.ConfirmedTransactionId!.Value);

                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Confirmed parsed transaction {TxnId} for user {UserId}",
                    parsedTxn.Id, request.UserId);

                var dto = MapToDto(parsedTxn);
                return ApiResponse<ParsedTransactionDto>.SuccessResult(dto, "Transaction confirmed and published");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error confirming parsed transaction {TxnId}", request.TransactionId);
                return ApiResponse<ParsedTransactionDto>.ErrorResult($"Error confirming transaction: {ex.Message}");
            }
        }

        private ParsedTransactionDto MapToDto(ParsedTransaction txn)
        {
            return new ParsedTransactionDto
            {
                Id = txn.Id,
                UserId = txn.UserId,
                ProcessedEmailId = txn.ProcessedEmailId,
                Amount = txn.Amount,
                Currency = txn.Currency,
                TransactionType = txn.TransactionType,
                Category = txn.Category,
                Description = txn.Description,
                TransactionDate = txn.TransactionDate,
                MerchantName = txn.MerchantName,
                ReferenceNumber = txn.ReferenceNumber,
                SuggestedAccountId = txn.SuggestedAccountId,
                Status = txn.Status.ToString(),
                ConfidenceScore = txn.ConfidenceScore,
                Source = txn.Source,
                ConfirmedTransactionId = txn.ConfirmedTransactionId,
                EmailSubject = txn.ProcessedEmail?.Subject,
                EmailSender = txn.ProcessedEmail?.SenderEmail,
                EmailDate = txn.ProcessedEmail?.EmailDate,
                CreatedAt = txn.CreatedAt
            };
        }
    }

    /// <summary>
    /// Bulk confirms all parsed transactions above a confidence threshold.
    /// </summary>
    public class BulkConfirmTransactionsCommand : IRequest<ApiResponse<BulkConfirmResultDto>>
    {
        public Guid UserId { get; set; }
        public float MinConfidenceScore { get; set; } = 0.9f;
        public Guid AccountId { get; set; }
    }

    public class BulkConfirmResultDto
    {
        public int TotalConfirmed { get; set; }
        public int TotalSkipped { get; set; }
        public int TotalErrors { get; set; }
    }

    public class BulkConfirmTransactionsCommandHandler :
        BaseRequestHandler<BulkConfirmTransactionsCommand, ApiResponse<BulkConfirmResultDto>>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public BulkConfirmTransactionsCommandHandler(
            EmailIngestionDbContext context,
            ILogger<BulkConfirmTransactionsCommandHandler> logger,
            IMapper mapper,
            IPublishEndpoint publishEndpoint) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<ApiResponse<BulkConfirmResultDto>> Handle(
            BulkConfirmTransactionsCommand request, CancellationToken cancellationToken)
        {
            var result = new BulkConfirmResultDto();

            try
            {
                if (request.AccountId == Guid.Empty)
                    return ApiResponse<BulkConfirmResultDto>.ErrorResult("Please select a target account for bulk confirmation.");

                var pendingTxns = await Context.ParsedTransactions
                    .Where(p => p.UserId == request.UserId &&
                                p.Status == ParsedTransactionStatus.Pending &&
                                p.ConfidenceScore >= request.MinConfidenceScore)
                    .ToListAsync(cancellationToken);

                Logger.LogInformation(
                    "Bulk confirming {Count} transactions with confidence >= {Score} for user {UserId}",
                    pendingTxns.Count, request.MinConfidenceScore, request.UserId);

                foreach (var txn in pendingTxns)
                {
                    try
                    {
                        var txnId = Guid.NewGuid();

                        await _publishEndpoint.Publish(new ExternalTransactionConfirmedEvent
                        {
                            TransactionId = txnId,
                            ExternalId = txn.Id,
                            UserId = txn.UserId,
                            AccountId = request.AccountId,
                            Amount = txn.Amount,
                            Currency = txn.Currency,
                            TransactionType = txn.TransactionType,
                            Description = txn.Description,
                            Category = txn.Category,
                            TransactionDate = txn.TransactionDate
                        }, cancellationToken);

                        txn.Confirm(txnId);
                        txn.UpdateDetails(null, null, null, null, request.AccountId);
                        result.TotalConfirmed++;
                    }
                    catch (Exception ex)
                    {
                        result.TotalErrors++;
                        Logger.LogWarning(ex, "Error confirming transaction {TxnId} during bulk confirm", txn.Id);
                    }
                }

                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Bulk confirm completed: {Confirmed} confirmed, {Errors} errors",
                    result.TotalConfirmed, result.TotalErrors);

                return ApiResponse<BulkConfirmResultDto>.SuccessResult(result,
                    $"Bulk confirm completed: {result.TotalConfirmed} transactions confirmed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during bulk confirm for user {UserId}", request.UserId);
                return ApiResponse<BulkConfirmResultDto>.ErrorResult($"Bulk confirm failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Rejects a single parsed transaction.
    /// </summary>
    public class RejectParsedTransactionCommand : IRequest<ApiResponse<ParsedTransactionDto>>
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
    }

    public class RejectParsedTransactionCommandHandler :
        BaseRequestHandler<RejectParsedTransactionCommand, ApiResponse<ParsedTransactionDto>>
    {
        public RejectParsedTransactionCommandHandler(
            EmailIngestionDbContext context,
            ILogger<RejectParsedTransactionCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<ParsedTransactionDto>> Handle(
            RejectParsedTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var parsedTxn = await Context.ParsedTransactions
                    .FirstOrDefaultAsync(p => p.Id == request.TransactionId && p.UserId == request.UserId,
                        cancellationToken);

                if (parsedTxn == null)
                    return ApiResponse<ParsedTransactionDto>.ErrorResult("Parsed transaction not found");

                parsedTxn.Reject();
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Rejected parsed transaction {TxnId} for user {UserId}",
                    parsedTxn.Id, request.UserId);

                return ApiResponse<ParsedTransactionDto>.SuccessResult(null!, "Transaction rejected");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error rejecting parsed transaction {TxnId}", request.TransactionId);
                return ApiResponse<ParsedTransactionDto>.ErrorResult($"Error rejecting transaction: {ex.Message}");
            }
        }
    }
}

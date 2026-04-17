using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalFinance.Services.EmailIngestion.Application.Commands;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects.Response;
using PersonalFinance.Services.EmailIngestion.Application.Queries;

namespace PersonalFinance.Services.EmailIngestion.Controllers
{
    /// <summary>
    /// Handles Gmail email ingestion, transaction parsing, and review operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmailIngestionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmailIngestionController> _logger;

        public EmailIngestionController(IMediator mediator, ILogger<EmailIngestionController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Triggers a manual Gmail sync for the authenticated user.
        /// </summary>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(ApiResponse<EmailSyncResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<EmailSyncResultDto>>> SyncGmail(
            [FromBody] SyncGmailRequest request)
        {
            try
            {
                _logger.LogInformation("Manual Gmail sync triggered for user {UserId}", request.UserId);

                var command = new SyncGmailTransactionsCommand
                {
                    UserId = request.UserId,
                    GmailAccessToken = request.GmailAccessToken,
                    GmailRefreshToken = request.GmailRefreshToken,
                    CategoryFilter = request.CategoryFilter
                };

                var result = await _mediator.Send(command);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual Gmail sync");
                return StatusCode(500, ApiResponse<EmailSyncResultDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Gets the sync status and connection info for a user.
        /// </summary>
        [HttpGet("sync-status/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SyncStatusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<SyncStatusDto>>> GetSyncStatus(
            Guid userId, [FromQuery] bool hasGmailAccess = false)
        {
            try
            {
                var query = new GetSyncStatusQuery
                {
                    UserId = userId,
                    HasGmailAccess = hasGmailAccess
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sync status for user {UserId}", userId);
                return StatusCode(500, ApiResponse<SyncStatusDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Gets paginated parsed transactions for review.
        /// </summary>
        [HttpGet("parsed-transactions/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedParsedTransactionsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedParsedTransactionsDto>>> GetParsedTransactions(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] float? minConfidence = null)
        {
            try
            {
                var query = new GetParsedTransactionsQuery
                {
                    UserId = userId,
                    Page = page,
                    PageSize = pageSize,
                    StatusFilter = status,
                    MinConfidence = minConfidence
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching parsed transactions for user {UserId}", userId);
                return StatusCode(500,
                    ApiResponse<PaginatedParsedTransactionsDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Confirms a single parsed transaction and publishes it to the Transactions service.
        /// </summary>
        [HttpPost("confirm/{transactionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ParsedTransactionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ParsedTransactionDto>>> ConfirmTransaction(
            Guid transactionId, [FromBody] ConfirmTransactionRequest request,
            [FromQuery] Guid userId)
        {
            try
            {
                var command = new ConfirmParsedTransactionCommand
                {
                    TransactionId = transactionId,
                    UserId = userId,
                    AccountId = request.AccountId
                };

                var result = await _mediator.Send(command);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming transaction {TxnId}", transactionId);
                return StatusCode(500,
                    ApiResponse<ParsedTransactionDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Rejects a single parsed transaction.
        /// </summary>
        [HttpPost("reject/{transactionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ParsedTransactionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ParsedTransactionDto>>> RejectTransaction(
            Guid transactionId, [FromQuery] Guid userId)
        {
            try
            {
                var command = new RejectParsedTransactionCommand
                {
                    TransactionId = transactionId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting transaction {TxnId}", transactionId);
                return StatusCode(500,
                    ApiResponse<ParsedTransactionDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Bulk confirms all pending transactions with confidence above the threshold.
        /// </summary>
        [HttpPost("bulk-confirm")]
        [ProducesResponseType(typeof(ApiResponse<BulkConfirmResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<BulkConfirmResultDto>>> BulkConfirm(
            [FromBody] BulkConfirmRequest request, [FromQuery] Guid userId)
        {
            try
            {
                _logger.LogInformation(
                    "Bulk confirm triggered for user {UserId} with min confidence {Score}",
                    userId, request.MinConfidenceScore);

                var command = new BulkConfirmTransactionsCommand
                {
                    UserId = userId,
                    MinConfidenceScore = request.MinConfidenceScore,
                    AccountId = request.AccountId
                };

                var result = await _mediator.Send(command);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk confirm for user {UserId}", userId);
                return StatusCode(500, ApiResponse<BulkConfirmResultDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Resets confirmed transactions for a user back to pending.
        /// </summary>
        [HttpPost("reset-confirmed/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> ResetConfirmed(Guid userId)
        {
            try
            {
                var command = new ResetConfirmedTransactionsCommand { UserId = userId };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting confirmed transactions for user {UserId}", userId);
                return StatusCode(500, ApiResponse<int>.ErrorResult("An internal error occurred"));
            }
        }
    }

    /// <summary>
    /// Request model for manual Gmail sync.
    /// </summary>
    public class SyncGmailRequest
    {
        public Guid UserId { get; set; }
        public string GmailAccessToken { get; set; } = string.Empty;
        public string GmailRefreshToken { get; set; } = string.Empty;
        public string? CategoryFilter { get; set; }
    }
}


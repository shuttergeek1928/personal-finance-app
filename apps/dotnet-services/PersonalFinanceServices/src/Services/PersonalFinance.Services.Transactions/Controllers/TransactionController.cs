using MediatR;

using Microsoft.AspNetCore.Mvc;

using PersonalFinance.Services.Transactions.Application.Commands;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Queries;

namespace PersonalFinance.Services.Transactions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(IMediator mediator, ILogger<TransactionController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Income/deposit transaction
        /// </summary>
        [HttpPost("income/deposit")]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> CreateIncomeTransaction([FromBody] CreateIncomeTransactionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating income transaction for user ID: {userId} (Account: {accountId}, Card: {cardId})",
                    request.UserId, request.AccountId, request.CreditCardId);

                var command = new CreateIncomeTransactionCommand
                {
                    UserId = request.UserId,
                    AccountId = request.AccountId,
                    CreditCardId = request.CreditCardId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    Category = request.Category,
                    TransactionDate = request.TransactionDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetTransaction), new { id = result.Data!.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating income transaction");
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Expense/withdraw transaction
        /// </summary>
        [HttpPost("expense/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> CreateExpenseTransaction([FromBody] CreateExpenseTransactionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating expense transaction for user ID: {userId} (Account: {accountId}, Card: {cardId})",
                    request.UserId, request.AccountId, request.CreditCardId);

                var command = new CreateExpenseTransactionCommand
                {
                    UserId = request.UserId,
                    AccountId = request.AccountId,
                    CreditCardId = request.CreditCardId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    Category = request.Category,
                    TransactionDate = request.TransactionDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetTransaction), new { id = result.Data!.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense transaction");
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Transfer to another account transaction
        /// </summary>
        [HttpPost("transfer")]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> CreateTransferTransaction([FromBody] CreateTransferTransactionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating transfer transaction for user ID: {userId} (FromAcc: {fromAcc}, FromCard: {fromCard}, ToAcc: {toAcc}, ToCard: {toCard})",
                    request.UserId, request.AccountId, request.CreditCardId, request.ToAccountId, request.ToCreditCardId);

                var command = new CreateTransferTransactionCommand
                {
                    UserId = request.UserId,
                    FromAccountId = request.AccountId,
                    FromCreditCardId = request.CreditCardId,
                    ToAccountId = request.ToAccountId,
                    ToCreditCardId = request.ToCreditCardId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    Category = request.Category,
                    TransactionDate = request.TransactionDate
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetTransaction), new { id = result.Data!.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer transaction");
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> GetTransaction(Guid id)
        {
            try
            {
                var query = new GetTransactionByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transaction {Id}", id);
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionTransferObject>>>> GetTransactionsByUserId(Guid userId)
        {
            try
            {
                _logger.LogInformation("Fetching transactions for user ID: {userId}", userId);
                var query = new GetTransactionsByUserIdQuery(userId);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for user ID: {userId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<TransactionTransferObject>>.ErrorResult("An internal error occurred"));
            }
        }

        [HttpGet("user/{userId:guid}/paged")]
        public async Task<ActionResult<PaginatedApiResponse<IEnumerable<TransactionTransferObject>>>> GetPaginatedTransactions(
            Guid userId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 15,
            [FromQuery] string? sourceType = null,
            [FromQuery] Guid? cardId = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] string? period = null,
            [FromQuery] int? type = null)
        {
            try
            {
                _logger.LogInformation("Fetching paginated transactions for user ID: {userId}", userId);
                var query = new GetPaginatedTransactionsQuery
                {
                    UserId = userId,
                    Page = page,
                    PageSize = pageSize,
                    SourceType = sourceType,
                    CardId = cardId,
                    SearchQuery = searchQuery,
                    Period = period,
                    Type = type
                };

                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated transactions for user ID: {userId}", userId);
                return StatusCode(500, new PaginatedApiResponse<IEnumerable<TransactionTransferObject>>
                {
                    Success = false,
                    Message = "An internal error occurred",
                    Errors = new List<string> { "An internal error occurred" }
                });
            }
        }
        
        [HttpGet("user/{userId:guid}/dashboard-summary")]
        public async Task<ActionResult<ApiResponse<TransactionDashboardSummaryDto>>> GetDashboardSummary(
            Guid userId,
            [FromQuery] string? period = "THIS_MONTH")
        {
            try
            {
                _logger.LogInformation("Fetching dashboard summary for user ID: {userId}", userId);
                var query = new GetTransactionDashboardSummaryQuery
                {
                    UserId = userId,
                    Period = period
                };

                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard summary for user ID: {userId}", userId);
                return StatusCode(500, new ApiResponse<TransactionDashboardSummaryDto>
                {
                    Success = false,
                    Message = "An internal error occurred",
                    Errors = new List<string> { "An internal error occurred" }
                });
            }
        }
    }
}
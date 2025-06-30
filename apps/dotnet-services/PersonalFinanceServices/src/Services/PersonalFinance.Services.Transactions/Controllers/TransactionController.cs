using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Services.Transactions.Application.Commands;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Queries;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

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
        /// Register a new user
        /// </summary>
        /// <param name="request">User registration details</param>
        /// <returns>Created user information</returns>
        [HttpPost("create-income")]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> CreateIncomeTransaction([FromBody] CreateIncomeTransactionRequest request)
        {
            try
            {
                _logger.LogInformation("Transaction creating attempt for user ID: {userId} to account ID {accountId}", request.UserId, request.AccountId);

                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<TransactionTransferObject>.ErrorResult(errors));
                }

                // Create command from request
                var command = new CreateIncomeTransactionCommand()
                {
                    UserId = request.UserId,
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    Category = request.Category,
                    TransactionDate = request.TransactionDate,
                    Status = request.Status,
                    RejectionReason = request.RejectionReason
                };

                // Execute command
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("Transaction created successfully for user ID: {Id} to account ID {accountId}", request.UserId, request.AccountId);
                    return CreatedAtAction(nameof(CreateIncomeTransaction), result.Data!.Id, result);
                }

                _logger.LogInformation("Transaction failed for user ID: {Id} to account ID {accountId}", request.UserId, request.AccountId);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during creating transaction for: {UserId} to account {accountId}", request.UserId, request.AccountId);
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TransactionTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TransactionTransferObject>>> GetTransation(Guid id)
        {
            try
            {
                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<TransactionTransferObject>.ErrorResult(errors));
                }

                // Create command from request
                var query = new GetTransactionByIdQuery(id);

                // Execute command
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    _logger.LogInformation("Transaction fetched successfully for transaction ID: {Id}", id);
                    return Ok(result);
                }

                _logger.LogInformation("Transaction fetching failed for transaction ID: {Id}", id);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during feting a transaction for ID: {id}", id);
                return StatusCode(500, ApiResponse<TransactionTransferObject>.ErrorResult("An internal error occurred"));
            }
        }
    }
}
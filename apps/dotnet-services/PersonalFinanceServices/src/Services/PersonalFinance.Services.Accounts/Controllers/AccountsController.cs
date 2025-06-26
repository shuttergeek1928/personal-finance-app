using MediatR;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Services.Accounts.Application.Commands;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Application.Queries;
using System.Linq.Dynamic.Core;

namespace PersonalFinance.Services.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <param name="request">Account creation details/param>
        /// <returns>Created account information</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                _logger.LogInformation("Create account attempt for account number: {AccountNumber}", request.AccountNumber);

                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AccountTransferObject>.ErrorResult(errors));
                }

                // Create account from request
                var command = new CreateAccountCommand(
                    request.Name,
                    request.Type,
                    request.Balance,
                    request.UserId,
                    request.AccountNumber,
                    request.Description);

                // Execute command
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("Account created successfully: {AccountNumber}", request.AccountNumber);
                    return CreatedAtAction(nameof(CreateAccount), new { id = result.Data!.Id }, result);
                }

                _logger.LogWarning("Account creation failed for account number: {AccountNUmber}. Errors: {Errors}",
                    request.AccountNumber, string.Join(", ", result.Errors));

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during creating account for account number: {AccountNumber}", request.AccountNumber);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get account by account number
        /// </summary>
        /// <param name="number">Account Number</param>
        /// <returns>Account details</returns>
        [HttpGet("{number}")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> GetByAccountNumber(string number)
        {
            try
            {
                var query = new GetAccountByAccountNumberQuery(number);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account: {accountNumber}", number);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get account by account id
        /// </summary>
        /// <param name="id">Account Id</param>
        /// <returns>Account details</returns>
        [HttpGet("userid/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<AccountTransferObject>>>> GetByUserId(Guid userId)
        {
            try
            {
                var query = new GetAccountByUserIdQuery(userId);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for userId: {userId}", userId);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }
        
        /// <summary>
        /// Get account by account id
        /// </summary>
        /// <param name="id">Account Id</param>
        /// <returns>Account details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> GetByAccountId(Guid id)
        {
            try
            {
                var query = new GetAccountByAccountIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account: {accountId}", id);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Deposits the money
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="request">Update balance details</param>
        /// <returns>Updated account with deposited money</returns>
        [HttpPut("{id:guid}/deposit")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> DepositMoney(Guid id, [FromBody] UpdateBalanceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AccountTransferObject>.ErrorResult(errors));
                }

                var command = new UpdateBalanceCommand(id, request.Balance, request.IsDeposit);

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error depositing the money: {AccountNumber}", request.AccountNumber);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Withdraws the money
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="request">Update balance details</param>
        /// <returns>Updated account with money withdrawn</returns>
        [HttpPut("{id:guid}/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> WithdrawMoney(Guid id, [FromBody] UpdateBalanceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AccountTransferObject>.ErrorResult(errors));
                }

                var command = new UpdateBalanceCommand(id, request.Balance, false);

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing the money: {AccountNumber}", request.AccountNumber);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Transfers the money
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="request">Update balance details</param>
        /// <returns>Updated account with money withdrawn</returns>
        [HttpPut("transfer")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> TransferMoney([FromBody] TransferMoneyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<AccountTransferObject>.ErrorResult(errors));
                }

                if (request.ToAccountId == null)
                    return BadRequest(ApiResponse<AccountTransferObject>.ErrorResult("ToAccountId cannot be null"));

                var command = new TransferFundCommand(request.Id, request.Balance, request.ToAccountId);

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing the money: {AccountNumber}", request.AccountNumber);
                return StatusCode(500, ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Sets the default account for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountNumber"></param>
        /// <returns>Returns the account object which is set to default</returns>
        [HttpPut("{userId:guid}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AccountTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AccountTransferObject>>> SetDefaultAccount(Guid userId, string accountNumber)
        {
            try
            {
                var command = new SetDefaultAccountCommand(userId, accountNumber);

                var result = await _mediator.Send(command);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default account for user: {UserId} with account number: {AccountNumber}", userId, accountNumber);
                return ApiResponse<AccountTransferObject>.ErrorResult("An internal error occurred while setting default account");
            }
        }
    }
}
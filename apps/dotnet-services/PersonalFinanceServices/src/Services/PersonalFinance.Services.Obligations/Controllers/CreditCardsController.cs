using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Services.Obligations.Application.Commands;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Queries;
using System.Security.Claims;

namespace PersonalFinance.Services.Obligations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CreditCardsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreditCardsController> _logger;

        public CreditCardsController(IMediator mediator, ILogger<CreditCardsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreditCardDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCreditCards()
        {
            var userId = GetUserId();
            var query = new GetCreditCardsByUserIdQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreditCardDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CreditCardDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCreditCard([FromBody] CreateCreditCardRequest request)
        {
            var userId = GetUserId();
            
            var command = new AddCreditCardCommand
            {
                UserId = userId,
                BankName = request.BankName,
                CardName = request.CardName,
                Last4Digits = request.Last4Digits,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                NetworkProvider = request.NetworkProvider,
                TotalLimit = request.TotalLimit,
                OutstandingAmount = request.OutstandingAmount
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetCreditCards), new { id = result.Data?.Id }, result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CreditCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CreditCardDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreditCardDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCreditCard(Guid id, [FromBody] UpdateCreditCardRequest request)
        {
            var command = new UpdateCreditCardCommand
            {
                Id = id,
                BankName = request.BankName,
                CardName = request.CardName,
                Last4Digits = request.Last4Digits,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                NetworkProvider = request.NetworkProvider,
                TotalLimit = request.TotalLimit,
                OutstandingAmount = request.OutstandingAmount
            };

            var result = await _mediator.Send(command);

            if (result.Success)
                return Ok(result);

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCreditCard(Guid id)
        {
            var command = new DeleteCreditCardCommand(id);
            var result = await _mediator.Send(command);

            if (result.Success)
                return Ok(result);

            return NotFound(result);
        }
    }
}

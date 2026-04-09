using MediatR;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Services.Obligations.Application.Commands;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Queries;

namespace PersonalFinance.Services.Obligations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ObligationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ObligationsController> _logger;

        public ObligationsController(IMediator mediator, ILogger<ObligationsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Liability Endpoints

        /// <summary>
        /// Create a new liability (loan/EMI)
        /// </summary>
        [HttpPost("liabilities")]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LiabilityDto>>> CreateLiability([FromBody] CreateLiabilityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<LiabilityDto>.ErrorResult(errors));
                }

                var command = new CreateLiabilityCommand(
                    request.Name, request.Type, request.LenderName,
                    request.PrincipalAmount, request.InterestRate, request.TenureMonths,
                    request.StartDate, request.UserId, request.AccountId);

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetLiabilityById), new { id = result.Data!.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating liability");
                return StatusCode(500, ApiResponse<LiabilityDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Update an existing liability
        /// </summary>
        [HttpPut("liabilities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LiabilityDto>>> UpdateLiability(Guid id, [FromBody] UpdateLiabilityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<LiabilityDto>.ErrorResult(errors));
                }

                var command = new UpdateLiabilityCommand(
                    id, request.Name, request.Type, request.LenderName,
                    request.PrincipalAmount, request.InterestRate, request.TenureMonths,
                    request.StartDate, request.AccountId);

                var result = await _mediator.Send(command);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating liability: {Id}", id);
                return StatusCode(500, ApiResponse<LiabilityDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Delete (soft-delete) a liability
        /// </summary>
        [HttpDelete("liabilities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LiabilityDto>>> DeleteLiability(Guid id)
        {
            try
            {
                var command = new DeleteLiabilityCommand(id);
                var result = await _mediator.Send(command);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting liability: {Id}", id);
                return StatusCode(500, ApiResponse<LiabilityDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get all liabilities for a user
        /// </summary>
        [HttpGet("liabilities/user/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LiabilityDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<LiabilityDto>>>> GetLiabilitiesByUserId(Guid userId)
        {
            try
            {
                var query = new GetLiabilitiesByUserIdQuery(userId);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liabilities for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<List<LiabilityDto>>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get a liability by ID
        /// </summary>
        [HttpGet("liabilities/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LiabilityDto>>> GetLiabilityById(Guid id)
        {
            try
            {
                var query = new GetLiabilityByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving liability: {Id}", id);
                return StatusCode(500, ApiResponse<LiabilityDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get full amortization schedule for a liability
        /// </summary>
        [HttpGet("liabilities/{id:guid}/amortization")]
        [ProducesResponseType(typeof(ApiResponse<AmortizationScheduleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AmortizationScheduleDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AmortizationScheduleDto>>> GetAmortizationSchedule(Guid id)
        {
            try
            {
                var query = new GetAmortizationScheduleQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing amortization for liability: {Id}", id);
                return StatusCode(500, ApiResponse<AmortizationScheduleDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Record a payment against a liability (reduces outstanding balance)
        /// </summary>
        [HttpPost("liabilities/{id:guid}/payment")]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LiabilityDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LiabilityDto>>> MakePayment(Guid id, [FromBody] MakePaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<LiabilityDto>.ErrorResult(errors));
                }

                var command = new MakePaymentCommand(id, request.Amount, request.Note);
                var result = await _mediator.Send(command);

                if (result.Success) return Ok(result);

                // Distinguish between not-found and validation errors
                if (result.Errors.Any(e => e.Contains("not found")))
                    return NotFound(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment for liability: {Id}", id);
                return StatusCode(500, ApiResponse<LiabilityDto>.ErrorResult("An internal error occurred"));
            }
        }

        #endregion

        #region Subscription Endpoints

        /// <summary>
        /// Create a new subscription
        /// </summary>
        [HttpPost("subscriptions")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<SubscriptionDto>.ErrorResult(errors));
                }

                var command = new CreateSubscriptionCommand(
                    request.Name, request.Type, request.Provider,
                    request.Amount, request.BillingCycle, request.StartDate,
                    request.UserId, request.AutoRenew, request.EndDate);

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetSubscriptionById), new { id = result.Data!.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Update an existing subscription
        /// </summary>
        [HttpPut("subscriptions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<SubscriptionDto>.ErrorResult(errors));
                }

                var command = new UpdateSubscriptionCommand(
                    id, request.Name, request.Type, request.Provider,
                    request.Amount, request.BillingCycle, request.StartDate,
                    request.AutoRenew, request.EndDate);

                var result = await _mediator.Send(command);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription: {Id}", id);
                return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Delete (soft-delete) a subscription
        /// </summary>
        [HttpDelete("subscriptions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> DeleteSubscription(Guid id)
        {
            try
            {
                var command = new DeleteSubscriptionCommand(id);
                var result = await _mediator.Send(command);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription: {Id}", id);
                return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get all subscriptions for a user
        /// </summary>
        [HttpGet("subscriptions/user/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<SubscriptionDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<SubscriptionDto>>>> GetSubscriptionsByUserId(Guid userId)
        {
            try
            {
                var query = new GetSubscriptionsByUserIdQuery(userId);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<List<SubscriptionDto>>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get a subscription by ID
        /// </summary>
        [HttpGet("subscriptions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetSubscriptionById(Guid id)
        {
            try
            {
                var query = new GetSubscriptionByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success) return Ok(result);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription: {Id}", id);
                return StatusCode(500, ApiResponse<SubscriptionDto>.ErrorResult("An internal error occurred"));
            }
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// Get combined obligation dashboard for a user
        /// </summary>
        [HttpGet("dashboard/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ObligationDashboardDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ObligationDashboardDto>>> GetDashboard(Guid userId)
        {
            try
            {
                var query = new GetObligationDashboardQuery(userId);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving obligation dashboard for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<ObligationDashboardDto>.ErrorResult("An internal error occurred"));
            }
        }

        #endregion
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Services.UserManagement.Application.Commands;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response;
using PersonalFinance.Services.UserManagement.Application.DTOs;
using PersonalFinance.Services.UserManagement.Application.Queries;
using System.Linq.Dynamic.Core;

namespace PersonalFinance.Services.UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">User registration details</param>
        /// <returns>Created user information</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserTransferObject>>> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                _logger.LogInformation("User registration attempt for email: {Email}", request.Email);

                // Validate the request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<UserTransferObject>.ErrorResult(errors));
                }

                // Create command from request
                var command = new RegisterUserCommand
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber
                };

                // Execute command
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Email}", request.Email);
                    return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
                }

                _logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}",
                    request.Email, string.Join(", ", result.Errors));

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
                return StatusCode(500, ApiResponse<UserTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserTransferObject>>> GetById(Guid id)
        {
            try
            {
                var query = new GetUserByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {UserId}", id);
                return StatusCode(500, ApiResponse<UserTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">User email address</param>
        /// <returns>User information</returns>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserTransferObject>>> GetByEmail(string email)
        {
            try
            {
                var query = new GetUserByEmailQuery(email);
                var result = await _mediator.Send(query);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return StatusCode(500, ApiResponse<UserTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Updated profile information</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{id:guid}/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserTransferObject>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserTransferObject>>> UpdateProfile(Guid id, [FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<UserTransferObject>.ErrorResult(errors));
                }

                var command = new UpdateUserProfileCommand
                {
                    UserId = id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Currency = request.Currency,
                    TimeZone = request.TimeZone,
                    Language = request.Language
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", id);
                return StatusCode(500, ApiResponse<UserTransferObject>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Confirm user email
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/confirm-email")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmEmail(Guid id)
        {
            try
            {
                var command = new ConfirmEmailCommand(id);
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Get all users (admin only - pagination support)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserTransferObject>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<UserTransferObject>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var query = new GetUsersQuery
                {
                    PageNumber = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users list");
                return StatusCode(500, ApiResponse<PagedResult<UserTransferObject>>.ErrorResult("An internal error occurred"));
            }
        }

        /// <summary>
        /// Soft delete user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
        {
            try
            {
                var command = new DeleteUserCommand(id);
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An internal error occurred"));
            }
        }
    }
}
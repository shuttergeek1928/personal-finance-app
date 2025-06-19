using AutoMapper;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Domain.Entities;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using PersonalFinance.Shared.Contracts;

namespace PersonalFinance.Services.UserManagement.Application.Common
{
    /// <summary>  
    /// Base class for handling requests.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <typeparam name="TResponse">The type of the response.</typeparam>  
    public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        /// <summary>  
        /// The database context for user management.  
        /// </summary>  
        protected readonly UserManagementDbContext Context;

        /// <summary>  
        /// The logger instance for logging.  
        /// </summary>  
        protected readonly ILogger<BaseRequestHandler<TRequest, TResponse>> Logger;

        /// <summary>  
        /// The mapper instance for object mapping.  
        /// </summary>  
        protected readonly IMapper Mapper;

        /// <summary>  
        /// Initializes a new instance of the <see cref="BaseRequestHandler{TRequest, TResponse}"/> class.  
        /// </summary>  
        /// <param name="context">The database context.</param>  
        /// <param name="logger">The logger instance.</param>  
        /// <param name="mapper">The mapper instance.</param>  
        public BaseRequestHandler(UserManagementDbContext context, ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>  
        /// Handles the request asynchronously.  
        /// </summary>  
        /// <param name="request">The request to handle.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>  
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        /// <summary>  
        /// Checks if an email address already exists in the database.  
        /// </summary>  
        /// <param name="email">The email address to check.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the email exists; otherwise, false.</returns>  
        protected async Task<bool> EmailExistAsync(string email, CancellationToken cancellationToken = default)
        {
            return await Context.Users
                .AnyAsync(u => u.Email.Value == email.ToLower(), cancellationToken);
        }

        /// <summary>  
        /// Checks if a username already exists in the database.  
        /// </summary>  
        /// <param name="username">The username to check.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the username exists; otherwise, false.</returns>  
        protected async Task<bool> UsernameExistAsync(string username, CancellationToken cancellationToken = default)
        {
            return await Context.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.UserName == username, cancellationToken);
        }

        /// <summary>  
        /// Retrieves a role by its name if it exists in the Roles table.  
        /// </summary>  
        /// <param name="roleName">The name of the role to retrieve.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains the role if it exists; otherwise, null.</returns>  
        protected async Task<Role?> GetRoleByNameExistAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await Context.Roles
                .FirstOrDefaultAsync(u => u.Name == roleName, cancellationToken);
        }
    }

    /// <summary>  
    /// Base class for handling command requests.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <typeparam name="TResponse">The type of the response.</typeparam>  
    public abstract class BaseCommandHandler<TRequest, TResponse> : BaseRequestHandler<TRequest, TResponse>
           where TRequest : IRequest<TResponse>
           where TResponse : class
    {
        /// <summary>  
        /// The password hasher instance.  
        /// </summary>  
        protected readonly IPasswordHasher _passwordHasher;

        /// <summary>  
        /// Initializes a new instance of the <see cref="BaseCommandHandler{TRequest, TResponse}"/> class.  
        /// </summary>  
        /// <param name="context">The database context.</param>  
        /// <param name="logger">The logger instance.</param>  
        /// <param name="mapper">The mapper instance.</param>  
        /// <param name="passwordHasher">The password hasher instance.</param>  
        protected BaseCommandHandler(UserManagementDbContext context, ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper, IPasswordHasher passwordHasher) : base(context, logger, mapper)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>  
        /// Validates the strength of the provided password.  
        /// </summary>  
        /// <param name="password">The password to validate.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A <see cref="ValidationResult"/> indicating whether the password is valid and any associated errors.</returns>  
        protected async Task<ValidationResult> IsPasswordValidAsync(string password, CancellationToken cancellationToken = default)
        {
            var errors = new List<string>();

            // Validate password strength (you can implement more complex rules)  
            if (password.Length < 8)
            {
                errors.Add("Password must be at least 8 characters long");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Password must contain at least one uppercase letter");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Password must contain at least one number");
            }

            return await Task.FromResult(new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors
            });
        }

        /// <summary>  
        /// Compares a plain text password with a hashed password.  
        /// </summary>  
        /// <param name="password">The plain text password.</param>  
        /// <param name="hashedPassword">The hashed password.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the passwords match; otherwise, false.</returns>  
        protected async Task<bool> ComparePassword(string password, string hashedPassword, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => _passwordHasher.VerifyPassword(hashedPassword, password), cancellationToken);
        }
    }

    /// <summary>  
    /// Base class for handling query requests.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <typeparam name="TResponse">The type of the response.</typeparam>  
    public abstract class BaseQueryHandler<TRequest, TResponse> : BaseRequestHandler<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
       where TResponse : class
    {
        /// <summary>  
        /// Initializes a new instance of the <see cref="BaseQueryHandler{TRequest, TResponse}"/> class.  
        /// </summary>  
        /// <param name="context">The database context.</param>  
        /// <param name="logger">The logger instance.</param>  
        /// <param name="mapper">The mapper instance.</param>  
        protected BaseQueryHandler(
            UserManagementDbContext context,
            ILogger<BaseRequestHandler<TRequest, TResponse>> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }
    }

  
  /// <summary>  
    /// Represents the result of a validation operation.  
    /// </summary>  
    public class ValidationResult
    {
        /// <summary>  
        /// Gets or sets a value indicating whether the validation was successful.  
        /// </summary>  
        public bool IsValid { get; set; }

        /// <summary>  
        /// Gets or sets the list of validation errors.  
        /// </summary>  
        public List<string> Errors { get; set; } = new List<string>();
    }
}

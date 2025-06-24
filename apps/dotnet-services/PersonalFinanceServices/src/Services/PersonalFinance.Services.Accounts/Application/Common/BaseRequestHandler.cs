using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Accounts.Infrastructure.Data;

namespace PersonalFinance.Services.Accounts.Application.Common
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
        /// The database context for accounts.  
        /// </summary>  
        protected readonly AccountDbContext Context;

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
        public BaseRequestHandler(AccountDbContext context, ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper)
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
        /// Checks if an account already exists in the database.  
        /// </summary>  
        /// <param name="accountNumber">The email address to check.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the account exists; otherwise, false.</returns>  
        protected async Task<bool> AccountExistAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            return await Context.Accounts
                .AnyAsync(a => a.AccountNumber == accountNumber);
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
            AccountDbContext context,
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

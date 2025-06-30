using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;

namespace PersonalFinance.Services.Transactions.Application.Common
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
        protected readonly TransactionDbContext Context;

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
        public BaseRequestHandler(TransactionDbContext context, ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper)
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
        protected async Task<bool> FindTransaction(Guid id, CancellationToken cancellationToken = default)
        {
            return await Context.Transactions.FindAsync(id, cancellationToken) != null;
        }
    }
}

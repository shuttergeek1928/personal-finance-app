using AutoMapper;

using MediatR;

using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;

namespace PersonalFinance.Services.EmailIngestion.Application.Common
{
    /// <summary>
    /// Base class for handling requests in the EmailIngestion service.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        /// <summary>
        /// The database context for email ingestion.
        /// </summary>
        protected readonly EmailIngestionDbContext Context;

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
        public BaseRequestHandler(EmailIngestionDbContext context,
            ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper)
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
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}

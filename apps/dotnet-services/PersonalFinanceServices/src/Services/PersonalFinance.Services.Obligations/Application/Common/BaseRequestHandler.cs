using AutoMapper;

using MediatR;

using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Common
{
    /// <summary>
    /// Base class for handling command requests.
    /// </summary>
    public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        protected readonly ObligationDbContext Context;
        protected readonly ILogger<BaseRequestHandler<TRequest, TResponse>> Logger;
        protected readonly IMapper Mapper;

        public BaseRequestHandler(ObligationDbContext context, ILogger<BaseRequestHandler<TRequest, TResponse>> logger, IMapper mapper)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base class for handling query requests.
    /// </summary>
    public abstract class BaseQueryHandler<TRequest, TResponse> : BaseRequestHandler<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
       where TResponse : class
    {
        protected BaseQueryHandler(
            ObligationDbContext context,
            ILogger<BaseRequestHandler<TRequest, TResponse>> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }
    }
}

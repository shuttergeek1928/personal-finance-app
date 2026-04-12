using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Queries
{
    public class GetCreditCardsByUserIdQuery : IRequest<ApiResponse<IEnumerable<CreditCardDto>>>
    {
        public Guid UserId { get; set; }
        public GetCreditCardsByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetCreditCardsByUserIdQueryHandler : BaseRequestHandler<GetCreditCardsByUserIdQuery, ApiResponse<IEnumerable<CreditCardDto>>>
    {
        public GetCreditCardsByUserIdQueryHandler(
            ObligationDbContext context,
            ILogger<GetCreditCardsByUserIdQueryHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<IEnumerable<CreditCardDto>>> Handle(GetCreditCardsByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var creditCards = await Context.CreditCards
                    .AsNoTracking()
                    .Where(c => c.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                var dtos = Mapper.Map<IEnumerable<CreditCardDto>>(creditCards);
                return ApiResponse<IEnumerable<CreditCardDto>>.SuccessResult(dtos, "Credit Cards retrieved successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving credit cards for user {UserId}", request.UserId);
                return ApiResponse<IEnumerable<CreditCardDto>>.ErrorResult($"An error occurred while retrieving credit cards: {ex.Message}");
            }
        }
    }
}

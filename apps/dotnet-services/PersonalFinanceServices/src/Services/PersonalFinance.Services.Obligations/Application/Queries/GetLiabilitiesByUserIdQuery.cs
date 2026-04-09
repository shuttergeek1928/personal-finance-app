using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Queries
{
    public class GetLiabilitiesByUserIdQuery : IRequest<ApiResponse<List<LiabilityDto>>>
    {
        public Guid UserId { get; set; }

        public GetLiabilitiesByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetLiabilitiesByUserIdQueryHandler : BaseQueryHandler<GetLiabilitiesByUserIdQuery, ApiResponse<List<LiabilityDto>>>
    {
        public GetLiabilitiesByUserIdQueryHandler(ObligationDbContext context, ILogger<GetLiabilitiesByUserIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<List<LiabilityDto>>> Handle(GetLiabilitiesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var liabilities = await Context.Liabilities
                .Include(l => l.CreditCard)
                .Where(l => l.UserId == request.UserId && l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);

            if (!liabilities.Any())
            {
                Logger.LogInformation("No liabilities found for user: {UserId}", request.UserId);
                return ApiResponse<List<LiabilityDto>>.SuccessResult(new List<LiabilityDto>(), "No liabilities found");
            }

            var dtos = liabilities.Select(l => l.ToDto(Mapper)).ToList();
            return ApiResponse<List<LiabilityDto>>.SuccessResult(dtos);
        }
    }
}

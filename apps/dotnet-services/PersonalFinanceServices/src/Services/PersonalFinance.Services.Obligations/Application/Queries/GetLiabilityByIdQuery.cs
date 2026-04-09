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
    public class GetLiabilityByIdQuery : IRequest<ApiResponse<LiabilityDto>>
    {
        public Guid Id { get; set; }

        public GetLiabilityByIdQuery(Guid id)
        {
            Id = id;
        }
    }

    public class GetLiabilityByIdQueryHandler : BaseQueryHandler<GetLiabilityByIdQuery, ApiResponse<LiabilityDto>>
    {
        public GetLiabilityByIdQueryHandler(ObligationDbContext context, ILogger<GetLiabilityByIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<LiabilityDto>> Handle(GetLiabilityByIdQuery request, CancellationToken cancellationToken)
        {
            var liability = await Context.Liabilities
                .FirstOrDefaultAsync(l => l.Id == request.Id && l.IsActive, cancellationToken);

            if (liability == null)
            {
                Logger.LogWarning("Liability not found: {Id}", request.Id);
                return ApiResponse<LiabilityDto>.ErrorResult("Liability not found");
            }

            var dto = liability.ToDto(Mapper);
            return ApiResponse<LiabilityDto>.SuccessResult(dto);
        }
    }
}

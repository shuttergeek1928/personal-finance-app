using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class DeleteLiabilityCommand : IRequest<ApiResponse<LiabilityDto>>
    {
        public Guid Id { get; set; }

        public DeleteLiabilityCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteLiabilityCommandHandler : BaseRequestHandler<DeleteLiabilityCommand, ApiResponse<LiabilityDto>>
    {
        public DeleteLiabilityCommandHandler(
            ObligationDbContext context,
            ILogger<DeleteLiabilityCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<LiabilityDto>> Handle(DeleteLiabilityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var liability = await Context.Liabilities.FirstOrDefaultAsync(l => l.Id == request.Id && l.IsActive, cancellationToken);

                if (liability == null)
                {
                    Logger.LogWarning("Liability not found: {Id}", request.Id);
                    return ApiResponse<LiabilityDto>.ErrorResult("Liability not found");
                }

                // Soft delete
                liability.IsActive = false;
                await Context.SaveChangesAsync(cancellationToken);

                var dto = liability.ToDto(Mapper);

                Logger.LogInformation("Liability deleted successfully: {Id}", request.Id);

                return ApiResponse<LiabilityDto>.SuccessResult(dto, "Liability deleted successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting liability: {Id}", request.Id);
                return ApiResponse<LiabilityDto>.ErrorResult($"An error occurred while deleting the liability: {ex.Message}");
            }
        }
    }
}

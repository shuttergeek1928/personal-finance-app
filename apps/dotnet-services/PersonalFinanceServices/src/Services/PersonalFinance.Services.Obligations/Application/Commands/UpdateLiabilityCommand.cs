using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class UpdateLiabilityCommand : IRequest<ApiResponse<LiabilityDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public LiabilityType Type { get; set; }
        public string LenderName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DateTime StartDate { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? CreditCardId { get; set; }
        public bool IsNoCostEmi { get; set; }
        public decimal? ProcessingFee { get; set; }

        public UpdateLiabilityCommand(Guid id, string name, LiabilityType type, string lenderName,
            decimal principalAmount, decimal interestRate, int tenureMonths,
            DateTime startDate, Guid? accountId, Guid? creditCardId,
            bool isNoCostEmi, decimal? processingFee)
        {
            Id = id;
            Name = name;
            Type = type;
            LenderName = lenderName;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            TenureMonths = tenureMonths;
            StartDate = startDate;
            AccountId = accountId;
            CreditCardId = creditCardId;
            IsNoCostEmi = isNoCostEmi;
            ProcessingFee = processingFee;
        }
    }

    public class UpdateLiabilityCommandHandler : BaseRequestHandler<UpdateLiabilityCommand, ApiResponse<LiabilityDto>>
    {
        public UpdateLiabilityCommandHandler(
            ObligationDbContext context,
            ILogger<UpdateLiabilityCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<LiabilityDto>> Handle(UpdateLiabilityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var liability = await Context.Liabilities.FirstOrDefaultAsync(l => l.Id == request.Id && l.IsActive, cancellationToken);

                Logger.LogInformation("Updating liability: {Id}. New CreditCardId: {CreditCardId}",
                    request.Id, request.CreditCardId);

                if (liability == null)
                {
                    Logger.LogWarning("Liability not found: {Id}", request.Id);
                    return ApiResponse<LiabilityDto>.ErrorResult("Liability not found");
                }

                liability.Update(
                    request.Name,
                    request.Type,
                    request.LenderName,
                    request.PrincipalAmount,
                    request.InterestRate,
                    request.TenureMonths,
                    request.StartDate,
                    request.AccountId,
                    request.CreditCardId,
                    request.IsNoCostEmi,
                    request.ProcessingFee);

                await Context.SaveChangesAsync(cancellationToken);

                var dto = liability.ToDto(Mapper);

                Logger.LogInformation("Liability updated successfully: {Id}", liability.Id);

                return ApiResponse<LiabilityDto>.SuccessResult(dto, "Liability updated successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating liability: {Id}", request.Id);
                return ApiResponse<LiabilityDto>.ErrorResult($"An error occurred while updating the liability: {ex.Message}");
            }
        }
    }
}

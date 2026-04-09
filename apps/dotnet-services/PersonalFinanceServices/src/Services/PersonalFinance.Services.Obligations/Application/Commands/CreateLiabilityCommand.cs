using AutoMapper;
using MediatR;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class CreateLiabilityCommand : IRequest<ApiResponse<LiabilityDto>>
    {
        public string Name { get; set; } = string.Empty;
        public LiabilityType Type { get; set; }
        public string LenderName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DateTime StartDate { get; set; }
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? CreditCardId { get; set; }
        public bool IsNoCostEmi { get; set; }
        public decimal? ProcessingFee { get; set; }

        public CreateLiabilityCommand(string name, LiabilityType type, string lenderName,
            decimal principalAmount, decimal interestRate, int tenureMonths,
            DateTime startDate, Guid userId, Guid? accountId, Guid? creditCardId,
            bool isNoCostEmi, decimal? processingFee)
        {
            Name = name;
            Type = type;
            LenderName = lenderName;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            TenureMonths = tenureMonths;
            StartDate = startDate;
            UserId = userId;
            AccountId = accountId;
            CreditCardId = creditCardId;
            IsNoCostEmi = isNoCostEmi;
            ProcessingFee = processingFee;
        }
    }

    public class CreateLiabilityCommandHandler : BaseRequestHandler<CreateLiabilityCommand, ApiResponse<LiabilityDto>>
    {
        public CreateLiabilityCommandHandler(
            ObligationDbContext context,
            ILogger<CreateLiabilityCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<LiabilityDto>> Handle(CreateLiabilityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating liability: {Name} for user: {UserId}. CreditCardId: {CreditCardId}", 
                    request.Name, request.UserId, request.CreditCardId);

                var liability = new Liability(
                    request.Name,
                    request.Type,
                    request.LenderName,
                    request.PrincipalAmount,
                    request.InterestRate,
                    request.TenureMonths,
                    request.StartDate,
                    request.UserId,
                    request.AccountId,
                    request.CreditCardId,
                    request.IsNoCostEmi,
                    request.ProcessingFee);

                Context.Liabilities.Add(liability);
                await Context.SaveChangesAsync(cancellationToken);

                var dto = liability.ToDto(Mapper);

                Logger.LogInformation("Liability created successfully: {Id}", liability.Id);

                return ApiResponse<LiabilityDto>.SuccessResult(dto, "Liability created successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating liability: {Name}", request.Name);
                return ApiResponse<LiabilityDto>.ErrorResult($"An error occurred while creating the liability: {ex.Message}");
            }
        }
    }
}

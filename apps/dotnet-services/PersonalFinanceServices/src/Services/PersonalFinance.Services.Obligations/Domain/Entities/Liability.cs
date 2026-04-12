using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Domain.Entities
{
    public class Liability : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public LiabilityType Type { get; private set; }
        public string LenderName { get; private set; } = string.Empty;
        public Money PrincipalAmount { get; private set; }
        public Money OutstandingBalance { get; private set; }
        public decimal InterestRate { get; private set; }
        public int TenureMonths { get; private set; }
        public Money EmiAmount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? AccountId { get; private set; }
        public Guid? CreditCardId { get; private set; }
        public CreditCard? CreditCard { get; private set; }
        public bool IsNoCostEmi { get; private set; }
        public Money? ProcessingFee { get; private set; }

        private Liability() { }

        public Liability(
            string name,
            LiabilityType type,
            string lenderName,
            decimal principalAmount,
            decimal interestRate,
            int tenureMonths,
            DateTime startDate,
            Guid userId,
            Guid? accountId = null,
            Guid? creditCardId = null,
            bool isNoCostEmi = false,
            decimal? processingFee = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            LenderName = lenderName ?? throw new ArgumentNullException(nameof(lenderName));
            PrincipalAmount = new Money(principalAmount);
            InterestRate = interestRate;
            TenureMonths = tenureMonths;
            StartDate = startDate;
            UserId = userId;
            AccountId = accountId;
            CreditCardId = creditCardId;
            IsNoCostEmi = isNoCostEmi;

            if (processingFee.HasValue)
                ProcessingFee = new Money(processingFee.Value);

            // Calculate EMI using reducing balance formula
            EmiAmount = new Money(CalculateEmi(principalAmount, interestRate, tenureMonths));
            EndDate = startDate.AddMonths(tenureMonths);

            // If start date is in the past, compute outstanding balance after elapsed EMIs
            OutstandingBalance = new Money(CalculateOutstandingAfterElapsedMonths(
                principalAmount, interestRate, tenureMonths, startDate));
        }

        /// <summary>
        /// Calculates EMI using the reducing balance formula:
        /// EMI = P × r × (1+r)^n / ((1+r)^n - 1)
        /// where P = principal, r = monthly rate, n = tenure months
        /// </summary>
        public static decimal CalculateEmi(decimal principal, decimal annualRate, int tenureMonths)
        {
            if (annualRate == 0)
                return Math.Round(principal / tenureMonths, 2);

            var monthlyRate = annualRate / 12 / 100;
            var power = (decimal)Math.Pow((double)(1 + monthlyRate), tenureMonths);
            var emi = principal * monthlyRate * power / (power - 1);
            return Math.Round(emi, 2);
        }

        /// <summary>
        /// Calculates the outstanding balance after elapsed months from startDate to now.
        /// Uses reducing balance amortization: each month, interest = outstanding × monthly rate,
        /// principal component = EMI - interest, outstanding -= principal component.
        /// If startDate is in the future or today, returns the full principal.
        /// </summary>
        public static decimal CalculateOutstandingAfterElapsedMonths(
            decimal principal, decimal annualRate, int tenureMonths, DateTime startDate)
        {
            // Calculate elapsed months
            var now = DateTime.UtcNow;
            var elapsed = ((now.Year - startDate.Year) * 12) + (now.Month - startDate.Month);

            // If start date is today or in the future, no EMIs paid yet
            if (elapsed <= 0)
                return principal;

            // Cap at tenure (can't have paid more months than the tenure)
            elapsed = Math.Min(elapsed, tenureMonths);

            var emi = CalculateEmi(principal, annualRate, tenureMonths);
            var monthlyRate = annualRate / 12 / 100;
            var outstanding = principal;

            for (int i = 0; i < elapsed; i++)
            {
                var interestComponent = annualRate == 0 ? 0 : Math.Round(outstanding * monthlyRate, 2);
                var principalComponent = emi - interestComponent;
                outstanding -= principalComponent;

                if (outstanding <= 0)
                {
                    outstanding = 0;
                    break;
                }
            }

            return Math.Round(Math.Max(outstanding, 0), 2);
        }

        /// <summary>
        /// Records a payment against the outstanding balance.
        /// </summary>
        public void MakePayment(Money amount)
        {
            if (amount.Amount <= 0)
                throw new InvalidOperationException("Payment amount must be positive.");

            if (amount.Amount > OutstandingBalance.Amount)
                throw new InvalidOperationException("Payment amount exceeds outstanding balance.");

            OutstandingBalance = OutstandingBalance.Subtract(amount);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Recalculates EMI and end date based on current outstanding balance, interest rate, and remaining tenure.
        /// </summary>
        public void Recalculate()
        {
            var monthsElapsed = (int)((DateTime.UtcNow - StartDate).TotalDays / 30.44);
            var remainingMonths = Math.Max(TenureMonths - monthsElapsed, 1);

            EmiAmount = new Money(CalculateEmi(OutstandingBalance.Amount, InterestRate, remainingMonths));
            EndDate = DateTime.UtcNow.AddMonths(remainingMonths);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            LiabilityType type,
            string lenderName,
            decimal principalAmount,
            decimal interestRate,
            int tenureMonths,
            DateTime startDate,
            Guid? accountId,
            Guid? creditCardId = null,
            bool isNoCostEmi = false,
            decimal? processingFee = null)
        {
            Name = name;
            Type = type;
            LenderName = lenderName;
            InterestRate = interestRate;
            TenureMonths = tenureMonths;
            StartDate = startDate;
            AccountId = accountId;
            CreditCardId = creditCardId;
            IsNoCostEmi = isNoCostEmi;

            if (processingFee.HasValue)
                ProcessingFee = new Money(processingFee.Value);
            else
                ProcessingFee = null;

            PrincipalAmount = new Money(principalAmount);
            EmiAmount = new Money(CalculateEmi(principalAmount, interestRate, tenureMonths));
            EndDate = startDate.AddMonths(tenureMonths);

            // Recalculate outstanding based on elapsed months (handles past start dates)
            OutstandingBalance = new Money(CalculateOutstandingAfterElapsedMonths(
                principalAmount, interestRate, tenureMonths, startDate));

            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum LiabilityType
    {
        HomeLoan,
        PersonalLoan,
        CarLoan,
        EducationLoan,
        CreditCardEmi,
        Other
    }
}

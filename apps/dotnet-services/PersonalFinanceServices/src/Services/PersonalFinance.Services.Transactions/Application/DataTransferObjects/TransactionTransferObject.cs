using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Transactions.Application.DTOs
{
    public class TransactionTransferObject
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public Money Money { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}

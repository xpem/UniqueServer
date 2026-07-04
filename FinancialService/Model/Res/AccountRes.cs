using FinancialService.Model.DTO;

namespace FinancialService.Model.Res
{
    public record AccountRes
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public AccountType Type { get; set; }

        public decimal CurrentBalance { get; set; }

        public bool IncludeInGeneralBalance { get; set; }

        public bool Inactive { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Guid? AccountId { get; set; }
    }
}

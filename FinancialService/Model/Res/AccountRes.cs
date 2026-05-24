namespace FinancialService.Model.Res
{
    public record AccountRes
    {
        public int Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; }
    }
}

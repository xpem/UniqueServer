namespace FinancialService.Model.Req
{
    public record AccountReq
    {
        public int? Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; }
    }
}

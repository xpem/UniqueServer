namespace FinancialService.Model.Req
{
    public record AdjustAccountBalanceReq : BaseReq
    {
        public int? Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; }

        public required TransactionReq Transaction { get; set; }
    }
}

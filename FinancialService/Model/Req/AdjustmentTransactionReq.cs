namespace FinancialService.Model.Req
{
    public record AdjustmentTransactionReq
    {
        public AccountReq AccountReq { get; set; }
    }
}

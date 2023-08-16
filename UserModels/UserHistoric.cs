namespace UserModels
{
    public class UserHistoric : BaseModels.BaseModel
    {
        public required int HistoricTypeId { get; set; }

        public required int UserId { get; set; }

        public required User User { get; set; }
    }
}

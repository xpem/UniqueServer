namespace UserModels
{
    public class UserHistoric : BaseModels.BaseModel
    {
        public required int UserHistoricTypeId { get; set; }

        public UserHistoricType? UserHistoricType { get; set; }

        public required int UserId { get; set; }

        public User? User { get; set; }
    }
}

namespace InventoryModels.Res.Bases
{
    public record ResTextTypeBase
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool SystemDefault { get; set; }

        public int Sequence { get; set; }
    }
}

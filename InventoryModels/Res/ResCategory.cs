namespace InventoryModels.Res
{
    public record ResCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public bool SystemDefault { get; set; }
    }
}

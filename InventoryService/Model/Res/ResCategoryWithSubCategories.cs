namespace InventoryModels.Res
{
    public record ResCategoryWithSubCategories
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public bool SystemDefault { get; set; }
        public List<ResSubCategory>? SubCategories { get; set; }
    }
}

﻿namespace InventoryModels.Res.Item
{
    public record ResItem
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public string? TechnicalDescription { get; init; }

        public DateOnly AcquisitionDate { get; init; }

        public decimal? PurchaseValue { get; init; }

        public string? PurchaseStore { get; init; }

        public decimal? ResaleValue { get; init; }

        public ResItemItemSituation? Situation { get; init; }

        public ResItemCategory? Category { get; init; }

        public string? Comment { get; init; }

        public string? Image1 { get; init; }

        public string? Image2 { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }

        public ResItemAcquisitionType? AcquisitionType { get; init; }

        public DateOnly? WithdrawalDate { get; init; }
    }

    public record ResItemCategory
    {
        public int? Id { get; init; }
        public string? Name { get; init; }

        public string? Color { get; init; }

        public ResItemSubCategory? SubCategory { get; init; }
    }

    public record ResItemSubCategory
    {
        public int? Id { get; init; }
        public string? Name { get; init; }
        public string? IconName { get; init; }
    }

    public record ResItemAcquisitionType
    {
        public int? Id { get; init; }
        public string? Name { get; init; }
    }


    public record ResItemItemSituation
    {
        public int? Id { get; init; }
        public string? Name { get; init; }
    }
}

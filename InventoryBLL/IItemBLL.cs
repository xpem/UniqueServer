﻿using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL
{
    public interface IItemBLL
    {
        BLLResponse CreateItem(ReqItem reqItem, int uid);
        string? ValidateIndexes(ReqItem reqItem, int uid);
    }
}
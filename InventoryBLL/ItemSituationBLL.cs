using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Res.Item;

namespace InventoryBLL
{
    public class ItemSituationBLL(IItemSituationDAL itemSituationDAL) : IItemSituationBLL
    {
        public BLLResponse Get(int uid)
        {
            List<ItemSituation>? itemSituations = itemSituationDAL.Get(uid);
            List<ResItemSituation> resItemSituations = [];

            if (itemSituations != null && itemSituations.Count > 0)
                foreach (ItemSituation itemSituation in itemSituations)
                    resItemSituations.Add(
                        new()
                        {
                            Id = itemSituation.Id,
                            Name = itemSituation.Name,
                            Sequence = itemSituation.Sequence,
                            SystemDefault = itemSituation.SystemDefault,
                            Type = itemSituation.Type,
                        });

            return new BLLResponse(resItemSituations);
        }
    }
}

using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;

namespace InventoryBLL
{
    public class ItemSituationBLL(IItemSituationDAL itemSituationDAL) : IItemSituationBLL
    {
        public BaseResponse Get(int uid)
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

            return new BaseResponse(resItemSituations);
        }
    }
}

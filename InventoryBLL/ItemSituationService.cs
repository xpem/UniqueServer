using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;

namespace InventoryBLL
{
    public class ItemSituationService(IItemSituationRepo itemSituationRepo) : IItemSituationService
    {
        public async Task<BaseResponse> Get(int uid)
        {
            List<ItemSituation>? itemSituations = await itemSituationRepo.Get(uid);

            return new BaseResponse(BuildItemSituation(itemSituations));
        }

        public static List<ResItemSituation> BuildItemSituation(List<ItemSituation>? itemSituations)
        {
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

            return resItemSituations;
        }
    }
}

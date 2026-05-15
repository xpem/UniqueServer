using BaseModels;
using BookshelfModels.Request;

namespace BookshelfServices
{
    public interface IBookService
    {
        Task<BaseResp> CreateAsync(ReqBook reqBook, int uid);

        Task<BaseResp> UpdateAsync(ReqBook reqBook, int bookId, int uid);

        Task<BaseResp> InactivateAsync(int bookId, int uid);

        Task<BaseResp> GetByUpdatedAtAsync(DateTime updatedAt, int page, int uid);

        Task<int> DeleteAllAsync(int uid);
    }
}
using BaseModels;
using BookshelfModels.Request;

namespace BookshelfServices
{
    public interface IBookService
    {
        Task<BaseResponse> CreateAsync(ReqBook reqBook, int uid);

        Task<BaseResponse> UpdateAsync(ReqBook reqBook, int bookId, int uid);

        Task<BaseResponse> InactivateAsync(int bookId, int uid);

        Task<BaseResponse> GetByUpdatedAtAsync(DateTime updatedAt, int page, int uid);
    }
}
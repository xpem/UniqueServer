using BaseModels;
using BookshelfModels.Request;

namespace BookshelfBLL
{
    public interface IBookBLL
    {
        Task<BLLResponse> CreateBook(ReqBook reqBook, int uid);

        Task<BLLResponse> UpdateBook(ReqBook reqBook, int bookId, int uid);

        BLLResponse GetByUpdatedAt(DateTime updatedAt, int uid);

        Task<BLLResponse> UpdateBookStatusAsync(ReqBookStatus reqBookStatus, int bookId, int uid);

        Task<BLLResponse> InactivateBook(int bookId, int uid);
    }
}
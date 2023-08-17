using BaseModels;
using BookshelfModels.Request;

namespace BookshelfBLL
{
    public interface IBookBLL
    {
        Task<BLLResponse> CreateBook(ReqBook reqBook, int uid);
    }
}
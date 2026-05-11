using BaseModels;
using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfRepo
{
    public class BookshelfDbCtx(DbContextOptions<BookshelfDbCtx> options) : DbContext(options)
    {
        public virtual DbSet<Book> Book => Set<Book>();

        public virtual DbSet<BookHistoric> BookHistoric => Set<BookHistoric>();

        public virtual DbSet<BookHistoricType> BookHistoricType => Set<BookHistoricType>();

        public virtual DbSet<BookHistoricItemField> BookHistoricItemField => Set<BookHistoricItemField>();

        public virtual DbSet<BookHistoricItem> BookHistoricItem => Set<BookHistoricItem>();
    }
}
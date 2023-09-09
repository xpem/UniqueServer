using BaseModels;
using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfDbContextDAL
{
    public class BookshelfDbContext : DbContext
    {
        public virtual DbSet<Book> Book { get; set; }

        public virtual DbSet<BookHistoric> BookHistoric { get; set; }

        public virtual DbSet<BookHistoricType> BookHistoricType { get; set; }

        public virtual DbSet<BookHistoricItemField> BookHistoricItemField { get; set; }

        public virtual DbSet<BookHistoricItem> BookHistoricItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.BookShelfConn, ServerVersion.AutoDetect(PrivateKeys.BookShelfConn));
        }
    }
}
using BaseModels;
using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfDbContextDAL
{
    public class BookshelfDbContext : DbContext
    {
        public DbSet<Book> Book { get; set; }

        public DbSet<BookHistoric> BookHistoric { get; set; }

        public DbSet<BookHistoricType> BookHistoricType { get; set; }

        public DbSet<BookHistoricItemField> BookHistoricItemField { get; set; }

        public DbSet<BookHistoricItem> BookHistoricItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.BookShelfConn, ServerVersion.AutoDetect(PrivateKeys.BookShelfConn));
        }
    }
}
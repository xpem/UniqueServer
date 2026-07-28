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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseIdentityByDefaultColumns();
        }
    }

    //migrations
    //no console do gerenciador de pacotes selecione o dal referente:
    //EntityFrameworkCore\Add-Migration "202607071" -Context BookshelfDbCtx
    //EntityFrameworkCore\update-database -Context BookshelfDbCtx

    //to remove last migration snapshot
    //EntityFrameworkCore\Remove-Migration -Context BookshelfDbCtx 

}
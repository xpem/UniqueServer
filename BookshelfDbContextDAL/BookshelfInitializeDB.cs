using Microsoft.EntityFrameworkCore;

namespace BookshelfRepo
{
    public class BookshelfInitializeDB
    {
        readonly IDbContextFactory<BookshelfDbCtx> dbCtx;

        public BookshelfInitializeDB(IDbContextFactory<BookshelfDbCtx> dbCtx) { this.dbCtx = dbCtx; }

        public void CreateInitialValues()
        {
            using var context = dbCtx.CreateDbContext();
            context.Database.EnsureCreated();
            if (context.BookHistoricType.Count() is not 0) { return; }


            BookshelfModels.BookHistoricType[] bookHistoricType = new BookshelfModels.BookHistoricType[]
            {
                new(){Name = "Insert"},
                new(){Name = "Update"},
                new(){Name = "Delete"},
                new(){Name = "Inactivate"},
            };

            //foreach (var _bookHistoricType in bookHistoricType)
            //{
            context.BookHistoricType?.AddRange(bookHistoricType);
            //}

            if (context.BookHistoricItemField.Count() is not 0) { return; }

            BookshelfModels.BookHistoricItemField[] bookHistoricItemField = new BookshelfModels.BookHistoricItemField[]
            {
                new(){Name = "SubTítulo" },
                new(){Name = "Título" },
                new(){Name = "Capa" },
                new(){Name = "Autores" },
                new(){Name = "Volume"},
                new(){Name = "Páginas"},
                new(){Name = "Ano"},
                new(){Name = "Status"},
                new(){Name = "Avaliação"},
                new(){Name = "Gênero"},
                new(){Name = "Isbn"},
                new(){Name = "Inativo"}
            };

            //foreach (var _bookHistoricItemField in bookHistoricItemField)
            //{
            context.BookHistoricItemField?.AddRangeAsync(bookHistoricItemField);
            //}

            context.SaveChanges();
        }
    }
}

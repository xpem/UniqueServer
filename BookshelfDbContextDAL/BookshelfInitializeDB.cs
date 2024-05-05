namespace BookshelfDbContextDAL
{
    public class BookshelfInitializeDB
    {
        readonly BookshelfDbContext BookshelfDbContext;

        public BookshelfInitializeDB(BookshelfDbContext bookshelfDbContext) { BookshelfDbContext = bookshelfDbContext; }

        public void CreateInitialValues()
        {
            BookshelfDbContext.Database.EnsureCreated();

            if (BookshelfDbContext.BookHistoricType.Count() is not 0) { return; }


            BookshelfModels.BookHistoricType[] bookHistoricType = new BookshelfModels.BookHistoricType[]
            {
                new(){Name = "Insert"},
                new(){Name = "Update"},
                new(){Name = "Delete"},
                new(){Name = "Inactivate"},
            };

            //foreach (var _bookHistoricType in bookHistoricType)
            //{
            BookshelfDbContext.BookHistoricType?.AddRange(bookHistoricType);
            //}

            if (BookshelfDbContext.BookHistoricItemField.Count() is not 0) { return; }

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
            BookshelfDbContext.BookHistoricItemField?.AddRangeAsync(bookHistoricItemField);
            //}

            BookshelfDbContext.SaveChanges();
        }
    }
}

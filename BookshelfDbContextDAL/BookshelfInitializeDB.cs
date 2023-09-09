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
                new BookshelfModels.BookHistoricType(){Name = "Insert"},
                new BookshelfModels.BookHistoricType(){Name = "Update"},
                new BookshelfModels.BookHistoricType(){Name = "Delete"},
                new BookshelfModels.BookHistoricType(){Name = "Inactivate"},
            };

            //foreach (var _bookHistoricType in bookHistoricType)
            //{
            BookshelfDbContext.BookHistoricType?.AddRange(bookHistoricType);
            //}

            if (BookshelfDbContext.BookHistoricItemField.Count() is not 0) { return; }

            BookshelfModels.BookHistoricItemField[] bookHistoricItemField = new BookshelfModels.BookHistoricItemField[]
            {
                new BookshelfModels.BookHistoricItemField(){Name = "SubTítulo" },
                new BookshelfModels.BookHistoricItemField(){Name = "Título" },
                new BookshelfModels.BookHistoricItemField(){Name = "Capa" },
                new BookshelfModels.BookHistoricItemField(){Name = "Autores" },
                new BookshelfModels.BookHistoricItemField(){Name = "Volume"},
                new BookshelfModels.BookHistoricItemField(){Name = "Páginas"},
                new BookshelfModels.BookHistoricItemField(){Name = "Ano"},
                new BookshelfModels.BookHistoricItemField(){Name = "Status"},
                new BookshelfModels.BookHistoricItemField(){Name = "Avaliação"},
                new BookshelfModels.BookHistoricItemField(){Name = "Gênero"},
                new BookshelfModels.BookHistoricItemField(){Name = "Isbn"},
                new BookshelfModels.BookHistoricItemField(){Name = "Inativo"}
            };

            //foreach (var _bookHistoricItemField in bookHistoricItemField)
            //{
            BookshelfDbContext.BookHistoricItemField?.AddRangeAsync(bookHistoricItemField);
            //}

            BookshelfDbContext.SaveChanges();
        }
    }
}

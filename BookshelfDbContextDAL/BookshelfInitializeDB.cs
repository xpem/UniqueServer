namespace BookshelfDbContextDAL
{
    public class BookshelfInitializeDB(BookshelfDbContext bookshelfDbContext)
    {
        public void CreateInitialValues()
        {
            bookshelfDbContext.Database.EnsureCreated();

            if (bookshelfDbContext.BookHistoricType.Count() is not 0) { return; }


            BookshelfModels.BookHistoricType[] bookHistoricType =
            [
                new(){Name = "Insert"},
                new(){Name = "Update"},
                new(){Name = "Delete"},
                new(){Name = "Inactivate"},
            ];

            //foreach (var _bookHistoricType in bookHistoricType)
            //{
            bookshelfDbContext.BookHistoricType?.AddRange(bookHistoricType);
            //}

            if (bookshelfDbContext.BookHistoricItemField.Count() is not 0) { return; }

            BookshelfModels.BookHistoricItemField[] bookHistoricItemField =
            [
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
            ];

            //foreach (var _bookHistoricItemField in bookHistoricItemField)
            //{
            bookshelfDbContext.BookHistoricItemField?.AddRangeAsync(bookHistoricItemField);
            //}

            bookshelfDbContext.SaveChanges();
        }
    }
}

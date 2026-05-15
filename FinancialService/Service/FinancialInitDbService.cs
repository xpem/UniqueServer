using FinancialService.Repo;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Service
{
    public class FinancialInitDbService(IDbContextFactory<FinancialDbctx> dbCtx)
    {
        public void CreateCategoryInitialValues()
        {
            //using var context = dbCtx.CreateDbContext();
            //context.Database.EnsureCreated();


            //criação das categorias padrões principais do sistema
            //            TransactionCategoryDTO[] transactionCategories = new TransactionCategoryDTO[]
            //            {
            //                new(){Name = "Sem categoria", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Alimentação", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Carro", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Casa", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Educação", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Doações", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Eletrônicos", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Presentes", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Pessoais", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Impostos", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Lazer", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Outros", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Receita", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Saúde", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Seguro", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Transporte", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //                new(){Name = "Investimentos", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = true},
            //            };

            //            context.TransactionCategory?.AddRange(transactionCategories);
            //            context.SaveChanges();

            //            var categories = context.TransactionCategory.First(x => x.Name == "Alimentação");

            //            transactionCategories = new TransactionCategoryDTO[]
            //  {
            //      new(){Name = "Almoço", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                new(){Name = "Lanche", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //            };
            //            context.TransactionCategory?.AddRange(transactionCategories);
            //            context.SaveChanges();

            //            categories = context.TransactionCategory.First(x => x.Name == "Carro");

            //            transactionCategories = new TransactionCategoryDTO[]
            //            {
            //                    new(){Name = "Combustível", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                    new(){Name = "Estacionamento", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                    new(){Name = "Lavagem", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                    new(){Name = "Multas", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                    new(){Name = "Pedágio", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                    };

            //            context.TransactionCategory?.AddRange(transactionCategories);
            //            context.SaveChanges();


            //            categories = context.TransactionCategory.First(x => x.Name == "Casa");

            //            transactionCategories = new TransactionCategoryDTO[] {
            //                new(){Name = "Aluguel", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //                new(){Name = "Condomínio", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Internet", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Energia", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Manutenção", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Limpeza", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Móveis", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //new(){Name = "Utensílios", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //            };

            //            context.TransactionCategory?.AddRange(transactionCategories);
            //            context.SaveChanges();

            //var categories = context.TransactionCategory.First(x => x.Name == "Educação");

            //var transactionCategories = new TransactionCategoryDTO[] {
            //    new(){Name = "Cursos", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Livros", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Pós-Graduação", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Faculdade", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //};

            //context.TransactionCategory?.AddRange(transactionCategories);
            //context.SaveChanges();


            // categories = context.TransactionCategory.First(x => x.Name == "Pessoais");

            // transactionCategories = new TransactionCategoryDTO[] {
            //    new(){Name = "Academia", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Assessório", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Celular", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Cosmético", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Roupa", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Calçado", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //};

            //context.TransactionCategory?.AddRange(transactionCategories);
            //context.SaveChanges();

            //categories = context.TransactionCategory.First(x => x.Name == "Impostos");

            //transactionCategories = new TransactionCategoryDTO[] {
            //    new(){Name = "IR", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "IPTU", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "IPVA", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "FGTS", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //};

            //context.TransactionCategory?.AddRange(transactionCategories);
            //context.SaveChanges();


            //categories = context.TransactionCategory.First(x => x.Name == "Lazer");

            //transactionCategories = new TransactionCategoryDTO[] {
            //    new(){Name = "Streaming", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Bar", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Cinema", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Show", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Jogo", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //    new(){Name = "Viagem", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
            //};

            //context.TransactionCategory?.AddRange(transactionCategories);
            //context.SaveChanges();

          // var categories = context.TransactionCategory.First(x => x.Name == "Receita");

          //var  transactionCategories = new TransactionCategoryDTO[] {
          //      new(){Name = "13°", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Bonificação", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Comissão", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Estorno", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Férias", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Juros", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Reembolso", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Salário", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Outros", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //  };

          //  context.TransactionCategory?.AddRange(transactionCategories);
          //  context.SaveChanges();

          //  categories = context.TransactionCategory.First(x => x.Name == "Saúde");

          //  transactionCategories = new TransactionCategoryDTO[] {
          //      new(){Name = "Plano de Saúde", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Dentista", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Enxame", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Farmácia", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Médico", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //  };

          //  context.TransactionCategory?.AddRange(transactionCategories);
          //  context.SaveChanges();

          //  categories = context.TransactionCategory.First(x => x.Name == "Seguro");

          //  transactionCategories = new TransactionCategoryDTO[] {
          //      new(){Name = "Carro", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Moto", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Vida", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Residencial", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //  };

          //  context.TransactionCategory?.AddRange(transactionCategories);
          //  context.SaveChanges();

          //  categories = context.TransactionCategory.First(x => x.Name == "Transporte");

          //  transactionCategories = new TransactionCategoryDTO[] {
          //      new(){Name = "Metrô", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Ônibus", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Bicicleta", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "App", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //  };

          //  context.TransactionCategory?.AddRange(transactionCategories);
          //  context.SaveChanges();


          //  categories = context.TransactionCategory.First(x => x.Name == "Investimentos");

          //  transactionCategories = new TransactionCategoryDTO[] {
          //      new(){Name = "Ações", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Fundos Imobiliários", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Renda Fixa", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //      new(){Name = "Criptomoedas", CreatedAt = DateTime.Now, SystemDefault = true, IsMainTransactionCategory = false, ParentTransactionCategoryId = categories.Id},
          //  };

          //  context.TransactionCategory?.AddRange(transactionCategories);
          //  context.SaveChanges();
        }
    }
}

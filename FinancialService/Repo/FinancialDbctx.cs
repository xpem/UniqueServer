using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public class FinancialDbctx(DbContextOptions<FinancialDbctx> options) : DbContext(options)
    {
        public virtual DbSet<TransactionCategoryDTO> TransactionCategory => Set<TransactionCategoryDTO>();

        public virtual DbSet<TransactionDTO> Transaction => Set<TransactionDTO>();

        public virtual DbSet<AccountDTO> Account => Set<AccountDTO>();

        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "202605251" -Context FinancialDbctx
        //EntityFrameworkCore\update-database -Context FinancialDbctx

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context FinancialDbctx 
    }
}

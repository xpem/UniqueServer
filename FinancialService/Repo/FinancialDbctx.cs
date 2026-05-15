using FinancialService.Model.Dto;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public class FinancialDbctx(DbContextOptions<FinancialDbctx> options) : DbContext(options)
    {
        public virtual DbSet<TransactionCategoryDTO> TransactionCategory => Set<TransactionCategoryDTO>();

        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "202605132" -Context FinancialDbctx
        //EntityFrameworkCore\update-database -Context FinancialDbctx

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context FinancialDbctx 
    }
}

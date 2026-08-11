# Migration: Adicionar Sistema de Doença

## Comandos para criar a migration

Execute no **Package Manager Console** do Visual Studio:

```powershell
# Certifique-se de estar no diretório correto
# Default project: MobService

# Criar a migration
EntityFrameworkCore\Add-Migration "AddPetSickness" -Context MobDbCtx

# Aplicar ao banco de dados
EntityFrameworkCore\update-database -Context MobDbCtx
```

## SQL Esperado (para referência)

A migration deve adicionar as seguintes colunas na tabela `Pet`:

```sql
ALTER TABLE "Pet" ADD COLUMN "sickness" integer NOT NULL DEFAULT 0;
ALTER TABLE "Pet" ADD COLUMN "sickness_timer" bigint NOT NULL DEFAULT 0;
```

## Verificação

Após aplicar a migration, verifique no banco de dados PostgreSQL:

```sql
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'Pet' AND column_name IN ('sickness', 'sickness_timer');
```

Resultado esperado:
```
column_name     | data_type | is_nullable | column_default
----------------|-----------|-------------|----------------
sickness        | integer   | NO          | 0
sickness_timer  | bigint    | NO          | 0
```

## Rollback (se necessário)

Se precisar desfazer a migration:

```powershell
EntityFrameworkCore\Update-Database -Migration <PreviousMigrationName> -Context MobDbCtx
EntityFrameworkCore\Remove-Migration -Context MobDbCtx
```

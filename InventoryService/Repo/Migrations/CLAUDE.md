# Regras para Migrations

Antes de criar qualquer migration, declare a mudança no modelo primeiro:

- **Índices** → `HasIndex` no `OnModelCreating` do `InventoryDbCtx.cs`
- **Colunas** → propriedade na entidade correspondente
- **Constraints** → fluent API no `OnModelCreating`

Depois gere a migration com o EF (nunca crie o arquivo manualmente):

```
dotnet ef migrations add <Nome> --project InventoryService --startup-project UniqueServer
dotnet ef database update --project InventoryService --startup-project UniqueServer
```

Se o modelo e o snapshot divergirem do banco, a próxima migration vai reverter as mudanças silenciosamente.

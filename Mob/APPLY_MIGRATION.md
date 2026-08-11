# 🏠 Como Aplicar a Migration da Casinha

## Opção 1: Package Manager Console (Visual Studio)

```powershell
# 1. Abrir Package Manager Console (Tools > NuGet Package Manager > Package Manager Console)

# 2. Selecionar o projeto "Mob" no dropdown "Default project"

# 3. Executar:
Update-Database -Context MobDbCtx
```

## Opção 2: .NET CLI

```bash
# Na pasta Mob/
cd d:\Emanuel\Projetos\UniqueServer\UniqueServer\Mob

# Aplicar migration
dotnet ef database update --context MobDbCtx
```

## Opção 3: .NET CLI (com projeto de startup)

```bash
# Na pasta raiz do UniqueServer/
cd d:\Emanuel\Projetos\UniqueServer\UniqueServer

# Aplicar migration especificando o projeto
dotnet ef database update --context MobDbCtx --project Mob --startup-project UniqueServer
```

---

## ✅ Verificar Migration Aplicada

### SQL Query
```sql
-- Verificar se a coluna foi adicionada
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'Pet' 
  AND column_name = 'HouseCharges';

-- Verificar valores existentes
SELECT "Id", "Name", "HouseCharges" 
FROM "Pet" 
LIMIT 10;
```

### Entity Framework
```bash
# Listar migrations aplicadas
dotnet ef migrations list --context MobDbCtx --project Mob
```

---

## 🔧 Troubleshooting

### Erro: "No DbContext named 'MobDbCtx' was found"
**Solução:** Especificar o projeto correto
```bash
dotnet ef database update --context MobDbCtx --project Mob --startup-project UniqueServer
```

### Erro: "Connection string not found"
**Solução:** Verificar `appsettings.json` ou `appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "MobDb": "Host=localhost;Database=mob;Username=...;Password=..."
  }
}
```

### Erro: "Build failed"
**Solução:** Compilar o projeto primeiro
```bash
dotnet build
dotnet ef database update --context MobDbCtx
```

---

## 📊 Resultado Esperado

```
Build started...
Build succeeded.
Applying migration '20260811140000_AddPetHouseCharges'.
Done.
```

### Estrutura da Tabela Pet (Após Migration)
```
Pet
├─ Id (integer, PK)
├─ UserId (integer)
├─ Name (varchar(50), nullable)
├─ VariantIndex (integer)
├─ Phase (integer)
├─ PhaseStart (bigint)
├─ BornAt (bigint)
├─ Hunger (integer)
├─ Energy (integer)
├─ Health (integer)
├─ Joy (integer)
├─ Sickness (integer)
├─ SicknessTimer (bigint)
├─ PoopCount (integer)
├─ BowlPortions (integer)
├─ Toys (integer)
├─ HouseCharges (integer) ← NOVO!
├─ IsDead (boolean)
├─ DeathCause (varchar(20), nullable)
├─ CreatedAt (timestamp)
└─ UpdatedAt (timestamp, nullable)
```

---

## 🧹 Rollback (Se Necessário)

```bash
# Reverter para a migration anterior
dotnet ef database update AddPetSickness --context MobDbCtx

# Ou remover a migration (se ainda não foi aplicada)
dotnet ef migrations remove --context MobDbCtx
```

---

## ✅ Checklist

- [ ] Migration aplicada com sucesso
- [ ] Coluna `HouseCharges` existe na tabela `Pet`
- [ ] Pets existentes têm `HouseCharges = 0`
- [ ] API aceita valores 0-2 para `HouseCharges`
- [ ] Frontend consegue salvar e carregar `HouseCharges`
- [ ] Teste manual: criar pet com casinha carregada
- [ ] Teste manual: carregar pet e verificar cargas da casinha

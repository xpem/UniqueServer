# Resumo das Mudanças - Sistema de Doença e Autonomia

## ✅ Arquivos Modificados

### 1. **Pet.cs** (Modelo)
- ✅ Adicionado campo `Sickness` (int, 0-5)
- ✅ Adicionado campo `SicknessTimer` (long, em ms)

### 2. **ReqSavePet.cs** (DTO Request)
- ✅ Adicionado campo `Sickness` com validação [Range(0, 5)]
- ✅ Adicionado campo `SicknessTimer`

### 3. **PetService.cs** (Service Layer)
- ✅ Atualizado método `SavePetAsync` para persistir `Sickness` e `SicknessTimer`

### 4. **MobController.cs** (API Controller)
- ✅ Atualizado mapeamento no método `SavePet` para incluir novos campos

### 5. **ui.js** (Frontend)
- ✅ Função `cloudSave()`: Envia `sickness` e `sicknessTimer` para API
- ✅ Função `loadPetFromCloud()`: Carrega `sickness` e `sicknessTimer` da API

## 📋 Próximos Passos

### 1. Criar e Aplicar Migration

No **Package Manager Console** do Visual Studio:

```powershell
# Selecionar projeto: MobService
EntityFrameworkCore\Add-Migration "AddPetSickness" -Context MobDbCtx
EntityFrameworkCore\update-database -Context MobDbCtx
```

### 2. Compilar e Testar

```powershell
# Build do projeto
dotnet build

# Verificar se não há erros de compilação
```

### 3. Testes Recomendados

#### Teste 1: Salvar Pet com Doença
```json
POST /Mob/Pet
{
  "name": "TestPet",
  "variantIndex": 0,
  "phase": 1,
  "phaseStart": 1733950800000,
  "bornAt": 1733950800000,
  "hunger": 3,
  "energy": 4,
  "health": 5,
  "joy": 0,
  "sickness": 2,
  "sicknessTimer": 3600000,
  "poopCount": 1,
  "bowlPortions": 2,
  "toys": 1,
  "isDead": false,
  "deathCause": null
}
```

#### Teste 2: Carregar Pet da Nuvem
```
GET /Mob/Pet
Authorization: Bearer {token}
```

Verificar que o response inclui:
```json
{
  "success": true,
  "content": {
    "id": 1,
    "userId": 10,
    "name": "TestPet",
    ...
    "sickness": 2,
    "sicknessTimer": 3600000,
    ...
  }
}
```

#### Teste 3: Simulação Offline
1. Salvar pet com `joy: 0` e `sickness: 0`
2. Esperar 2+ horas
3. Recarregar página
4. Verificar se `sickness` aumentou para 1

## 🎮 Comportamento do Sistema de Doença

### Acúmulo de Doença
- **Joy = 0 por 2h** → +1 nível de doença
- **2+ cocôs por 4h** → +1 nível de doença
- **Máximo**: 5 níveis

### Consequências
- **Nível 0-3**: Apenas visual (botão 💊)
- **Nível 4-5**: Diminui health automaticamente

### Tratamento
- **Remédio**: Cura 2 níveis por dose
- **Efeito colateral**: -1 joy
- **Prioridade**: Doença antes de health baixo

## 📊 Nova Autonomia

### Com Preparação (bowl + toy)
- **Bebê**: ~10-12h
- **Jovem**: ~9-11h
- **Adulto**: ~8-10h

### Sem Preparação
- **Todos**: ~3-4h até morte por negligência

## 🔍 Verificação no Banco

```sql
-- Verificar pets salvos com doença
SELECT id, user_id, name, sickness, sickness_timer, joy, poop_count
FROM "Pet"
WHERE is_dead = false
ORDER BY created_at DESC
LIMIT 10;

-- Verificar estrutura da tabela
\d "Pet"
```

## ⚠️ Notas Importantes

1. **Retrocompatibilidade**: Pets antigos sem os campos `sickness` e `sicknessTimer` terão valores padrão 0
2. **Frontend**: Já está preparado para enviar e receber os novos campos
3. **Backend**: Todos os layers foram atualizados (Model, DTO, Service, Controller)
4. **Migration**: É o único passo pendente - executar comandos acima

## 🐛 Troubleshooting

### Erro: "Column 'sickness' does not exist"
- Executar a migration: `update-database -Context MobDbCtx`

### Erro: "Object reference not set to an instance"
- Verificar se `stats.sickness` está inicializado no frontend
- Verificar se `cloudPet.sickness` tem fallback `|| 0`

### Pet não carrega após atualização
- Limpar cache do navegador (Ctrl+Shift+R)
- Verificar console do navegador para erros
- Verificar logs do servidor

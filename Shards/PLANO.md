# Ancestral Shards — Plano do MVP (módulo `Shards`)

Jogo idle web de mineração de minérios (Bronze, Prata, Ouro, Rubi, Safira, Esmeralda...). Este documento consolida as decisões de planejamento do backend e serve de contexto para a implementação.

**Decisão de arquitetura:** o jogo entra como **novo módulo dentro do UniqueServer** (não é um projeto de backend separado), seguindo o padrão do `FinancialService`. Reaproveita JWT/`uid`, `BaseController`, `BaseModels`, rate limiter, CORS, Serilog, Scalar/OpenAPI, Postgres e o deploy atual. Para facilitar uma separação futura: `DbContext` e connection string próprios, **sem FK para tabelas de outros módulos**, e vínculo com o usuário apenas por `UserId` (int, claim `uid`).

---

## 1. Convenções do projeto

- Pasta e projeto: `Shards/Shards.csproj`, namespace `Shards`, `ShardsDbctx`, connection string `ShardsConn`. Testes em `ShardsTests`.
- Subpastas: `Model/DTO`, `Model/Req`, `Model/Res`, `Repo`, `Service`, `Migrations` (como no Financial).
- Entidades com sufixo `DTO` e `[Table]`, `int Id` identity, datas `DateTime` em UTC.
- Services nomeados pela entidade: `PlayerService`, `MineService`, `InventoryItemService`, `PlayerSkillService`, `PlayerMissionService`. Atenção: já existe o namespace `InventoryServices`, cuidado nos `using`.
- Migrations são **manuais** (não há `Migrate()` no `Program.cs`): `Add-Migration ... -Context ShardsDbctx` / `update-database -Context ShardsDbctx`.
- Erros usam objeto `{ code, message }` com enum próprio do Shards (não alterar o `ErrorCode` compartilhado do `BaseModels`).

---

## 2. Entidades

Todas com `int Id` identity e token de concorrência `xmin` (Postgres) nas entidades mutáveis.

**`PlayerDTO`** (1 por usuário, criado no primeiro acesso)
- `UserId` (único), `CreatedAt`, `UpdatedAt`
- `Level`, `Experience`, `SkillPoints`
- `Energy`, `MaxEnergy`, `LastEnergyUpdateUtc`

**`PlayerSkillDTO`** (extensível)
- `PlayerId`, `SkillType`, `Level`; único em (`PlayerId`, `SkillType`)
- Linha criada ao investir o primeiro ponto. Nível máximo e efeito por nível em configuração.

**`MineDTO`**
- `PlayerId`, `MineNumber` (único junto com `PlayerId`)
- `CartCapacity` (100), `LastCartCollectionUtc`, `OresPerMinute` (0.33, **auto-mineração já ativa**)
- A Mina 1 é criada junto com o `Player`.

**`InventoryItemDTO`**
- `PlayerId`, `OreType`, `Quantity`; único em (`PlayerId`, `OreType`)

**`PlayerMissionDTO`**
- `PlayerId`, `MissionType` (único junto com `PlayerId`), `Status` (Active/Completed/Claimed)
- `Progress`, `CompletedAt`, `ClaimedAt`

**Enums:** `OreType` (Bronze=1, Silver=2, Gold=3, Ruby=4, Sapphire=5, Emerald=6), `SkillType` (`DoubleDrop`), `MissionType` (`FirstPickaxe`, `Specialization`, `OperationalExpansion`), `MissionStatus`.

### Fora do MVP
Upgrade de vagonete e de auto-mineração (campos existem, mecanismo não), minas além da 2, outras skills, títulos/conquistas, catálogo de minérios em banco.

---

## 3. Regras e balanceamento

### Configuração (`GameBalanceOptions`, seção `Shards:Balance` em `appsettings`)
- Energia: máximo 100, custo 1 por mineração, 216 s por ponto (100 em 6 h).
- Drop da Mina 1: Bronze 85% (+1 XP), Prata 15% (+3 XP).
- Mina 2: custo 150 Bronze + 30 Prata. Vagonete 100, 0.33 min/min na Mina 1.
- Skill `DoubleDrop`: +15% de chance por nível, com nível máximo configurável. Vale só na mineração ativa.

### Curva de XP
XP acumulado para o nível N = `15 × N × (N+1) / 2` (15, 45, 90, 150, 225...). Cada nível ganho dá 1 ponto de habilidade. Uma ação pode subir mais de um nível.

### Missões (definição no código, progresso no banco)
Catálogo estático `MissionCatalog` (nome, meta, recompensa, ordem). Missão nova = item no catálogo, sem migration.

| Missão | Objetivo | Conclui quando | Recompensa |
|---|---|---|---|
| 1 `FirstPickaxe` "Primeira Picaretagem" | Gastar 10 de energia minerando | `Progress` chega a 10 | +15 XP |
| 2 `Specialization` "Especialização" | Alocar o 1º ponto de habilidade | Primeira distribuição de skill | 20 Bronze + 5 Prata |
| 3 `OperationalExpansion` "Expansão Operacional" | Desbloquear a 2ª mina (150 Bronze + 30 Prata) | Compra da Mina 2 | Título/conquista (futuro) |

- A Missão 1 nasce ativa com o `Player`. A seguinte é criada como `Active` ao resgatar a anterior.
- **Claim manual:** o objetivo marca `Completed`; um endpoint de claim paga e marca `Claimed`. Idempotente (o status só passa de `Completed` a `Claimed` uma vez).

### Comportamentos
- **Energia:** ao regenerar N pontos, avançar `LastEnergyUpdateUtc` em `N × 216 s`, sem perder o resto parcial. Com energia cheia, o timestamp é só reajustado para agora.
- **Vagonete:** `min(CartCapacity, minutos × OresPerMinute)` arredondado para baixo; cheio, a auto-mineração pausa (tempo extra se perde). Na coleta, a tabela de drop é sorteada para cada minério pendente. O `DoubleDrop` não se aplica à coleta.
- **Primeiro acesso:** `Player` + Mina 1 + Missão 1 em uma transação; tratar a corrida de duas requisições simultâneas via índice único em `UserId`, capturando a violação e relendo.
- **Concorrência:** `xmin` nas entidades mutáveis e retry curto (2 a 3 tentativas) em `DbUpdateConcurrencyException`, depois `409`. Testes específicos de concorrência ficam **fora do MVP**.
- **Tempo:** todo cálculo usa `TimeProvider` injetado, para testes determinísticos. `Random` injetável para o sorteio.

---

## 4. API

Controller `ShardsController`: `[Route("[Controller]")]`, `[ApiController]`, `[Authorize]`, herdando `BaseController` (usa `Uid`; nenhuma rota recebe `playerId`). Em produção o prefixo `/api` vem do `UsePathBase`. Minas são endereçadas por `mineNumber`, não por Id. Todos os instantes em UTC e as respostas trazem `serverTimeUtc`.

| # | Método e rota | Descrição |
|---|---|---|
| 1 | `GET /shards/state` | Estado completo; cria o jogador no primeiro acesso |
| 2 | `POST /shards/mines/{mineNumber}/mine` | Mineração ativa, com `times` |
| 3 | `POST /shards/mines/{mineNumber}/collect-cart` | Esvazia o vagonete |
| 4 | `POST /shards/skills/distribute` | Gasta 1 ponto numa skill |
| 5 | `POST /shards/mines/unlock` | Compra a próxima mina (`MineNumber` máximo + 1) |
| 6 | `POST /shards/missions/{missionType}/claim` | Resgata a recompensa |

### 1. `GET /shards/state`
Recalcula e persiste a energia. Resposta `ShardsStateRes`:
- `serverTimeUtc`
- `player`: `level`, `experience`, `experienceToNextLevel`, `skillPoints`, `energy`, `maxEnergy`, `nextEnergyAtUtc` (nulo se cheia)
- `skills`: `{ type, level, maxLevel }[]`
- `inventory`: `{ oreType, quantity }[]`
- `mines`: `{ mineNumber, cartCapacity, cartAmount, cartFullAtUtc, oresPerMinute }[]`
- `missions`: `{ type, status, progress, goal }[]` (ativas ou concluídas não resgatadas)
- `nextMine`: `{ mineNumber, cost: [{ oreType, quantity }], canAfford }` (nulo se não houver)

### 2. `POST /shards/mines/{mineNumber}/mine`
Body opcional `{ "times": N }` (padrão 1, máximo 100, validado no servidor).
1. Regenera energia; exige energia para N, senão `NotEnoughEnergy`.
2. Debita N e ajusta o timestamp.
3. Sorteia N drops; com `DoubleDrop`, chance (nível × 15%) de 2 minérios pelo custo de 1.
4. Soma XP e sobe nível(is); atualiza inventário.
5. Incrementa a Missão 1 em N e marca `Completed` ao atingir a meta, tudo na mesma transação.

Resposta `MineRes`: `drops` (`{ oreType, quantity }[]`), `doubleDrops` (quantidade), `xpGained`, `levelUp`, `player`, `missions` (os que mudaram).

### 3. `POST /shards/mines/{mineNumber}/collect-cart`
Calcula o pendente; se for 0, `CartEmpty`. Sorteia, soma ao inventário e define `LastCartCollectionUtc = agora`. Resposta `CollectCartRes`: `collected`, `total`, `cart`, `inventory`.

### 4. `POST /shards/skills/distribute`
Body `{ "skillType": "DoubleDrop" }`. Exige `SkillPoints ≥ 1` e nível abaixo do máximo. Debita 1 ponto, cria ou incrementa `PlayerSkill`, conclui a Missão 2 se ativa. Resposta: `skills`, `skillPoints`, `missions` (os que mudaram).

### 5. `POST /shards/mines/unlock`
Valida que há próxima mina configurada e que o inventário cobre o custo. Debita, cria a `Mine` com os padrões, conclui a Missão 3 se ativa. Resposta: `mine`, `inventory`, `missions` (os que mudaram).

### 6. `POST /shards/missions/{missionType}/claim`
Exige `Completed` (senão `MissionNotCompleted`). Aplica a recompensa (XP e/ou minérios), marca `Claimed` e cria a próxima missão como `Active`. Resposta: `rewards` (`{ xp, ores[] }`), `levelUp`, `player`, `inventory`, `missions` (resgatada e nova).

### Erros
Objeto `{ "code": "...", "message": "..." }` em `BadRequest`; conflito de concorrência retorna `409` com `ConcurrencyConflict`.
Códigos: `PlayerNotFound`, `MineNotFound`, `NotEnoughEnergy`, `CartEmpty`, `NoSkillPoints`, `SkillMaxLevel`, `InvalidSkill`, `NotEnoughResources`, `NoMoreMines`, `MissionNotCompleted`, `MissionNotFound`, `InvalidTimes`, `ConcurrencyConflict`.

### Rate limit
A política `fixed` atual (8 req / 12 s, fila de 4) é estreita para o jogo. Criar uma política **`shards`** dedicada (proposta: 30 requisições a cada 10 s por usuário), aplicada só ao `ShardsController`. O `MapControllers().RequireRateLimiting("fixed")` global precisa ser ajustado para não sobrepor a política do Shards.

---

## 5. Plano de implementação

### Fase 0 — Esqueleto  ✅ concluída (branch `shards-mvp`)
- Criar `Shards/Shards.csproj` (net10.0, mesmos pacotes EF/Npgsql do Financial), adicionar à `UniqueServer.sln` numa solution folder `Shards` e referenciar no `UniqueServer.csproj`.
- `ConnectionStrings:ShardsConn` em `appsettings.Development.json` (ignorado pelo git). O `appsettings.json` não guarda connection strings (produção usa configuração do servidor), então **a produção precisa ganhar `ShardsConn` antes do deploy**, senão a API não sobe (`GetConfigValue` lança exceção).
- Registrar em `BuilderServicesCollection`: `AddDbContextFactory<ShardsDbctx>` (com `EnableRetryOnFailure`), repos e services num bloco `shards`.
- Criar `ShardsTests/ShardsTests.csproj` (xUnit, padrão dos outros testes).
- **Pronto quando:** a solução compila e a API sobe com o novo `DbContext` registrado.

### Fase 1 — Modelo e banco  ✅ código e migration prontos (falta aplicar no banco de desenvolvimento)
- DTOs e enums da seção 2; `ShardsDbctx` com `UseIdentityByDefaultColumns`, índices únicos, FKs por `PlayerId` e `xmin`.
- Migration `Init` e aplicação no banco de desenvolvimento.
- **Pronto quando:** schema criado e índices únicos presentes na migration.

### Fase 2 — Configuração e regras puras  ✅ concluída (30 testes passando)
- `GameBalanceOptions` via `IOptions`, `MissionCatalog` estático e serviço de regras puras (sem banco): regeneração de energia, nível e XP, minério pendente do vagonete, sorteio de drop. `TimeProvider` e `Random` injetáveis.
- **Pronto quando:** testes unitários cobrem regeneração (resto parcial, energia cheia, múltiplos pontos), curva de XP (15/45/90/150/225) e limite do vagonete.

### Fase 3 — Erros, DTOs e controller base  ✅ concluída (47 testes passando)
Decisões da implementação (valem para as fases seguintes):
- Entradas que podem vir inválidas chegam como **texto** e são convertidas no service com `ShardsErrors.ParseSkillType` / `ParseMissionType` (`skillType` no body, `missionType` na rota). Assim o erro sai em `{ code, message }` e não no ProblemDetails automático do ASP.NET. Os controllers das próximas fases devem receber `string missionType` na rota.
- Enums do Shards serializam como **nome** (`[JsonConverter(JsonStringEnumConverter)]`), sem mexer na configuração JSON global.
- `MineReq.Times` tem padrão 1; o limite vem de `Energy.MaxMinesPerRequest` (100) no appsettings. O endpoint `mine` deve usar `[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]`, já que o body é opcional.
- `ShardsController.RunAsync` traduz `ShardsException` (400, ou 409 para `ConcurrencyConflict`). Os endpoints reais entram nas Fases 4 a 6.
- **Rate limit:** a política `fixed` é um balde único para toda a API. A política `shards` é por jogador (claim `uid`, fallback por IP), 30 req / 10 s. Para ler a claim, `UseRateLimiter()` foi movido para depois de `UseAuthorization()` em `Program.cs`, e o `fixed` global passou a ser aplicado só aos controllers sem política própria. Verificado num host isolado: Shards 30 aceitas e 10 barradas em 40 chamadas; os demais controllers continuam em 8.
- `ShardsErrorCode`, `ShardsErrorRes`, `ShardsException`, helper de tradução no controller (400 e 409) e todos os DTOs de Req/Res.
- Política de rate limit `shards`.

### Fase 4 — `GET /shards/state`  ✅ código e testes prontos (63 testes); falta validar contra o Postgres real
Decisões da implementação:
- Os services do Shards usam o `IDbContextFactory<ShardsDbctx>` direto (não há repositório por entidade como nos outros módulos): cada comando grava várias entidades no mesmo `SaveChanges`. Um `DbContext` novo por tentativa.
- `ConcurrencyRetry.RunAsync` envolve cada comando (3 tentativas; depois `ConcurrencyConflict`/409). As Fases 5 e 6 devem usá-lo.
- `ShardsMapper` converte entidades em Res e é reutilizado pelos demais endpoints. Skills sempre listam todas (nível 0 se não possui); o inventário omite quantidade 0; missões `Claimed` ficam ocultas.
- `GET /state` só grava quando a energia mudou; com a energia cheia não há escrita.
- Criação do jogador: `Player` + Mina 1 + Missão 1 numa transação (com `CreateExecutionStrategy`, por causa do `EnableRetryOnFailure`). Em corrida, o índice único em `UserId` faz o perdedor reler o jogador criado.
- Testes usam EF InMemory, que não impõe índices únicos nem `xmin`: a corrida do primeiro acesso e a concorrência real só se confirmam no Postgres.
- `PlayerService`: buscar ou criar `Player` + Mina 1 + Missão 1 em transação, tratando a corrida do primeiro acesso; regenerar e persistir energia; montar `ShardsStateRes`.
- **Pronto quando:** o primeiro `GET` cria o jogador e o segundo devolve o mesmo, sem duplicar.

### Fase 5 — Mineração e vagonete  ✅ código e testes prontos (93 testes); falta validar contra o Postgres real
Decisões da implementação:
- `MineService` tem `MineAsync` (`times` 1 a `MaxMinesPerRequest`, validado antes de abrir o banco) e `CollectCartAsync`. Ambos rodam dentro de `ConcurrencyRetry`.
- Mineração: debita `times × CostPerMine` de energia (regenerando antes e preservando o resto parcial), sorteia 1 drop por mineração, o `DoubleDrop` soma 1 minério extra (o XP é o do drop, uma vez por mineração), sobe de nível (1 ponto de habilidade por nível) e avança a Missão 1 pela energia gasta, tudo no mesmo `SaveChanges`. Sem energia suficiente nada é gravado (`NotEnoughEnergy`).
- Coleta do vagonete: sorteia a tabela de drop para cada minério pendente, sem XP e sem drop duplo; o relógio do vagonete volta para agora, então a fração de minério pendente se perde. Vagonete vazio dá `CartEmpty`.
- Helpers reutilizáveis nas Fases 6: `MissionTracker.Advance` (progresso/conclusão), `InventoryItemService` (`Add`, `CanAfford`, `Remove`) e `ShardsQueries` (jogador, mina, inventário, missões e skill rastreados).
- `ConcurrencyRetry` agora também repete a violação de índice único do Postgres (23505): dois minérios do mesmo tipo novos ao mesmo tempo criariam duas linhas de inventário.
- Endpoint `mine`: body opcional (`EmptyBodyBehavior.Allow`), verificado num host isolado: sem body, `{}` e `{"times":7}` funcionam. `{"times":"abc"}` cai no ProblemDetails automático do ASP.NET (400), fora do formato `{ code, message }`; é entrada malformada, não regra de negócio.
- `mine` com `times`, subida de nível múltipla e progresso da Missão 1; `collect-cart`; helper de retry de concorrência.
- **Pronto quando:** `times = 100` gasta 100 de energia numa só transação e a coleta zera o vagonete.

### Fase 6 — Skills, missões e Mina 2  ✅ código e testes prontos (124 testes); falta validar contra o Postgres real
Decisões da implementação:
- Novo código de erro `MissionAlreadyClaimed` (400) para o resgate repetido, em vez de reaproveitar `MissionNotCompleted`, que dava uma mensagem enganosa. Total: 14 códigos.
- `skills/distribute`: o nível máximo é checado antes dos pontos (`SkillMaxLevel` antes de `NoSkillPoints`). Conclui a Missão 2 se ela estiver ativa.
- `missions/{missionType}/claim`: paga XP (pode subir de nível e dar ponto de habilidade) e minérios, marca `Claimed` e cria a próxima da cadeia. Duas requisições simultâneas pagam uma só vez (token `xmin` da missão + `ConcurrencyRetry`).
- **Missão já cumprida antes de liberada:** a missão seguinte só nasce no resgate da anterior. Se o jogador já fez o que ela pede (gastou o ponto de habilidade, ou comprou a Mina 2), ela nasce **concluída** (`MissionTracker.CompleteIfAlreadySatisfied`), em vez de exigir repetir a ação. O PDF não cobre esse caso: mineração pode dar nível 1 antes de a Missão 1 ser resgatada.
- `mines/unlock`: compra a próxima mina configurada (maior `MineNumber` do jogador + 1); `NotEnoughResources` não altera nada; `NoMoreMines` se não houver. A mina nova começa com o vagonete vazio e conclui a Missão 3 se ativa.
- Body de `skills/distribute` e `skillType`/`missionType` são texto, validados no service (`InvalidSkill`/`MissionNotFound`).
- Teste de ponta a ponta do onboarding (primeiro acesso, Missões 1 a 3, compra da Mina 2).
- `skills/distribute` e Missão 2; `missions/claim` (idempotente, ativa a seguinte); `mines/unlock` e Missão 3.
- **Pronto quando:** o onboarding (Missões 1 a 3) funciona de ponta a ponta e um claim repetido não paga duas vezes.

### Fase 7 — Testes e revisão  ✅ concluída (151 testes; revisão `/code-review` high feita e corrigida)
Resultado da revisão (10 achados): 7 corrigidos, 3 mantidos de propósito.
- Corrigidos: `Enum.TryParse` aceitava lista com vírgula (agora só letras); rate limiter agora fica entre `UseAuthentication` e `UseAuthorization` (lê a claim `uid` e os 401/403 continuam limitados, verificado com 40 requisições sem token); `PlayerService` só trata violação de unicidade e preserva a causa; `GameBalanceOptionsValidator` + `ValidateOnStart` (a API não sobe com balanceamento inválido, verificado); `MissionTracker.AdvanceChanged` elimina o boilerplate; contas de XP em `long` (sem laço infinito/overflow); migration vazia removida.
- Mantidos: (1) `ShardsConn` ausente derruba a API inteira, igual aos outros módulos (fail-fast): **é item obrigatório do checklist de deploy**; (2) 5 a 6 consultas por mineração: otimizar só se a medição pedir; (3) testes de concorrência no Postgres ficam fora do MVP.
- OpenAPI: as rotas declaram `ProducesResponseType` (sucesso, `ShardsErrorRes` 400/409), então o Scalar mostra os modelos.

**Checklist antes de colocar em produção**
1. Criar o banco `Shards` e aplicar a migration `Init` (`update-database -Context ShardsDbctx`); conferir `__EFMigrationsHistory` (não deve haver entradas de migrations que não existem mais, como `20261006205114_Init` e `20261006210459_202610061`).
2. Configurar `ConnectionStrings:ShardsConn` no servidor.
3. Testar login e um endpoint de outro módulo (a ordem do rate limiter mudou para toda a API).
4. Validar os 6 endpoints contra o Postgres real, incluindo duas requisições simultâneas.

Fase 7 original:
- Testes de serviço com `DbContext` em memória: onboarding completo, regeneração com resto, vagonete cheio, claim repetido, recursos insuficientes, nível máximo da skill, `times` fora do limite.
- Testes de concorrência fora do MVP (decisão registrada).
- Revisão com `/code-review` e conferência do Scalar/OpenAPI.

---

## 6. Riscos e pontos de atenção
1. Corrida no primeiro acesso: resolvida por índice único em `UserId` e releitura (Fase 4).
2. Lote `times`: limitar a 100 e validar no servidor.
3. Sem testes de concorrência no MVP: o `xmin` e o retry são implementados, mas a garantia não é validada automaticamente. Reavaliar antes de abrir o jogo ao público.
4. A política de rate limit global (`MapControllers().RequireRateLimiting("fixed")`) precisa conviver com a política `shards`.
5. Balanceamento (drops, XP, custos) em `appsettings`, ajustável sem recompilar.

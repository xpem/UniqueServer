# Ancestral Shards — Plano do MVP (módulo `Shards`)

Jogo idle web de mineração (quartzo como moeda básica; gemas como Rubi, Safira e Esmeralda nas próximas etapas). Este documento consolida as decisões de planejamento do backend e serve de contexto para a implementação.

**Decisão de arquitetura:** o jogo entra como **novo módulo dentro do UniqueServer** (não é um projeto de backend separado), seguindo o padrão do `FinancialService`. Reaproveita JWT/`uid`, `BaseController`, `BaseModels`, rate limiter, CORS, Serilog, Scalar/OpenAPI, Postgres e o deploy atual. Para facilitar uma separação futura: `DbContext` e connection string próprios, **sem FK para tabelas de outros módulos**, e vínculo com o usuário apenas por `UserId` (int, claim `uid`).

> **Refactor dos recursos (2026-10):** Bronze, Prata e Ouro foram substituídos por um único **Quartzo**, a moeda básica do jogo.
> Metais não fazem parte do caminho "quartzo (moeda) -> gemas brutas -> lapidação". Números mantidos no mesmo ritmo:
> 200 Quartzo para a Mina 2 (antes a Prata, no 15%, limitava a compra a ~200 minérios) e 25 Quartzo de recompensa na Missão 2.
> **Visão de longo prazo (fora do MVP):** quartzo compra minas e desbloqueia gemas brutas; a Oficina de Lapidação (bruto -> polido -> facetado)
> custa quartzo e tempo; gemas polidas terão elemento e ataque. O inventário vai precisar de um estágio por item (ex.: Rubi polido) e
> existirá venda de minérios. Nada disso está implementado.

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

**Enums:** `OreType` (Quartz=1, Ruby=4, Sapphire=5, Emerald=6; os valores 2 e 3 ficam sem uso para não alterar dados já gravados), `SkillType` (`DoubleDrop`), `MissionType` (`FirstPickaxe`, `Specialization`, `OperationalExpansion`), `MissionStatus`.

### Fora do MVP
Upgrade de vagonete e de auto-mineração (campos existem, mecanismo não), minas além da 2, outras skills, títulos/conquistas, catálogo de minérios em banco.

---

## 3. Regras e balanceamento

### Configuração (`GameBalanceOptions`, seção `Shards:Balance` em `appsettings`)
- Energia: máximo 100, custo 1 por mineração, 216 s por ponto (100 em 6 h).
- Produções (tipo de produção da mina): Quartz 100% (+1 XP); gemas Sapphire, Ruby e Emerald com 90% da gema (+3 XP) e 10% de quartzo (+1 XP). Toda mina nasce em Quartz.
- Vagonete: a capacidade sobe por mina com a skill `CartCapacity` (teto, nível 0 a 5) e quartzo (nível 1 = 150 por 300; 2 = 220 por 800; 3 = 320 por 2.000; 4 = 450 por 5.000; 5 = 600 por 12.000).
- Minas: o jogador começa com 1; a skill `MineSlots` libera espaços (1 + nível, até 5 minas, nível máximo 4). Cada mina nova também custa quartzo: Mina 2 = 200, 3 = 600, 4 = 1.800, 5 = 5.000. Trocar a produção de uma mina: Sapphire 500, Ruby 1200, Emerald 3000 de Quartzo (por mina, a cada mudança; voltar ao Quartz é de graça). Vagonete 100, 0.33 min/min na Mina 1.
- Skill `DoubleDrop`: +15% de chance por nível, com nível máximo configurável. Vale só na mineração ativa.

### Curva de XP
XP acumulado para o nível N = `15 × N × (N+1) / 2` (15, 45, 90, 150, 225...). Cada nível ganho dá 1 ponto de habilidade. Uma ação pode subir mais de um nível.

### Missões (definição no código, progresso no banco)
Catálogo estático `MissionCatalog` (nome, meta, recompensa, ordem). Missão nova = item no catálogo, sem migration.

| Missão | Objetivo | Conclui quando | Recompensa |
|---|---|---|---|
| 1 `FirstPickaxe` "Primeira Picaretagem" | Gastar 10 de energia minerando | `Progress` chega a 10 | +15 XP |
| 2 `Specialization` "Especialização" | Alocar o 1º ponto de habilidade | Primeira distribuição de skill | 25 Quartzo |
| 3 `OperationalExpansion` "Expansão Operacional" | Desbloquear a 2ª mina (200 Quartzo) | Compra da Mina 2 | Título/conquista (futuro) |

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

---

## 7. Evolução 1 — Tipo de produção da mina  ✅ pronta (183 testes)

Decisão de design: cada mina tem um **tipo de produção**. O jogador paga quartzo para mudá-lo e a mina passa a produzir a gema escolhida, com um pouco de quartzo.

- **Modelo:** `MineDTO.ProductionType` (`OreType`, padrão `Quartz`). Migration `AddMineProduction` (`ALTER TABLE "Mine" ADD "ProductionType" integer NOT NULL DEFAULT 1`): as minas existentes continuam em Quartzo. **Aplicar com `update-database -Context ShardsDbctx`.**
- **Configuração:** a tabela de drop saiu de `Mines[].Drops` e virou `Productions[]` (`Focus`, `SwitchCost`, `Drops`). `Mines[]` guarda só capacidade, taxa e custo de desbloqueio. O validador exige a produção Quartz, sem repetidas, e que os drops incluam o próprio minério.
- **Regras:**
  - O custo é **por mina e a cada mudança** (Safira → Rubi paga o Rubi). Não existe desbloqueio global.
  - Mudar para **Quartz é de graça** (evita travar quem ficou sem quartzo).
  - A troca **esvazia o vagonete antes, com a produção antiga**: o tempo acumulado foi trabalhado nela. O que sai do vagonete já conta para pagar a troca. Sem recursos suficientes, nada é gravado, nem a coleta.
  - Mina 1 e Mina 2 podem mudar. Mesma produção: `SameProduction`; tipo inexistente: `InvalidProduction`.
  - A mineração ativa e o vagonete passam a sortear a tabela da produção da mina. O `DoubleDrop` vale também para gemas.
- **API:** `POST /Shards/mines/{n}/production` com `{ "focus": "Sapphire" }`. O `state` mostra `mines[].production` e `productions[]` (custo, probabilidades normalizadas e `canAfford`).
- **Missão 4 "Primeira Gema":** conclui ao pôr uma mina em gema (voltar ao quartzo não conta); recompensa de 50 XP. Nasce quando a Missão 3 é resgatada, ou já concluída se o jogador já tiver uma mina em gema.
- **Missões novas do catálogo chegam a quem já resgatou a anterior:** o `GET /state` cria as missões que faltam na cadeia (só grava quando falta alguma). Isso vale para jogadores que já tinham resgatado a Missão 3 antes da Missão 4 existir.
- **Fora do escopo (próximas etapas):** venda/mercado de gemas por quartzo, Oficina de Lapidação (estágio por item), combate.

---

## 8. Evolução 2 — Skill de espaços de mina (`MineSlots`)  ✅ pronta (196 testes)

O número de minas que o jogador pode ter virou uma **skill**: começa com 1 mina e cada ponto de habilidade em `MineSlots` libera mais um espaço, até **5 minas** (nível máximo 4, base de 1 espaço).

- **Dois requisitos para comprar uma mina:** espaço livre (skill) **e** quartzo (`UnlockCost` da mina). Sem espaço, `POST /mines/unlock` devolve `MineSlotsFull` **antes de cobrar**. A ordem das verificações é: próxima mina configurada (`NoMoreMines`), espaço (`MineSlotsFull`), recursos (`NotEnoughResources`).
- **Configuração:** `Shards:Balance:MineSlots` (`BaseSlots` 1, `MaxLevel` 4) e as Minas 3, 4 e 5 em `Mines[]` (custos 600 / 1.800 / 5.000 de quartzo, mesma capacidade e taxa da Mina 1). O validador exige `BaseSlots >= 1` e `MaxLevel >= 0`.
- **API:** `SkillType.MineSlots` (o `distribute` já era genérico); o `state` ganhou `mineSlots { used, total }` e `nextMine.hasSlot`; erro novo `MineSlotsFull`. Sem mudança de banco (as skills são linhas por jogador).
- **Contas antigas:** quem já tem mais minas que espaços **mantém todas**; o total exibido é o maior entre os espaços da skill e as minas que o jogador já tem, e só novas compras ficam bloqueadas.
- **Missões:** a Missão 2 continua concluindo com o primeiro ponto em **qualquer** skill; a descrição da Missão 3 avisa que a compra exige um ponto em `MineSlots`. Um jogador que gaste o primeiro ponto em `DoubleDrop` só compra a Mina 2 no próximo nível (45 XP de experiência acumulada).
- **Disputa de pontos:** `DoubleDrop` (até 5 níveis) contra `MineSlots` (até 4 níveis): o jogador escolhe onde investir.

---

## 9. Evolução 3 — Ampliar o vagonete (skill `CartCapacity` + upgrade por mina)  ✅ pronta (215 testes)

Mais capacidade no vagonete aumenta o tempo que a mina trabalha sozinha (hoje 100 minérios ≈ 5 h; no nível 5, 600 ≈ 30 h) e dá mais sentido aos níveis do jogador.

- **Mesmo desenho do desbloqueio de minas:** a **skill dá a permissão** e o **quartzo paga**. `CartCapacity` (nível 0 a 5) é o **teto**: com a skill no nível N, **cada** mina pode ser ampliada até o nível N (não é um total a distribuir entre as minas). Cada melhoria custa quartzo, **por mina**.
- **Tabela (configuração `Shards:Balance:CartUpgrades`):** nível 1 = 150 (300), 2 = 220 (800), 3 = 320 (2.000), 4 = 450 (5.000), 5 = 600 (12.000) [capacidade (custo)]. O nível máximo da skill é o maior nível configurado. O validador exige níveis de 1 a N sem buracos, capacidade crescente e custos positivos.
- **Modelo:** `MineDTO.CartLevel` (padrão 0). Migration `AddMineCartLevel` (`ALTER TABLE "Mine" ADD "CartLevel" integer NOT NULL DEFAULT 0`): as minas atuais ficam no nível 0 com a capacidade que já têm. A melhoria grava a nova capacidade em `CartCapacity`.
- **API:** `POST /Shards/mines/{n}/upgrade-cart`. O `state` ganhou `mines[].cartLevel` e `cartUpgrades[]` (por mina: nível e a próxima melhoria com capacidade, custo, `canAfford` e `skillAllows`; `next` é nulo no nível máximo). `SkillType.CartCapacity`. Erros novos: `CartUpgradeLocked` (a skill ainda não permite o nível) e `CartMaxLevel`.
- **Ordem das verificações:** nível máximo → skill → recursos. Sem permissão, nem chega a cobrar.
- **O vagonete é esvaziado antes de ampliar** (como na troca de produção): sem isso, a capacidade nova contaria retroativamente o tempo em que o vagonete já estava cheio e a produção estava parada. O que sai do vagonete já conta para pagar; se faltar recurso, nada é gravado, nem a coleta.
- **Só capacidade.** Velocidade (`OresPerMinute`) fica para depois: a tabela de níveis foi pensada para aceitar uma segunda trilha.
- **Observação de balanceamento:** com as três skills (DoubleDrop 5 + MineSlots 4 + CartCapacity 5) são ~14 pontos, o nível 14 (1.575 de XP): os pontos acabam em poucos dias e o quartzo (≈100 mil para ampliar 5 minas ao máximo) passa a ser o freio de longo prazo. Mercado e outras fontes de quartzo entram nos próximos passos.

---

## 10. Evolução 4 — Estágio no inventário (`AddItemStage`)  ✅ pronta (223 testes)

Fundação da futura Oficina de Lapidação: o inventário passa a distinguir o **estágio** do item (bruto → polido → facetado). Nada da oficina, combate ou mercado foi implementado.

- **Decisões:**
  - O **quartzo continua um `OreType` no inventário** (moeda e minério ao mesmo tempo); separar a moeda numa coluna do jogador ficou de fora por ser um refactor grande (custos, migrations e testes) sem ganho imediato. Pode virar uma feat à parte.
  - Concorrência no padrão do módulo: `xmin` + `ConcurrencyRetry` (nada de SQL cru com `ON CONFLICT`). O índice único cobre a corrida de duas linhas novas.
  - `CartEmpty` já existia (código 4); nenhum código de erro novo, pois nenhum fluxo recebe estágio de entrada. `InvalidStage`/`ItemNotFound` entram só quando a oficina precisar.
- **Modelo:** enum `ItemStage` (`Raw = 0`, `Polished = 1`, `Faceted = 2`) e `InventoryItemDTO.Stage` (padrão `Raw`). O índice único passou de (`PlayerId`, `OreType`) para (`PlayerId`, `OreType`, `Stage`).
- **Migration `AddItemStage`:** `ADD "Stage" integer NOT NULL DEFAULT 0` + troca do índice único. As linhas existentes viram Raw sem migrar dados. **Aplicar com `update-database -Context ShardsDbctx`.**
  - **Rollback:** o `Down` recria o índice antigo; ele falha se já existirem linhas em estágios diferentes do mesmo minério. Antes de reverter depois que a oficina existir, é preciso consolidar ou apagar essas linhas.
  - A migration vazia `20261017201500_17102026` (criada antes, sem mudanças) continua na cadeia, é inofensiva.
- **Regra central:** todo custo e saldo do jogo olha **só o estágio Raw** (`InventoryItemService.Remove/CanAfford`, mapper). Sem isso, uma linha polida de quartzo pagaria uma mina.
- **API:** `GET /Shards/inventory` → `{ items: [{ oreType, stage, quantity }] }`, só quantidade > 0, ordenado por minério e estágio; jogador ainda inexistente recebe lista vazia (o jogador continua sendo criado só em `GET /state`). O `state.inventory` e as respostas de `mine`/`collect-cart` seguem inalterados (só o bruto): o front não quebra.
- **Mineração e vagonete** continuam gravando em Raw (`InventoryItemService.Add` com estágio padrão).
- **Pendente:** teste de concorrência contra Postgres real (já era pendência das fases anteriores).

---

## 11. Evolução 5 — Oficina de Lapidação (skill `Workshop`)  ✅ pronta (237 testes)

Transforma gema bruta em polida e polida em facetada, usando o estágio do inventário (Evolução 4). Só a lapidação: combate, mercado e venda continuam fora.

- **Decisões:** instantânea (sem fila de tempo), **determinística** (sem chance de falha), custo em **gemas do estágio anterior + quartzo bruto**, liberada pela skill nova `Workshop`.
- **Skill `Workshop`** (`SkillType` = 4, nível máximo = número de receitas, hoje 2): nível 1 libera Polished, nível 2 libera Faceted. Sem permissão, `WorkshopLocked` **antes de cobrar**. Compete por pontos com DoubleDrop (5), MineSlots (4) e CartCapacity (5): ~16 pontos no total.
- **Receitas (`Shards:Balance:Workshop`, ajustáveis sem recompilar):** `Polished` = 5 gemas brutas + 100 Quartz → 1 polida; `Faceted` = 3 polidas + 400 Quartz → 1 facetada (1 facetada = 15 brutas + 700 quartzo). O validador exige níveis de 1 a N sem buracos, `Stage` de mesmo número do nível, `InputQuantity > 0` e custos positivos.
- **API:** `POST /Shards/workshop/craft` com `{ "gem": "Ruby", "stage": "Polished", "times": 1 }` (`times` 1 a 100, padrão 1). Resposta: `crafted`, `spent` (gemas + quartzo) e `inventory` completo (por estágio). Tudo numa transação, com `ConcurrencyRetry`.
- **Erros novos:** `InvalidGem` (20; inclui Quartz, que não tem estágios), `InvalidStage` (21; inclui Raw, que não se fabrica) e `WorkshopLocked` (22). Total: 22 códigos. Ordem: entradas inválidas → `InvalidTimes` → jogador → skill → recursos.
- **Regras mantidas:** o custo em quartzo sai só do estágio **Raw** (quartzo polido não paga nada); faltando gema **ou** quartzo, nada é gravado.
- **Sem migration:** a skill é uma linha por jogador e as receitas são configuração.
- **`GET /state`:** ganhou `workshop[]`, uma entrada por receita (`stage`, `level`, `inputQuantity`, `cost`, `skillAllows`) com `gems[]` (Ruby, Sapphire, Emerald): `available` (quantas do estágio consumido) e `canAfford` (gemas + quartzo de 1 lapidação; a skill é um campo à parte).
- **Fora do escopo:** elemento/ataque nas gemas polidas (combate), mercado e venda.

---

## 12. Evolução 6 — Missões das novas mecânicas (vagonete, oficina, gemas)  ✅ pronta (265 testes)

A cadeia de missões passou de 4 para **17**, cobrindo as skills `CartCapacity`, `Workshop`, `DoubleDrop`, `MineSlots`, a produção de gemas e a lapidação. O catálogo continua no código (`MissionCatalog`) e o progresso no banco: **sem migration**; quem já resgatou a `FirstGem` recebe a `CartTraining` no próximo `GET /state` (`CreateMissingNext`).

| # | Tipo | Objetivo | Conclui quando | Recompensa |
|---|---|---|---|---|
| 5 | `CartTraining` | Ponto de habilidade em Vagonete | skill `CartCapacity` > 0 (`distribute`) | 20 XP |
| 6 | `CartUpgrade` | Ampliar o vagonete de uma mina | mina com `CartLevel` > 0 (`upgrade-cart`) | 30 XP + 50 Quartz |
| 7 | `OpenWorkshop` | Ponto de habilidade em Oficina | skill `Workshop` > 0 (`distribute`) | 30 XP + 100 Quartz |
| 8 | `FirstPolish` | Polir uma gema | existe linha `Polished` ou `Faceted` (`craft`) | 50 XP + 600 Quartz |
| 9 | `FirstFacet` | Facetar uma gema | existe linha `Faceted` (`craft`) | 100 XP |
| 10 | `Diversification` | Duas minas em gemas **diferentes** | 2 produções de gema distintas (`production`) | 30 XP + 300 Quartz |
| 11 | `Collector` | Obter Rubi, Safira e Esmeralda (qualquer estágio) | linhas das 3 gemas (`mine`, `collect-cart`, `production`) | 20 XP + 200 Quartz |
| 12 | `PolishBatch` | Polir 10 gemas | contagem (meta 10), soma o `times` de crafts `Polished` | 40 XP + 500 Quartz |
| 13 | `FullSet` | Uma gema facetada de cada tipo | linhas `Faceted` das 3 gemas (`craft`) | 60 XP + 500 Quartz |
| 14 | `FacetBatch` | Facetar 5 gemas | contagem (meta 5), soma o `times` de crafts `Faceted` | 80 XP + 1000 Quartz |
| 15 | `ThirdMine` | Ter 3 minas | `mines.Count >= 3` (`unlock`) | 30 XP + 300 Quartz |
| 16 | `FullCart` | Esvaziar um vagonete cheio | `collect-cart` com total = capacidade | 15 XP |
| 17 | `DoubleLuck` | `DoubleDrop` no nível 3 | skill `DoubleDrop` >= 3 (`distribute`) | 30 XP |

**Decisões:**
- **Condição x contagem.** As de condição leem o estado (`MissionTracker.IsSatisfied`) e valem também retroativamente (`CompleteIfAlreadySatisfied`): a missão nasce concluída se o jogador já fez a ação antes de ela ser liberada. As de contagem (`PolishBatch`, `FacetBatch`, `FullCart`) só avançam por ação e começam do zero quando liberadas.
- **Estágios pela existência da linha, não pela quantidade.** A linha do inventário nasce no primeiro item e não some; assim gastar (ou, no futuro, vender) o item não desfaz a missão. `Collector` e `FullSet` seguem a mesma regra.
- **Avaliação só do que a ação pode ter cumprido.** Cada serviço chama `MissionTracker.CompleteSatisfiedChanged` só com os tipos afetados e os fatos (`MissionFacts`: skills, minas, inventário) que eles leem; sem consultas extras desnecessárias. `collect-cart`, `upgrade-cart` e `craft` passaram a carregar as missões.
- **Respostas.** `CollectCartRes`, `UpgradeCartRes` e `CraftRes` ganharam `missions` (as que mudaram), como as demais.
- **Recompensas.** Só XP e **quartzo bruto** (nunca gema lapidada, para não pular a oficina). `FirstPolish` paga 600 de quartzo, o que fecha a conta até a primeira facetada (200 das 2 polidas restantes + 400). Total de XP das 13 novas: 440, ou cerca de 5 níveis (= 5 pontos de habilidade), o que cobre os 2 pontos que `CartTraining` e `OpenWorkshop` exigem.
- **Custo x recompensa.** `CartUpgrade` gasta 300 e paga 50: é um tutorial que gasta, de propósito.
- **Jogador sem pontos de habilidade** fica parado em `CartTraining`/`OpenWorkshop`/`DoubleLuck` até subir de nível (mesmo comportamento da `Specialization`).
- **Fora do escopo:** missões repetíveis (ex.: "lapide 10 gemas" a cada ciclo), para depois de observar o uso.

---

## 13. Evolução 7 — Missões diárias  ✅ pronta (296 testes)

Sistema à parte da cadeia do tutorial, para dar motivo de voltar todo dia: 3 tarefas básicas, um baú por completar todas e uma sequência (streak) de dias seguidos.

- **Liberação:** só depois de resgatar a Missão 1 (`FirstPickaxe`), para não competir com o início do tutorial. Antes disso `state.daily` é nulo.
- **Dia do jogo:** vira em `Shards:Balance:Daily:ResetHourUtc` (3 = meia-noite em Brasília), não à meia-noite UTC. Tudo usa o relógio do servidor (`TimeProvider`).
- **Criação preguiçosa, sem job:** o primeiro `GET /state` do dia cria as diárias (como o `CreateMissingNext`). Quem não entra, não gera linhas. A corrida de duas leituras simultâneas cai no índice único e o `ConcurrencyRetry` resolve.
- **Quais são sorteadas:** `Login` (sempre, já concluída ao criar) mais `MissionsPerDay - 1` do pool elegível. O sorteio é determinístico por (jogador, dia): o mesmo jogador vê as mesmas tarefas a cada leitura.
- **Pool (`DailyMissionCatalog`):**

| Tipo | Tarefa | Meta | Elegível se | Avança em | Recompensa |
|---|---|---|---|---|---|
| `Login` | Entrar no jogo | 1 | sempre | criação do dia | 5 XP + 20 Quartz |
| `Energy` | Gastar energia minerando | 50 | sempre | `mine` | 10 XP + 40 Quartz |
| `Cart` | Esvaziar um vagonete | 1 | sempre | `collect-cart` | 5 XP + 40 Quartz |
| `Quartz` | Obter quartzo | 100 | sempre | `mine`, `collect-cart` | 10 XP + 40 Quartz |
| `Gems` | Obter gemas brutas | 20 | tem mina em gema | `mine`, `collect-cart` | 10 XP + 60 Quartz |
| `Polish` | Polir gemas | 2 | Workshop >= 1 | `craft` (por `times`) | 15 XP + 100 Quartz |
| `Facet` | Facetar uma gema | 1 | Workshop >= 2 | `craft` (por `times`) | 20 XP + 150 Quartz |

- **Baú do dia:** exige as diárias de hoje **todas resgatadas**. Paga `Chest` (10 XP + 100 Quartz) e atualiza a sequência.
- **Streak:** o baú do dia N do ciclo paga `100 + (N - 1) * 20` de quartzo; no 7o dia (fim do ciclo) o total é dobrado (440); depois recomeça. Resgatar o baú no dia seguinte ao último continua a sequência; faltar um dia zera (a sequência já aparece quebrada no `state`). Config em `Daily.Chest` e `Daily.Streak`, validada na subida da API.
- **Recompensas:** só XP e quartzo bruto, sem gema e sem ponto de habilidade direto (o XP pode subir de nível como em qualquer ação).
- **API:**
  - `GET /state` ganhou `daily` (`day`, `resetsAtUtc`, `missions[]`, `chest { claimed, canClaim, reward }`, `streak { days, cycleDays }`).
  - `POST /Shards/daily/{type}/claim` (ex.: `Energy`) e `POST /Shards/daily/chest/claim`; ambos devolvem `DailyClaimRes` (`rewards`, `levelUp`, `player`, `inventory`, `daily`).
  - `mine`, `collect-cart` e `craft` ganharam `dailyMissions` (as que mudaram), separado de `missions` (tutorial).
- **Erros novos (23 a 27):** `DailyNotFound` (não é uma das de hoje, tipo inválido, ou de ontem), `DailyNotCompleted`, `DailyAlreadyClaimed`, `ChestNotReady`, `ChestAlreadyClaimed`.
- **Banco:** migration `AddDailyMissions` (tabela `DailyMission`, única por `PlayerId + Day + DailyType`, e `Player.DailyStreak` / `LastChestDay`). **Aplicar com `update-database -Context ShardsDbctx`.** Rollback: o `Down` apaga a tabela e as colunas (a sequência e o histórico de diárias se perdem).
- **Limitações assumidas:**
  - Ação feita depois da virada do dia, **sem refazer o `GET /state`**, não avança nada (as diárias do novo dia ainda não existem): o front deve refazer o `state` ao passar de `resetsAtUtc`.
  - O que o vagonete entrega dentro de `production` e `upgrade-cart` não conta para as diárias.
  - Diárias de ontem não resgatadas se perdem na virada.

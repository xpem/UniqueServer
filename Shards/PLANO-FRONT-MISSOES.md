# Ancestral Shards — Plano do front: missões novas e missões diárias

Roteiro para a **sessão do front** (Angular, `D:\Emanuel\Projetos\AncestralShards`) consumir o que o backend entregou nas Evoluções 6 e 7 (`PLANO.md`, seções 12 e 13). O contrato completo e os exemplos de JSON estão em `PLANO-FRONT-OFICINA.md` (seções 12 e 13); este documento é a **lista de trabalho**, na ordem, com os tipos prontos para colar.

> O backend não precisa de nada do front. Se faltar algum campo na API, descreva o pedido (rota, campo, motivo) para a sessão do backend, como no `PEDIDO-BACKEND-MISSOES.md`.

## 1. O que o front já tem (não refazer)
Conferido no código atual do front:

| Já implementado | Onde |
|---|---|
| `ItemStage`, `InventoryItem`, `InventoryRes`, `WorkshopRecipe`, `CraftRequest`, `CraftResult` | `models/shards.model.ts` |
| `GET /inventory`, `POST /workshop/craft` | `services/shards-api.ts` |
| `fullInventory`, `workshop`, `craft()`, `loadFullInventory()`, aba `workshop` | `services/game.store.ts` |
| Erros `InvalidGem`, `InvalidStage`, `WorkshopLocked` | `services/shards-errors.ts` |
| Componente da Oficina | `pages/game/components/workshop.ts` |
| Lista de missões genérica (`name`, `description`, `progress/goal`, recompensa vêm da API) | `pages/game/components/mission-list.ts` |

Ou seja: **as 13 missões novas funcionam na lista atual sem mudar a tela**, desde que o `MissionType` as aceite. O trabalho de verdade é (a) tipos e mesclagem das respostas e (b) o painel das diárias.

## 2. O que falta (resumo)
1. **Modelos:** `MissionType` com 13 valores novos; campo `missions` em `CollectCartResult`, `UpgradeCartResult` e `CraftResult`; `dailyMissions` em `MineResult`, `CollectCartResult` e `CraftResult`; `ShardsState.daily`; tipos das diárias.
2. **API:** 2 rotas de resgate das diárias.
3. **Erros:** 5 códigos novos no mapa de mensagens.
4. **Store:** sinal `daily`, mesclagem das diárias, resgates, refetch na virada do dia, selo (badge) da aba.
5. **UI:** painel "Missões do dia" com baú e sequência; avisos de missão concluída.
6. **Docs:** atualizar `docs/CONTRATO-FRONT.md` (cópia local) com os campos novos.

## 3. Fase F1 — Modelos, API e erros (pequena, sem risco)

### 3.1 `models/shards.model.ts`
```ts
export type MissionType =
  | 'FirstPickaxe' | 'Specialization' | 'OperationalExpansion' | 'FirstGem'
  | 'CartTraining' | 'CartUpgrade' | 'OpenWorkshop' | 'FirstPolish' | 'FirstFacet'
  | 'Diversification' | 'Collector' | 'PolishBatch' | 'FullSet' | 'FacetBatch'
  | 'ThirdMine' | 'FullCart' | 'DoubleLuck';

export type DailyMissionType = 'Login' | 'Energy' | 'Cart' | 'Quartz' | 'Gems' | 'Polish' | 'Facet';

export interface DailyMission {
  type: DailyMissionType; name: string; description: string; status: MissionStatus;
  progress: number; goal: number; reward: MissionReward;
}
export interface DailyChest { claimed: boolean; canClaim: boolean; reward: MissionReward; }   // reward = do PRÓXIMO baú
export interface DailyStreak { days: number; cycleDays: number; }                              // days = 0 se quebrou
export interface DailyState {
  day: string;                 // 'yyyy-MM-dd' (dia do jogo, vira em resetsAtUtc)
  resetsAtUtc: string;
  missions: DailyMission[];
  chest: DailyChest;
  streak: DailyStreak;
}
export interface DailyClaimResult {
  rewards: MissionReward; levelUp: boolean; player: Player; inventory: OreAmount[]; daily: DailyState;
}
```
Alterar os existentes (campos **adicionados**, nada removido):
```ts
export interface ShardsState { /* ...atual... */ daily: DailyState | null; }          // null até resgatar a Missão 1
export interface MineResult        { /* ... */ dailyMissions: DailyMission[]; }
export interface CollectCartResult { /* ... */ missions: Mission[]; dailyMissions: DailyMission[]; }
export interface UpgradeCartResult { /* ... */ missions: Mission[]; }
export interface CraftResult       { /* ... */ missions: Mission[]; dailyMissions: DailyMission[]; }
```

### 3.2 `services/shards-api.ts`
```ts
claimDaily(type: DailyMissionType): Observable<DailyClaimResult> {
  return this.http.post<DailyClaimResult>(`${this.base}/daily/${type}/claim`, null);
}
claimDailyChest(): Observable<DailyClaimResult> {
  return this.http.post<DailyClaimResult>(`${this.base}/daily/chest/claim`, null);
}
```

### 3.3 `services/shards-errors.ts`
```ts
DailyNotFound: 'Esta missão do dia não está mais disponível.',
DailyNotCompleted: 'A missão do dia ainda não foi concluída.',
DailyAlreadyClaimed: 'Recompensa do dia já resgatada.',
ChestNotReady: 'Resgate todas as missões do dia para abrir o baú.',
ChestAlreadyClaimed: 'O baú de hoje já foi resgatado.',
```
`DailyNotFound` também aparece quando o dia virou e o painel está desatualizado: tratar com um `load()` (ver F2).

**Pronto quando:** o projeto compila e nada visível mudou.

## 4. Fase F2 — Store (`services/game.store.ts`)

1. **Sinal** `readonly daily = signal<DailyState | null>(null);` e, em `fetchState()`, `this.daily.set(s.daily ?? null);`.
2. **Mesclagem das missões do tutorial** nas ações que agora trazem `missions`: em `collectCart`, `upgradeCart` e `craft`, chamar `this.mergeMissions(res.missions)` antes do `fetchState()` (os três já refazem o state, então a mesclagem serve ao **aviso de conclusão**, ver item 5).
3. **`mergeDaily(changed: DailyMission[])`**: substitui por `type` em `daily().missions`. **Diferente das missões do tutorial, uma diária `Claimed` NÃO é removida** (continua na lista até a virada, com selo "Resgatada"). Chamar em `flushPending` (mine), `collectCart` e `craft`.
4. **Resgates:**
```ts
claimDaily(type: DailyMissionType) {
  return this.act(async () => {
    const res = await this.withRetry(() => firstValueFrom(this.api.claimDaily(type)));
    this.player.set(res.player); this.inventory.set(res.inventory); this.daily.set(res.daily);
    this.notify('success', res.levelUp ? 'Recompensa do dia resgatada. Você subiu de nível!' : 'Recompensa do dia resgatada.');
    return res;
  });
}
claimDailyChest() { /* igual, com a mensagem do baú e usando api.claimDailyChest() */ }
```
   A resposta já traz o `daily` completo (use-o no lugar do local; não precisa de `fetchState`).
5. **Aviso de conclusão:** helper que, ao mesclar, detecta uma missão/diária que passou a `Completed` e chama `notify('info', 'Missão concluída: <name>')`. Como o `notice` é um slot único, em lote de mineração mostrar só o último.
6. **Virada do dia:** no `setInterval` do `start()`, se `daily()` existe e `nowMs() >= Date.parse(daily().resetsAtUtc)`, chamar `load()` **uma vez** (guardar o `day` já recarregado para não repetir). Sem isso, ações depois da virada não contam para as diárias novas, porque o servidor só cria o dia no `GET /state`.
7. **Selo da aba** (`progressBadge`): somar as diárias `Completed` (a resgatar) e `+1` se `daily().chest.canClaim`.
8. **Voltar ao foco:** o `visibilitychange` já chama `load()`; isso também cobre quem deixou a aba aberta durante a virada.

**Pronto quando:** minerar, coletar e lapidar atualizam `daily` sem recarregar; o resgate paga e atualiza o saldo.

## 5. Fase F3 — UI

### 5.1 Componente novo `pages/game/components/daily-panel.ts`
Na aba **Progresso**, acima da lista de missões. Reaproveitar as classes `sh-card`, `sh-quest`, `sh-bar`, `sh-tag`, `sh-btn`.
- Cabeçalho "Missões do dia" + **contagem regressiva** até `resetsAtUtc` (usar `store.nowMs()`; formato `hh:mm`, helper em `shared/format.ts`) + sequência (`🔥 {days}/{cycleDays}`; com `days = 0`, "Sem sequência").
- Para cada missão: nome, descrição, barra `progress/goal`, recompensa (XP + quartzo) e estado: `Active` → barra; `Completed` → botão **Resgatar**; `Claimed` → selo "Resgatada" (esmaecida).
- **Baú do dia:** card com a recompensa prevista (`chest.reward`) e o botão **Abrir baú**, habilitado só com `chest.canClaim`; depois de aberto mostrar "Baú aberto. Volte amanhã" e a prévia de amanhã. Destacar o último dia do ciclo (`days + 1 === cycleDays`): "Amanhã: baú em dobro".
- Botões desabilitados com `store.busy()`, como no `mission-list`.
- **Esconder o componente inteiro** se `store.daily()` for `null`.

### 5.2 Ajustes pequenos na lista de missões (`mission-list.ts`)
- Nenhum texto de missão no código. Só uma dica contextual para as que exigem ponto de habilidade (`CartTraining`, `OpenWorkshop`, `DoubleLuck`): se `player.skillPoints === 0` e a missão está `Active`, mostrar "Suba de nível para ganhar um ponto de habilidade".
- Barra de progresso já funciona para as de contagem (`PolishBatch` 10, `FacetBatch` 5).
- O texto de recompensa vazia ("título (em breve)") não aparece nas novas (todas têm XP), pode ficar.

### 5.3 Toasts
Reusar `store.notify`. Mensagens: missão concluída (tutorial e diária), baú aberto, sequência aumentou ("Sequência de N dias!").

## 6. Fase F4 — Verificação e documentação
- [ ] Jogador novo: sem painel de diárias até resgatar a Missão 1; depois o painel aparece com 3 missões (Login já concluída).
- [ ] Minerar, coletar o vagonete e lapidar avançam as diárias sem recarregar a página.
- [ ] Resgate individual e baú; baú só habilitado com as 3 `Claimed`; erros com texto amigável.
- [ ] Contagem regressiva correta e, ao zerar, o painel traz o novo dia (testar mudando `Daily:ResetHourUtc` no `appsettings.Development.json` do backend para a hora atual + 1).
- [ ] Sequência: abrir o baú em dois dias seguidos mostra 2/7; pular um dia mostra 0.
- [ ] As 13 missões novas aparecem na ordem, resgatáveis, sem quebrar a tela atual. Em especial `CartUpgrade`, `FirstPolish`, `FirstFacet` e `FullCart` (respostas novas com `missions`).
- [ ] `docs/CONTRATO-FRONT.md` (cópia local) atualizado com os campos novos (copiar das seções 12 e 13 de `PLANO-FRONT-OFICINA.md`).
- [ ] Testes de unidade da store: `mergeDaily` (mantém `Claimed`), `mergeMissions` (remove `Claimed`), selo da aba e refetch na virada (com relógio simulado).

## 7. Armadilhas conhecidas
1. **Dois comportamentos de `Claimed`:** missão do tutorial some da lista; diária fica até a virada. Não reaproveitar a mesma função de mesclagem sem essa diferença.
2. **Ações depois da virada sem refazer o state** não contam para as diárias novas (o servidor só cria o dia no `GET /state`). Por isso o refetch na virada (F2.6).
3. **O vagonete entregue dentro de `production` e `upgrade-cart` não conta** para as diárias `Cart`/`Quartz`/`Gems` (decisão do backend). Não esperar progresso nessas respostas.
4. **`chest.reward` é do próximo baú**, não do atual: depois de aberto, mostra o de amanhã.
5. **Fuso:** `resetsAtUtc` é UTC (03:00 UTC = meia-noite em Brasília). Formatar a hora local só para exibição.
6. **Sem balanceamento no código:** metas, recompensas e custos vêm da API (regra 1 do `CLAUDE.md` do front).
7. **Rate limit (30 req / 10 s):** o resgate das diárias é uma requisição por clique; manter os botões desabilitados com `busy()`.

## 8. Ordem sugerida e esforço
| Fase | Conteúdo | Esforço |
|---|---|---|
| F1 | Modelos, API, erros | pequeno |
| F2 | Store: `daily`, mesclagens, resgates, virada, selo | médio |
| F3 | `daily-panel` e dicas na lista de missões | médio |
| F4 | Verificação, testes de unidade, `CONTRATO-FRONT.md` | pequeno |

Entregar F1 e F2 primeiro já destrava as 13 missões novas; o painel das diárias (F3) pode sair em seguida.

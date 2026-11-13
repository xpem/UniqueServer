# Ancestral Shards — Plano do front: depósitos (oferta semanal e troca) e energia de exploração

Contrato para o front consumir a Evolução 10 do backend (ver `PLANO.md`, seção 16): a **fatia 2 das minas finitas** (`PLANO-MINAS-FINITAS.md`).
Nesta fatia o jogador **vê** as ofertas da semana e pode **trocá-las**. **Ainda não dá para atacar nem extrair** (rotas `attack` e `mine` vêm nas fatias 3 e 4), e a **energia de exploração** existe e fica cheia, sem uso.

## 1. Convenções
As mesmas de `PLANO-FRONT-OFICINA.md` (rotas em `/Shards/...`, JSON camelCase, enums como nome, erros `{ code, message }`, rate limit de 30 req / 10 s).

## 2. O que mudou e o que NÃO mudou

| Item | Situação |
|---|---|
| `GET /state` → `player` | **4 campos novos** da energia de exploração (§3). |
| `GET /state` → `deposits` | **Campo novo**: objeto com as ofertas da semana e as trocas (§4). **`null` até a oficina ser liberada** (skill `Workshop` no nível 1 ou mais). |
| `POST /Shards/deposits/{id}/rotate` | **Rota nova** (§5). |
| Erros | 3 novos (§6). |
| Todo o resto | **Inalterado**: inventário, minas, produções, oficina, missões, diárias, `ores`, `combat`. |

> `player` agora vem com os campos de exploração em **todas** as respostas que o trazem (state e ações de mina, oficina, diárias etc.). O valor já vem regenerado até o instante da resposta.

## 3. `player` — energia de exploração (segunda barra)

```json
"player": {
  "energy": 100, "maxEnergy": 100, "secondsPerEnergy": 216, "nextEnergyAtUtc": null,
  "exploreEnergy": 72, "exploreMaxEnergy": 72, "exploreSecondsPerEnergy": 1200, "nextExploreEnergyAtUtc": null
}
```

- Mesma regra da energia normal: cada ponto leva `exploreSecondsPerEnergy` segundos (**1.200 s = 20 min**; **24 h para encher os 72 pontos**, ou seja, um dia inteiro de regeneração: quem visita 1 vez por dia aproveita tudo). `nextExploreEnergyAtUtc` é `null` com a barra cheia. A barra ao vivo no cliente se calcula como a da energia normal.
- **Só a extração do depósito vai gastá-la** (fatia 4). A mineração ativa continua usando `energy`. Por enquanto a barra fica sempre cheia: pode ser exibida, mas sem ação que a gaste.
- Valores (72 e 1.200 s) vêm da configuração e podem mudar: **não os coloque no código**.

## 4. `state.deposits` — ofertas da semana

`null` enquanto a skill `Workshop` estiver no nível 0. Depois:

```json
"deposits": {
  "weekStart": "2026-10-05",
  "resetsAtUtc": "2026-10-12T03:00:00Z",
  "openCount": 0,
  "maxOpen": 3,
  "rerolls": { "used": 0, "freeLeft": 1, "nextCost": null, "canAfford": true },
  "items": [
    {
      "id": 41, "slot": 1, "tier": "Light", "element": "Fire",
      "defenseCurrent": 212, "defenseMax": 212,
      "stockRemaining": 305, "stockMax": 305,
      "status": "Sealed", "canRotate": true, "expiresAtUtc": "2026-10-12T03:00:00Z",
      "drops": [ { "oreType": "Amethyst", "chance": 1, "xp": 1 } ]
    },
    { "id": 42, "slot": 2, "tier": "Medium", "...": "..." },
    { "id": 43, "slot": 3, "tier": "Hard", "...": "..." }
  ]
}
```

**Bloco**
- `weekStart`: segunda-feira da semana do jogo. A semana vira **na segunda, 00:00 de Brasília** (`resetsAtUtc`, a mesma hora da virada das diárias). Use `resetsAtUtc` para a contagem regressiva.
- `openCount` / `maxOpen`: depósitos **já atacados** e ainda não esgotados / limite ao mesmo tempo (3). Sempre `0` nesta fatia. Na fatia 3 o ataque será recusado quando já houver `maxOpen` abertos.
- `rerolls`: trocas da semana (somando os 3 slots). `freeLeft` = trocas grátis que restam (1 por semana). `nextCost` = custo da próxima troca em minério (`{ oreType, quantity }`), **`null` enquanto ainda há troca grátis**. `canAfford` = o inventário cobre a próxima (sempre `true` se for grátis). Custo cresce: 500, 1.000, 2.000, 4.000 de quartzo... (zera na semana nova).

**Item** (`items[]`, ofertas da semana ordenadas por `slot`; depois, os já atacados de semanas anteriores, que continuam valendo)
- `id`: use na rota `rotate` (e nas rotas futuras de ataque e extração).
- `slot` / `tier`: faixa de dificuldade. `1 Light` (defesa ~200), `2 Medium` (~400), `3 Hard` (~800). A defesa exata é sorteada por jogador (±20%) e o estoque é proporcional (~2,5 por ponto de defesa).
- `element`: `Fire`, `Water` ou `Wind` (depósito nunca é `Neutral`). Use `state.combat.effectiveness[]` para mostrar vantagem e desvantagem contra as gemas do jogador.
- `defenseCurrent` / `defenseMax`: pontos de defesa a derrubar. `stockRemaining` / `stockMax`: o que há para extrair.
- `status`: `Sealed` (intocado), `Open` (já atacado, persiste até esgotar), `Depleted`, `Expired`. **Nesta fatia só aparece `Sealed`.** Ofertas intocadas de semanas passadas **não são enviadas** (já contam como expiradas).
- `canRotate`: `true` só para `Sealed`. `expiresAtUtc`: quando a oferta intocada expira (a virada da semana); `null` se já foi atacada.
- `drops`: o que a extração rende, com probabilidades normalizadas (soma 1). Hoje **100% `Amethyst`**; trate como lista (pode vir outro minério no futuro, só configuração).

**A oferta nasce sozinha:** a primeira leitura do state em cada semana cria as 3 ofertas. Não há rota para "pedir" ofertas, e leituras repetidas devolvem as mesmas.

## 5. `POST /Shards/deposits/{id}/rotate` — trocar uma oferta

Sem corpo. Troca uma oferta **intocada da semana** por outra **no mesmo slot**: o elemento sempre **muda**, e a defesa e o estoque são sorteados de novo na faixa. O `id` da oferta é o mesmo.

- A **primeira troca da semana é grátis** (vale para qualquer slot); as seguintes custam quartzo (500, 1.000, 2.000...).
- Recusa se a oferta já foi atacada, expirou, não é do jogador ou não existe.

Resposta `200`:

```json
{
  "deposit":  { "id": 41, "slot": 1, "tier": "Light", "element": "Water", "...": "..." },
  "spent":    { "oreType": "Quartz", "quantity": 500 },
  "inventory": [ { "oreType": "Quartz", "quantity": 1100 } ],
  "deposits": { "weekStart": "...", "rerolls": { "used": 2, "freeLeft": 0, "nextCost": { "oreType": "Quartz", "quantity": 1000 }, "canAfford": true }, "items": [ ... ] }
}
```

- `deposit`: a oferta nova. `spent`: o que custou (`null` se foi a grátis). `inventory`: o minério bruto atualizado (mesmo formato de `state.inventory`). `deposits`: o bloco inteiro de `state.deposits`, atualizado: **basta substituí-lo no estado do cliente**.
- Se o jogador não tiver o quartzo da próxima troca, a rota responde `400 NotEnoughResources` e nada muda. Desabilite o botão com `rerolls.canAfford`.

## 6. Erros novos (`400`, corpo `{ code, message }`)

| `code` | Quando |
|---|---|
| `DepositNotFound` | O `id` não existe ou não é do jogador. |
| `DepositNotRotatable` | O depósito já foi atacado (ou esgotado): só intocado pode ser trocado. |
| `DepositExpired` | A oferta intocada é de uma semana que já virou. |

`NotEnoughResources` (já existente) cobre a falta de quartzo; `409 ConcurrencyConflict` vale como nas outras rotas (repita a chamada).

## 7. Sugestões de tela (livres)
- Cartões lado a lado, na ordem Leve, Média e Difícil, com ícone do elemento, barra de defesa, estoque e a contagem regressiva até `resetsAtUtc`.
- Botão "Trocar" com o custo de `rerolls.nextCost` (ou "Grátis") e desabilitado com `canAfford = false` ou `canRotate = false`.
- Mostrar a vantagem elemental com `state.combat.effectiveness` e o inventário de gemas lapidadas (via `GET /inventory`) para o jogador decidir se vale abrir o depósito. O ataque chega na próxima fatia.
- Esconder a seção inteira enquanto `deposits` for `null` (oficina ainda não liberada).

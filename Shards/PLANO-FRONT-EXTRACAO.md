# Ancestral Shards — Plano do front: extrair do depósito, esgotamento, missões e ametista lapidável

Contrato para o front consumir a Evolução 12 do backend (ver `PLANO.md`, seção 18): a **fatia 4, a última das minas finitas** (`PLANO-MINAS-FINITAS.md`).
Depois de derrubar a defesa (`PLANO-FRONT-ATAQUE.md`), o jogador **extrai ametista** do depósito gastando **energia de exploração**, até o estoque acabar.
Pré-requisitos: `PLANO-FRONT-DEPOSITOS.md` e `PLANO-FRONT-ATAQUE.md`.

## 1. Convenções
As mesmas de `PLANO-FRONT-OFICINA.md` (rotas em `/Shards/...`, JSON camelCase, enums como nome, erros `{ code, message }`, rate limit de 30 req / 10 s).

## 2. O que mudou e o que NÃO mudou

| Item | Situação |
|---|---|
| `POST /Shards/deposits/{id}/mine` | **Rota nova** (§3). |
| `state.deposits.items[]` | Campo **novo** `canMine`; o `status` agora também pode ser **`Depleted`** (§4). |
| `state.deposits` | Campo **novo** `energyPerExtraction`. |
| `POST .../attack` | A resposta ganhou `missions[]` (missões que mudaram). |
| Missões | **3 novas** no fim da cadeia (§5). |
| Ametista | Agora **lapidável**: aparece em `state.workshop[].gems` e `POST /workshop/craft` aceita `"gem": "Amethyst"` (§6). |
| Erros | 1 novo: `DepositDefenseActive`. |
| Todo o resto | **Inalterado**. |

> **Atenção:** `OreType.Amethyst` agora aparece no inventário bruto (`state.inventory`, `GET /inventory`) e nas respostas de ação. Qualquer mapa de ícone por minério precisa tratá-lo. `state.workshop[].gems` passa de 3 para **4** entradas.

## 3. `POST /Shards/deposits/{id}/mine`

Corpo opcional, o mesmo da mineração ativa: `{ "times": 10 }` (sem corpo, 1 unidade). `times` vai de **1 a 100** (`InvalidTimes` fora disso).

- Só funciona com a **defesa em zero** (depósito `Open` com `canMine: true`).
- **1 ponto de energia de exploração por unidade** (`state.deposits.energyPerExtraction`). Cada unidade sorteia a tabela de drop da faixa (`items[].drops`; hoje 100% ametista) e rende **1 XP**.
- Se o `times` pedido passa do **estoque restante**, o servidor extrai **só o que resta** e cobra só essas unidades (`units` na resposta diz quantas foram).
- Se a energia de exploração não cobre as unidades extraídas, responde `NotEnoughExploreEnergy` e **nada muda** (sem extração parcial por falta de energia). Desabilite o botão com `player.exploreEnergy`.
- A habilidade `DoubleDrop` **não** vale aqui (vale só na mineração ativa).
- Ao zerar o estoque o depósito passa a **`Depleted`** e **libera uma vaga** do limite de abertos (`maxOpen`).

Resposta `200`:

```json
{
  "units": 10,
  "drops": [ { "oreType": "Amethyst", "quantity": 10 } ],
  "xpGained": 10, "levelUp": false,
  "energyCost": 10, "depleted": false,
  "deposit":   { "id": 41, "status": "Open", "stockRemaining": 90, "canMine": true, "...": "..." },
  "inventory": [ { "oreType": "Amethyst", "quantity": 10 } ],
  "player":    { "exploreEnergy": 62, "level": 1, "skillPoints": 1, "...": "..." },
  "deposits":  { "openCount": 1, "energyPerExtraction": 1, "items": [ ... ] },
  "missions":  [ { "type": "Extractor", "progress": 10, "goal": 100, "...": "..." } ]
}
```

- `drops[]`: o que rendeu, por minério (sempre trate como lista).
- `inventory`: minério **bruto** do jogador (mesmo formato de `state.inventory`); os estágios lapidados saem em `GET /inventory`.
- `deposit`, `player`, `deposits` e `missions` já vêm **atualizados**: substitua no estado do cliente. `levelUp` indica que subiu de nível (e ganhou ponto de habilidade).

## 4. Campos novos no state

```json
"deposits": { "energyPerExtraction": 1, "items": [ { "id": 41, "status": "Open", "defenseCurrent": 0, "stockRemaining": 90, "canMine": true, "canAttack": false } ] }
```

- `items[].canMine`: dá para extrair agora (defesa zero, `Open` e estoque > 0). Falso para `Sealed`, defesa parcial, `Depleted` ou expirado.
- `status: "Depleted"`: esgotado. Pode continuar aparecendo no bloco até virar a semana; mostre como "esgotado" (sem ações).
- Um depósito `Open` com `defenseCurrent: 0` e `stockRemaining > 0` é o "pronto para extração" que a fatia anterior sinalizava (`broken: true` no ataque).

## 5. Missões novas (fim da cadeia, depois de `DoubleLuck`)

| `type` | Nome | Meta | Recompensa |
|---|---|---|---|
| `FirstAttack` | Primeiro Ataque | atacar um depósito | 50 XP |
| `VaultOpened` | Cofre Aberto | derrubar a defesa de um depósito | 80 XP + 500 quartzo |
| `Extractor` | Extrator | extrair 100 unidades | 100 XP + 1.000 quartzo |

- Seguem as regras das outras missões: só ficam ativas depois de resgatar a anterior da cadeia (a cadeia é longa, então jogadores iniciantes só as verão mais tarde); as duas primeiras **já nascem concluídas** se o jogador já fez a ação.
- A resposta do **ataque real** (não a prévia) e a da **extração** trazem `missions[]` com as que mudaram. O resgate é o `POST /missions/{type}/claim` de sempre.
- A extração **não** avança as missões diárias.

## 6. Ametista lapidável

- `state.ores[]`: `Amethyst` agora tem `craftable: true`.
- `state.workshop[].gems` passa a listar `Amethyst` (4 gemas). `POST /workshop/craft` aceita `"gem": "Amethyst"` (polida: 5 brutas + quartzo; facetada: 3 polidas + quartzo, como as outras).
- A ametista lapidada (perfil de defesa 5 polida / 40 facetada, ataque fraco) é a **melhor gema defensora** para o contra-ataque dos depósitos seguintes (`PLANO-FRONT-ATAQUE.md`).
- `Collector` e `FullSet` (missões de coleção) continuam pedindo só Rubi, Safira e Esmeralda.

## 7. Erros (`400`, corpo `{ code, message }`)

| `code` | Quando |
|---|---|
| `DepositDefenseActive` | **Novo.** A defesa do depósito ainda não é zero. Ataque primeiro. |
| `NotEnoughExploreEnergy` | A energia de exploração não cobre as unidades a extrair. Nada é gasto. |
| `InvalidTimes` | `times` fora de 1 a 100. |
| `DepositDepleted` | O depósito já se esgotou. |
| `DepositExpired` / `DepositNotFound` | Oferta intocada de semana passada / id inexistente ou de outro jogador. |

`409 ConcurrencyConflict` vale como nas outras rotas (repita a chamada).

## 8. Sugestões de tela (livres)
- No cartão do depósito `Open` com `canMine`: seletor de quantidade (1 até `min(estoque, energia disponível ÷ energyPerExtraction, 100)`) e botão "Extrair", com a barra de exploração e o estoque.
- Após esgotar, mostrar o cartão como concluído (sem botões) e o saldo de ametista; sugerir levá-la à oficina para lapidar como defesa.
- Destacar a missão `Extractor` com a barra de progresso (`progress` / `goal`).

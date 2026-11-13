# Ancestral Shards — Plano do front: atacar um depósito (contra-ataque e energia de exploração)

Contrato para o front consumir a Evolução 11 do backend (ver `PLANO.md`, seção 17): a **fatia 3 das minas finitas** (`PLANO-MINAS-FINITAS.md`).
O jogador agora **ataca** um depósito com gemas lapidadas, que são **consumidas**. O depósito **contra-ataca** e o que as gemas de defesa não absorvem tira **energia de exploração**.
**Ainda não dá para extrair a ametista** (rota `mine`, fatia 4): um depósito com a defesa derrubada fica `Open` à espera. Pré-requisito: `PLANO-FRONT-DEPOSITOS.md`.

## 1. Convenções
As mesmas de `PLANO-FRONT-OFICINA.md` (rotas em `/Shards/...`, JSON camelCase, enums como nome, erros `{ code, message }`, rate limit de 30 req / 10 s).

## 2. O que mudou e o que NÃO mudou

| Item | Situação |
|---|---|
| `POST /Shards/deposits/{id}/attack` | **Rota nova** (§3), com **prévia** (`"preview": true`). |
| `state.deposits.items[]` | Dois campos **novos** por oferta: `canAttack` e `counterRatio` (§4). O `status` agora pode ser **`Open`** e `openCount` deixa de ser sempre 0. |
| `state.combat` | Campo **novo**: `energyPerCounterPoint` (§4). |
| Erros | 5 novos (§5). |
| Todo o resto | **Inalterado**: inventário, minas, oficina, missões, diárias, a rota `rotate`, `ores`. |

## 3. `POST /Shards/deposits/{id}/attack`

Corpo (`id` é o `state.deposits.items[].id`):

```json
{
  "attackers": [ { "ore": "Ruby", "stage": "Faceted", "quantity": 4 } ],
  "defenders": [ { "ore": "Sapphire", "stage": "Polished", "quantity": 5 } ],
  "preview": false
}
```

- `ore`: `Ruby`, `Sapphire`, `Emerald` ou `Amethyst`. `stage`: **`Polished` ou `Faceted`** (bruto não ataca nem defende; quartzo é recusado). Aceita maiúsculas e minúsculas.
- `attackers` é obrigatório (ao menos uma pilha). `defenders` é opcional. Até **10 pilhas distintas por lista**; pilhas repetidas são somadas.
- A **ordem das listas importa**: o servidor usa as pilhas na ordem em que vierem.
- As quantidades **listadas** precisam existir no inventário (a mesma gema nas duas listas é somada). O servidor **só gasta o que usa**, então listar mais do que o necessário é seguro.

### Como o servidor calcula (tudo determinístico, sem sorteio: a prévia é exata)
1. **Dano:** cada gema atacante causa `ataque do estágio × multiplicador` (matriz `state.combat.effectiveness`: elemento da gema contra o do depósito). As pilhas são aplicadas em ordem e o ataque **para quando a defesa chega a zero**, gastando só as gemas necessárias. O dano nunca passa da defesa que restava.
2. **Contra-ataque:** `dano efetivo × counterRatio` do depósito (leve 5%, média 10%, difícil 20%).
3. **Absorção:** as gemas defensoras, em ordem, **só até cobrir o contra-ataque**. Cada gema absorve `defesa do estágio ÷ multiplicador` (elemento do **depósito** contra o da gema): fraqueza absorve menos, resistência mais, imunidade tudo.
4. **Energia:** o que não foi absorvido custa `energyPerCounterPoint` (3) de energia de exploração por ponto, arredondado para cima.
5. A defesa derrubada **fica assim** (não regenera). O depósito `Sealed` vira `Open`.

### Prévia
Mande o mesmo corpo com `"preview": true`: **nada é gravado** e a resposta descreve o que aconteceria. Use-a antes de confirmar. Se o ataque real seria recusado por falta de gema, de energia ou de espaço, a prévia **não dá erro**: devolve `canConfirm: false` e `blocker` com o código (`NotEnoughResources`, `NotEnoughExploreEnergy` ou `TooManyOpenDeposits`).

### Resposta `200`

```json
{
  "preview": false, "canConfirm": true, "blocker": null,
  "defenseBefore": 100, "damageDealt": 100, "defenseAfter": 0, "broken": true,
  "attackersUsed": [ { "oreType": "Ruby", "stage": "Faceted", "quantity": 4 } ],
  "defendersUsed": [ { "oreType": "Sapphire", "stage": "Polished", "quantity": 1 } ],
  "counterAttack": 5, "absorbed": 5, "unabsorbed": 0,
  "energyCost": 0, "exploreEnergyAfter": 72,
  "deposit":   { "id": 41, "status": "Open", "defenseCurrent": 0, "canAttack": false, "...": "..." },
  "inventory": { "items": [ { "oreType": "Ruby", "stage": "Faceted", "quantity": 6 } ] },
  "player":    { "exploreEnergy": 72, "...": "..." },
  "deposits":  { "openCount": 1, "...": "..." }
}
```

- `attackersUsed` / `defendersUsed`: o que **foi gasto** (ou seria, na prévia). Pode ser menos que o listado.
- `broken`: a defesa chegou a zero (pronto para a extração, fatia 4). `counterAttack`, `absorbed` e `unabsorbed` são pontos (com até 2 casas decimais).
- `energyCost` / `exploreEnergyAfter`: energia de exploração perdida e a que sobra (na prévia, a que sobraria, nunca negativa).
- `deposit`, `inventory` (**por estágio**, como na oficina), `player` e `deposits` já vêm **atualizados**: basta substituí-los no estado do cliente.

## 4. Campos novos no state

```json
"combat": { "effectiveness": [ ... ], "energyPerCounterPoint": 3 },
"deposits": { "openCount": 0, "maxOpen": 3, "items": [ { "id": 41, "status": "Sealed", "canAttack": true, "counterRatio": 0.05, "...": "..." } ] }
```

- `items[].canAttack`: dá para atacar agora. É falso quando a defesa já é zero, o depósito expirou ou se esgotou, ou quando é intocado e **já há `maxOpen` depósitos abertos** (abrir mais um passaria do limite). Um depósito já `Open` continua atacável mesmo no limite.
- `items[].counterRatio`: contra-ataque por ponto de dano (0,05 = 5%). Com ele e `energyPerCounterPoint` o front estima o custo sem chamar a prévia, mas **a prévia é a fonte da verdade**.
- Depósitos `Open` de semanas anteriores continuam vindo em `items[]` (persistem até se esgotar); intocados de semanas passadas não vêm.

## 5. Erros (`400`, corpo `{ code, message }`)

| `code` | Quando |
|---|---|
| `InvalidGem` / `InvalidStage` | Minério inválido ou quartzo / estágio inválido ou `Raw`. |
| `InvalidAttack` | Sem atacantes, quantidade menor que 1, pilhas demais, ou as gemas **não causam dano** (imune). Nada é gasto. |
| `NotEnoughResources` | Alguma pilha listada passa do que o jogador tem (somando as duas listas). |
| `NotEnoughExploreEnergy` | O contra-ataque não absorvido custaria mais energia de exploração do que o jogador tem. **Nada é gasto**: leve gemas de defesa ou espere regenerar. |
| `TooManyOpenDeposits` | Abrir um depósito intocado com `maxOpen` já abertos. |
| `DepositDefenseBroken` | A defesa do depósito já é zero. |
| `DepositDepleted` | O depósito já se esgotou. |
| `DepositExpired` / `DepositNotFound` | Oferta de semana passada / id inexistente ou de outro jogador (já existem desde a fatia 2). |

`409 ConcurrencyConflict` vale como nas outras rotas (repita a chamada).

## 6. Sugestões de tela (livres)
- Tela de ataque por depósito: seletor de gemas atacantes e defensoras (do `GET /inventory`, só `Polished` e `Faceted`), com a **prévia chamada a cada mudança** (com debounce) mostrando dano, defesa restante, gemas que serão gastas, contra-ataque, energia perdida e a barra de exploração depois.
- Botão "Atacar" habilitado só com `canConfirm: true`; mostrar o `blocker` traduzido quando falso.
- Mostrar a vantagem elemental (`combat.effectiveness`) ao escolher as gemas: Água vence Fogo, Fogo vence Vento, Vento vence Água.
- Dica de balanceamento para o texto da tela: na faixa leve o contra-ataque cabe folgado na barra de exploração (~0,4 barra de 72; pode nem valer defender); nas faixas média e difícil, levar gemas de defesa (Safira e Ametista são as melhores) costuma sair mais barato que perder energia.
- Depósito `Open` com `broken: true` mostra "pronto para extração" (a ação de extrair chega na próxima fatia).

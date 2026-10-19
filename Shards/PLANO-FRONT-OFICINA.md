# Ancestral Shards — Plano do front: inventário por estágio e Oficina de Lapidação

Contrato e roteiro para o front consumir as Evoluções 4 e 5 do backend (ver `PLANO.md`, seções 10 e 11). Nada aqui quebra o que o front já usa: só há campos e rotas **novos**.

## 1. Convenções (valem para tudo)
- Rotas em `/Shards/...` (em produção, prefixo `/api`). Todas exigem `Authorization: Bearer <token>`.
- JSON em **camelCase**; enums viajam como **nome** (`"Ruby"`, `"Polished"`), nunca número.
- Erro de regra: HTTP 400 com `{ "code": "NotEnoughResources", "message": "..." }`. Conflito de concorrência: 409 `ConcurrencyConflict` (repetir a ação).
- Rate limit do Shards: 30 req / 10 s por jogador.
- Enums: `OreType` = `Quartz | Ruby | Sapphire | Emerald`; `ItemStage` = `Raw | Polished | Faceted` (bruto, polido, facetado); `SkillType` ganhou `Workshop`.

## 2. O que mudou e o que NÃO mudou

| Item | Situação |
|---|---|
| `state.inventory`, `inventory` das respostas de `mine`/`collect-cart`/`unlock`/`production`/`upgrade-cart`/`claim` | **Inalterados**: `{ oreType, quantity }[]`, só o estágio **Raw**. Continua sendo o "saldo para pagar custos". |
| `state.skills` | Agora lista **4** skills (nova: `Workshop`, `maxLevel` 2). Quem renderiza a lista por tipo conhecido precisa tratar o novo. |
| `state.workshop` | **Novo** (receitas, ver §4). |
| `GET /inventory` | **Novo** (inventário completo por estágio, ver §3). |
| `POST /workshop/craft` | **Novo** (ver §5). |
| Erros | 3 novos: `InvalidGem`, `InvalidStage`, `WorkshopLocked` (ver §6). |

## 3. `GET /Shards/inventory`
Inventário completo, **todos os estágios**. Só itens com `quantity > 0`, ordenados por minério (Quartz, Ruby, Sapphire, Emerald) e depois por estágio (Raw, Polished, Faceted). Jogador novo recebe `{ "items": [] }` (o jogador só é criado em `GET /state`).

```json
{
  "items": [
    { "oreType": "Quartz",   "stage": "Raw",      "quantity": 1240 },
    { "oreType": "Ruby",     "stage": "Raw",      "quantity": 12 },
    { "oreType": "Ruby",     "stage": "Polished", "quantity": 3 },
    { "oreType": "Emerald",  "stage": "Faceted",  "quantity": 1 }
  ]
}
```
Uso: tela de inventário/oficina. Chamar ao abrir a tela e depois de cada `craft` (a resposta do craft já traz o inventário completo, então não precisa rebuscar).

## 4. `state.workshop`
Uma entrada por receita, na ordem de nível. Cada receita produz **1 gema** do estágio `stage` a partir de `inputQuantity` gemas do estágio anterior + o `cost` em minério **bruto**.

```json
"workshop": [
  {
    "stage": "Polished", "level": 1, "inputQuantity": 5,
    "cost": [ { "oreType": "Quartz", "quantity": 100 } ],
    "skillAllows": false,
    "gems": [
      { "oreType": "Ruby",     "available": 12, "canAfford": true  },
      { "oreType": "Sapphire", "available": 0,  "canAfford": false },
      { "oreType": "Emerald",  "available": 2,  "canAfford": false }
    ]
  },
  { "stage": "Faceted", "level": 2, "inputQuantity": 3, "cost": [ { "oreType": "Quartz", "quantity": 400 } ], "skillAllows": false, "gems": [ ... ] }
]
```
- `skillAllows`: a skill `Workshop` já está no nível `level`. Se `false`, o craft dá `WorkshopLocked`.
- `gems[].available`: quantas gemas do **estágio consumido** o jogador tem (para a receita Polished, brutas; para Faceted, polidas).
- `gems[].canAfford`: tem gemas **e** quartzo para **1** lapidação (não considera a skill). Para `times = N`, o front calcula `available >= inputQuantity * N` e `quartzo >= cost * N`.
- Quartzo não aparece em `gems` (não é lapidável).

Valores atuais (configuráveis no servidor, **não fixar no front**, sempre ler de `state.workshop`): Polished = 5 brutas + 100 Quartz; Faceted = 3 polidas + 400 Quartz.

## 5. `POST /Shards/workshop/craft`
Body (JSON):
```json
{ "gem": "Ruby", "stage": "Polished", "times": 2 }
```
- `gem`: `Ruby | Sapphire | Emerald` (Quartz é inválido).
- `stage`: estágio **a produzir**: `Polished | Faceted` (Raw é inválido).
- `times`: opcional, 1 a 100, padrão 1 (produz várias de uma vez).
- Instantâneo e **sem risco**: se der certo, sempre produz `times` gemas.

Resposta 200:
```json
{
  "crafted": { "oreType": "Ruby", "stage": "Polished", "quantity": 2 },
  "spent": [
    { "oreType": "Ruby",   "stage": "Raw", "quantity": 10 },
    { "oreType": "Quartz", "stage": "Raw", "quantity": 200 }
  ],
  "inventory": { "items": [ ... inventário completo, como em §3 ... ] }
}
```
Depois do craft, o front deve atualizar: o inventário (vem na resposta) e o **saldo de quartzo/`state.inventory`** (o craft gastou quartzo Raw; refazer `GET /state` ou subtrair `spent`). Também atualizar `state.workshop` (`available`/`canAfford`).

## 6. Erros do craft (todos HTTP 400, nada é gravado)
| `code` | Quando | Sugestão de UI |
|---|---|---|
| `InvalidGem` | `gem` ausente, desconhecida ou Quartz | Não deve ocorrer com UI correta; mostrar mensagem genérica |
| `InvalidStage` | `stage` ausente, desconhecido ou Raw | Idem |
| `InvalidTimes` | `times` fora de 1–100 | Limitar o input |
| `WorkshopLocked` | Skill `Workshop` abaixo do nível da receita | Mostrar "libere com a skill Workshop (nível N)" e atalho para distribuir ponto |
| `NotEnoughResources` | Faltam gemas ou quartzo | Desabilitar o botão quando `canAfford` for falso (o servidor é a palavra final) |
| `PlayerNotFound` | Jogador ainda não criado | Chamar `GET /state` primeiro |
| `ConcurrencyConflict` (409) | Duas ações ao mesmo tempo | Repetir uma vez |

Ordem de validação no servidor: entradas inválidas → `InvalidTimes` → jogador → skill → recursos. Sem permissão, **nada é cobrado**.

## 7. Skill `Workshop`
- Distribuída como as outras: `POST /Shards/skills/distribute` com `{ "skillType": "Workshop" }` (1 ponto de habilidade por nível).
- `maxLevel` = 2: nível 1 libera **Polished**; nível 2 libera **Faceted**. Em `state.skills` aparece como `{ type: "Workshop", level, maxLevel }`.
- Compete por pontos com DoubleDrop, MineSlots e CartCapacity: vale destacar na tela de skills o que cada nível desbloqueia.

## 8. Telas e tarefas sugeridas
1. **Skills**: aceitar `Workshop` (nome, ícone, descrição dos níveis: "1 = polir, 2 = facetar").
2. **Inventário** (nova tela ou aba): `GET /inventory`, agrupado por gema com colunas Bruto / Polido / Facetado; quartzo em destaque como moeda (vem de `state.inventory`, estágio Raw). Ocultar linhas com zero (o servidor já omite).
3. **Oficina**: lista `state.workshop`; para cada receita e gema mostrar entrada (`inputQuantity`× estágio anterior) → saída (1× estágio novo), custo em quartzo, `available`, estado travado (`skillAllows`) e botão **Lapidar**; seletor de quantidade (`times`) limitado ao que o jogador consegue pagar (e a 100).
4. **Feedback do craft**: toast com `crafted`; atualizar inventário, quartzo e receitas. Erros por `code` (§6).
5. **Estado de transição**: bloquear o botão durante a requisição (evita duplo clique; cada clique já é uma ação).
6. **Compatibilidade**: manter o consumo de `state.inventory` como está; **não** usar `GET /inventory` para checar custos de mina/vagonete/produção (esses usam só o estágio Raw, e `canAfford` dos outros campos do state já vem calculado).

## 9. Critérios de aceite do front
- [ ] Skill `Workshop` aparece e pode receber ponto até o nível 2.
- [ ] Tela de inventário mostra gemas por estágio e o quartzo; vazio mostra estado vazio.
- [ ] Oficina lista as receitas a partir de `state.workshop` (sem números fixos no código).
- [ ] Botão Lapidar fica desabilitado quando `skillAllows` ou `canAfford` forem falsos, e mostra o motivo.
- [ ] Craft com `times > 1` funciona e o máximo respeita o saldo.
- [ ] Todos os códigos de erro do §6 têm mensagem amigável.
- [ ] Depois do craft, inventário, quartzo e receitas atualizam sem recarregar a página.
- [ ] Nenhuma tela existente quebra com os campos novos (`workshop`, 4ª skill).

## 10. Para testar manualmente
`Shards/Shards.http`, seções 1.2 (`GET /inventory`) e 5D (oficina). Para ter gemas sem esperar horas, ver as dicas no fim do arquivo (acelerar energia/vagonete e baixar o custo de produção) e dar pontos de skill/gemas direto no banco de desenvolvimento.

## 11. Fora do escopo (não implementar ainda)
Elemento e ataque das gemas, combate, mercado/venda, limite de inventário, fila de lapidação com tempo, chance de falha.

---

## 12. Atualização — novas missões (cadeia de 4 para 17)

Nenhum endpoint novo. O que muda para o front:

- **`MissionType`** ganha 13 valores (strings, nesta ordem da cadeia): `CartTraining`, `CartUpgrade`, `OpenWorkshop`, `FirstPolish`, `FirstFacet`, `Diversification`, `Collector`, `PolishBatch`, `FullSet`, `FacetBatch`, `ThirdMine`, `FullCart`, `DoubleLuck`. Nome, descrição, `goal`, `progress` e recompensa já chegam em `state.missions` e nas respostas: **nenhum texto de missão fica no front**.
- **`missions` em mais respostas.** `POST /mines/{n}/collect-cart`, `POST /mines/{n}/upgrade-cart` e `POST /workshop/craft` agora devolvem `missions` (as que mudaram), como `mine`, `unlock`, `production`, `distribute` e `claim`. O front deve mesclar por `type` em **todas** elas.
- **Missões de contagem** (`PolishBatch` meta 10, `FacetBatch` meta 5): mostrar barra `progress/goal`; o `craft` com `times > 1` avança vários de uma vez. As de condição têm meta 1.
- **Recompensas**: só XP e Quartz (bruto); `claim` segue igual (`POST /missions/{type}/claim`).
- **Missão nascida concluída**: se o jogador já fez a ação antes, a missão aparece direto como `Completed` (pronta para resgatar). Tratar o estado `Completed` ao receber missão nova.
- **Cadeia longa**: `state.missions` continua trazendo só as visíveis (ativas ou concluídas não resgatadas), normalmente 1 por vez. Quem já resgatou a `FirstGem` ganha a `CartTraining` no próximo `GET /state`.
- **Dependência de pontos de habilidade**: `CartTraining`, `OpenWorkshop` e `DoubleLuck` pedem pontos; sem pontos o jogador precisa subir de nível. Vale um aviso/atalho nessas missões.

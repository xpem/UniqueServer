# Ancestral Shards — Plano do front: catálogo de minérios, ametista e regras de combate

Contrato para o front consumir as Evoluções 8 e 9 do backend (ver `PLANO.md`, seções 14 e 15). É a base das minas finitas com defesa (`PLANO-MINAS-FINITAS.md`): por enquanto, só **dados**.
**Nenhum comportamento do jogo mudou.** Só há campos novos no `GET /state` e um valor novo no enum de minérios. Os depósitos, o ataque, o contra-ataque e a energia de exploração vêm nas próximas fatias.

## 1. Convenções
As mesmas de `PLANO-FRONT-OFICINA.md` (rotas em `/Shards/...`, JSON camelCase, enums como nome, erros `{ code, message }`, rate limit de 30 req / 10 s).

## 2. O que mudou e o que NÃO mudou

| Item | Situação |
|---|---|
| `OreType` | Ganhou **`Amethyst`**: `Quartz \| Ruby \| Sapphire \| Emerald \| Amethyst`. A ametista **ainda não pode ser obtida** (só sairá dos depósitos): nunca aparece no inventário nem nas respostas de ação por enquanto. |
| `GET /state` | Dois campos **novos**: `ores[]` (catálogo de minérios, com ataque e defesa) e `combat` (matriz de efetividade entre elementos). |
| Enums novos | `OreKind` = `Currency \| Gem \| Exclusive`; `Element` = `Neutral \| Fire \| Water \| Wind` (**só o quartzo é `Neutral`**). |
| `state.workshop[].gems` | **Inalterado**: continua só com Ruby, Sapphire e Emerald. A ametista não é lapidável ainda (`craftable: false`). |
| `state.inventory`, `GET /inventory`, `productions`, missões, diárias | **Inalterados**. |
| `POST /workshop/craft` com `"gem": "Amethyst"` | `400 InvalidGem`, como Quartz. |
| Erros | Nenhum novo. |

> **Atenção para quem renderiza por tipo de minério:** o valor `Amethyst` aparece em `state.ores[]`. Qualquer `switch`/mapa de ícone por `OreType` precisa ter um caso padrão (ou o novo caso), para não quebrar.
>
> **Atenção para quem já leu `state.combat` numa versão anterior:** o formato mudou. `elementCycle`, `advantage` e `disadvantage` **foram removidos** e `effectiveness[]` entrou no lugar (ver §4).

## 3. `state.ores[]` — catálogo de minérios
Uma entrada por minério, na ordem da configuração (Quartz, Ruby, Sapphire, Emerald, Amethyst). Vem da configuração do servidor: **não coloque esses valores no código**.

```json
"ores": [
  { "oreType": "Quartz",   "kind": "Currency",  "element": "Neutral", "craftable": false, "attack": [], "defense": [] },
  { "oreType": "Ruby",     "kind": "Gem",       "element": "Fire",    "craftable": true,
    "attack":  [ { "stage": "Polished", "attack": 4 },  { "stage": "Faceted", "attack": 32 } ],
    "defense": [ { "stage": "Polished", "defense": 2 }, { "stage": "Faceted", "defense": 16 } ] },
  { "oreType": "Sapphire", "kind": "Gem",       "element": "Water",   "craftable": true,  "attack": [ ... ], "defense": [ ... ] },
  { "oreType": "Emerald",  "kind": "Gem",       "element": "Wind",    "craftable": true,  "attack": [ ... ], "defense": [ ... ] },
  { "oreType": "Amethyst", "kind": "Exclusive", "element": "Wind",    "craftable": false,
    "attack":  [ { "stage": "Polished", "attack": 1 },  { "stage": "Faceted", "attack": 8 } ],
    "defense": [ { "stage": "Polished", "defense": 5 }, { "stage": "Faceted", "defense": 40 } ] }
]
```

- `kind`: `Currency` é o quartzo (moeda: sem elemento, ataque nem defesa); `Gem` são as gemas de mina comum; `Exclusive` é a ametista (só dos depósitos).
- `element`: elemento do minério. **Todo minério tem elemento, menos o quartzo.** A ametista é `Wind` **provisoriamente**.
- `craftable`: a oficina aceita lapidar este minério.
- `attack` e `defense`: pontos por **estágio lapidado**. A mesma gema serve para atacar ou para defender (no futuro, o jogador divide o que leva entre as duas coisas). Gema **bruta não ataca nem defende** (não há entrada `Raw`).
- **Perfis atuais (polida; a facetada multiplica por 8):** Rubi ataca (4 de ataque e 2 de defesa), Safira defende (2 e 4), Esmeralda é equilibrada (3 e 3), Ametista é defensiva (1 e 5). Valores **iniciais**, sujeitos a ajuste.

## 4. `state.combat` — matriz de efetividade

```json
"combat": {
  "effectiveness": [
    { "attacker": "Fire",  "defender": "Wind",  "multiplier": 1.5 },
    { "attacker": "Wind",  "defender": "Water", "multiplier": 1.5 },
    { "attacker": "Water", "defender": "Fire",  "multiplier": 1.5 },
    { "attacker": "Wind",  "defender": "Fire",  "multiplier": 0.75 },
    { "attacker": "Water", "defender": "Wind",  "multiplier": 0.75 },
    { "attacker": "Fire",  "defender": "Water", "multiplier": 0.75 }
  ]
}
```

Cada par diz o **multiplicador do ataque** do elemento `attacker` contra a defesa do elemento `defender`: `1.5` é vantagem, `0.75` é desvantagem, `0` seria imune. **Par que não aparece vale 1** (inclusive elementos iguais). Hoje o resultado é um pedra-papel-tesoura: Fogo vence Vento, Vento vence Água, Água vence Fogo. A matriz cresce por configuração quando entrarem elementos novos, então **não presuma 3 elementos nem um ciclo fechado**: leia o que vier.

```ts
function elementMultiplier(c: Combat, attacker: Element, defender: Element): number {
  return c.effectiveness.find(e => e.attacker === attacker && e.defender === defender)?.multiplier ?? 1;
}
```

Use para tooltips e para desenhar a roda de vantagens (a partir dos pares com `multiplier > 1`). O servidor é quem calcula o dano de verdade (nas próximas fatias); a função do front é **só informativa**.

## 5. Interfaces TypeScript

```ts
export type OreType = 'Quartz' | 'Ruby' | 'Sapphire' | 'Emerald' | 'Amethyst';
export type OreKind = 'Currency' | 'Gem' | 'Exclusive';
export type Element = 'Neutral' | 'Fire' | 'Water' | 'Wind';

export interface OreAttack  { stage: ItemStage; attack: number; }
export interface OreDefense { stage: ItemStage; defense: number; }
export interface OreInfo {
  oreType: OreType; kind: OreKind; element: Element; craftable: boolean;
  attack: OreAttack[]; defense: OreDefense[];
}

export interface Effectiveness { attacker: Element; defender: Element; multiplier: number; }
export interface Combat { effectiveness: Effectiveness[]; }

// ShardsState ganha:
//   ores: OreInfo[];
//   combat: Combat;
```

## 6. O que o front pode fazer agora (opcional, sem depender de nada novo)
- Mostrar elemento, ataque e defesa nas gemas polidas e facetadas do inventário/oficina (ícone do elemento; ataque e defesa no tooltip).
- Uma tela de "guia de elementos" com a roda de vantagens, lendo `state.combat.effectiveness`.
- Preparar o tratamento do novo `OreType` (`Amethyst`).

## 7. O que vem depois (não implementar ainda)
Depósitos semanais com defesa, ataque e contra-ataque com gemas, energia de exploração e extração de ametista (`PLANO-MINAS-FINITAS.md`). Cada fatia vira um contrato próprio.

# Ancestral Shards — Plano do Mercado (EM ESPERA)

> **Status: planejado e adiado.** O mercado cria uma fonte forte de quartzo e pode quebrar o equilíbrio das mecânicas atuais
> (minas, produção de gemas, oficina, missões). Só implementar depois de cumprir as **condições para retomar** (seção 7).
> Nada deste documento está implementado.

## 1. Ideia

Um **mercado NPC**: o jogo compra itens do jogador por **quartzo**, a **preço fixo**. Só **vende** (o jogador não compra gema
com quartzo, porque isso pularia as minas). O quartzo **não** é vendável: ele é a moeda.

## 2. Preço próprio por item

Um preço por **(gema, estágio)**: 3 gemas (Safira, Rubi, Esmeralda) × 3 estágios (bruta, polida, facetada) = **9 preços**, na
configuração (`Shards:Balance:Market`), como o resto do balanceamento. O quartzo não entra na tabela.

## 3. Como calcular os preços (raciocínio)

Valores de base, extraídos da configuração atual:

- Uma mina rende ~475 minérios por dia (0,33/min). Em Quartzo: 475 quartzos. Em gema (90% gema + 10% quartzo): ~427 gemas + ~47 quartzos.
- Trocar a produção de uma mina custa: Safira 500, Rubi 1.200, Esmeralda 3.000 de quartzo.
- Receitas da oficina: Polida = 5 brutas + 100 quartzos; Facetada = 3 polidas + 400 quartzos (1 facetada = 15 brutas + 700 quartzos).

Dois critérios:

1. **A mina de gema precisa compensar a troca.** Para a troca se pagar em ~1 a 2 dias, a gema bruta tem que valer mais de 1 quartzo.
2. **Cada estágio precisa valer mais que o custo de produzi-lo**, senão a oficina vira prejuízo e ninguém a usa
   (preço do estágio ≈ (insumos + quartzo) × margem de ~20–25%).

### Tabela de partida (em quartzo; **valores para simular, não para fixar**)

| Gema | Bruta | Polida | Facetada |
|---|---|---|---|
| Safira | 2 | 140 | 1.000 |
| Rubi | 3 | 150 | 1.050 |
| Esmeralda | 5 | 170 | 1.100 |

Com esses valores: uma mina de Safira rende ~900 por dia (≈ 2× a de quartzo) e a troca se paga em ~1,2 dia; Rubi ~1.330 (1,4 dia);
Esmeralda ~2.180 (1,8 dia). Lapidar dá ~27% de lucro no polimento e ~22% na faceta.

## 4. Desenho técnico (quando for implementar)

- **Rota:** `POST /Shards/market/sell` com `{ "oreType": "Sapphire", "stage": "Polished", "quantity": 10 }`.
  Resposta: `sold`, `earned` (quartzo recebido) e `inventory` completo (por estágio), com `missions`/`dailyMissions` que mudaram.
- **`GET /state`:** ganha `market[]` (`oreType`, `stage`, `price`, `available`).
- **Configuração:** `Market: [{ Ore, Stage, Price }]`. O validador exige preço > 0, sem Quartz, sem Stage repetido por gema e um
  preço por (gema, estágio) das 3 gemas.
- **Erros novos:** item não vendável (Quartz) e quantidade inválida. Faltando item, reaproveita `NotEnoughResources`;
  gema/estágio inválidos reaproveitam `InvalidGem`/`InvalidStage`.
- **Transação única com `ConcurrencyRetry`**, como os outros comandos; o quartzo recebido entra na linha **Raw**.
- **Missões:** as que olham a existência da linha do inventário (`Collector`, `FullSet`, `FirstPolish`, `FirstFacet`) **não desfazem**
  ao vender, por desenho (a linha não some). Pode-se criar uma missão de primeira venda depois.
- **Venda de "tudo":** atalho com `quantity` vazio, além da quantidade exata (a decidir).
- **Testes:** preço × quantidade, estágio, quartzo não vendável, falta de item sem gravar nada, vender zera/some da lista,
  validador, e a conta de ida e volta (minerar → lapidar → vender) para garantir que não há ciclo que gere quartzo sem limite de tempo.

## 5. Riscos conhecidos

1. **A identidade da gema some nos estágios altos.** A oficina cobra o mesmo quartzo por qualquer gema, e esse quartzo domina o preço
   (facetada em 1.000 / 1.050 / 1.100). Se a Esmeralda facetada deve valer bem mais, o **custo em quartzo da oficina precisa variar por gema**
   (mudança pequena de configuração, mas decisão de design).
2. **Inflação de quartzo.** Cinco minas de Esmeralda chegariam a ~10 mil quartzos por dia. Os ralos atuais (≈100 mil para ampliar os
   vagonetes, trocas de produção, oficina) aguentam, mas não sem uma simulação antes de fixar valores.
3. **Sem limite de venda, nada freia um excesso de oferta.** A primeira versão pode ir sem limite, mas a configuração deve nascer
   pronta para aceitar um teto diário ou preço decrescente depois.

## 6. Decisões em aberto

1. Só vender, preço fixo, sem comprar (recomendado)?
2. Preços: a tabela de partida, com custo da oficina diferente por gema, ou manter o custo igual?
3. Limite de venda: nenhum agora (recomendado, com a configuração preparada) ou um teto diário?
4. Venda de "tudo" de um item além da quantidade exata?

## 7. Condições para retomar

1. **Simulação de economia** (quartzo por dia, por estratégia e número de minas) para ajustar os preços com números, não palpite.
2. **Decidir a fonte e o ralo de quartzo no longo prazo** (outras formas de obter quartzo, e o que consome quartzo depois do vagonete e da oficina).
3. **Resolver o risco 1** (custo da oficina por gema) antes de fixar os preços das gemas lapidadas.
4. O front já consumindo a oficina e as missões, para o mercado não chegar antes da tela que o usa.

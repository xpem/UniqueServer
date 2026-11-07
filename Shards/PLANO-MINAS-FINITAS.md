# Ancestral Shards — Plano das Minas Finitas com Defesa (PLANEJADO, NÃO IMPLEMENTADO)

> Status: **em planejamento**. Nada deste documento está no código. As decisões marcadas **(a confirmar)** são recomendações minhas
> que o dono do projeto ainda não aprovou. O mercado (`PLANO-MERCADO.md`) continua em espera.

## 1. Ideia

O jogador recebe **minas finitas** (depósitos), cada uma com um **elemento**, uma **defesa** (pontos) e um **estoque limitado** de um minério
**exclusivo** (ametista). Para minerar, ele precisa **reduzir a defesa a zero atacando com gemas polidas ou facetadas**, que são
**consumidas** ao atacar. Cada gema lapidada tem **ataque**, **defesa** e um **elemento**; uma **matriz de efetividade** (estilo pedra-papel-tesoura) diz quem tem vantagem sobre quem. A mina **contra-ataca**, e o jogador divide as gemas entre atacar e defender.
O jogador decide, depósito a depósito, **se vale a pena** gastar gemas para abri-lo.

Isso dá propósito às gemas lapidadas (hoje só se acumulam), cria um ralo de gemas e uma decisão com os elementos. Também é o ponto de partida
do combate do jogo, sem multiplayer: cada depósito pertence a um jogador, então **não há disputa de registros**.

## 2. Decisões já tomadas

| # | Decisão |
|---|---|
| 1 | A gema é **consumida** ao atacar (vira ralo; sem durabilidade nem recarga) |
| 2 | O minério do depósito é **exclusivo**: **ametista** (nova, só sai daqui) |
| 3 | As minas são oferecidas em **rotação semanal**, **mais de uma por vez**, com **defesa aleatória**; o jogador pode **rotacionar** (trocar) uma que não queira |
| 4 | **Defesa da gema** fica para depois; começamos só com **ataque e elemento** |
| 5 | A defesa de um depósito é, por ora, **(elemento, pontos)**. No futuro, a mina do próprio jogador também poderá ter defesa feita de um **estoque de minérios defensivos** (ideia a amadurecer; sem pressa) |
| 6 | Elementos numa **matriz de efetividade** (atacante × defensor → multiplicador), na configuração. Hoje 3 elementos: Fogo vence Vento, Vento vence Água, Água vence Fogo (×1,5 e ×0,75). **Um elemento novo entra junto do minério que o introduz.** **Só o quartzo é neutro** |
| 7 | No revide (evolução futura), a gema tem uma **chance de ser gasta** (modelo C, seção 9), com prévia da chance e da perda esperada |
| 8 | **Liberação** dos depósitos: skill `Workshop` no nível 1 ou mais |
| 9 | **3 ofertas por semana**, de dificuldades diferentes (leve, média e difícil) |
| 10 | A semana começa na **segunda-feira, na hora de virada das diárias** (00:00 de Brasília) |
| 11 | A **extração** do depósito usa uma **energia de exploração própria** (segunda barra); o **ataque** só gasta gemas |
| 12 | Tamanho inicial da barra de exploração: **máximo 30, 1 ponto a cada 20 minutos** (~10 h para encher), a ajustar na simulação |
| 13 | **Contra-ataque leve já na faixa fácil**, para manter a mecânica; cresce nas faixas seguintes |
| 14 | A **mesma gema** serve para **atacar ou defender**; o jogador divide o que leva. **Perfis** (polida; a facetada multiplica por 8): Rubi 4 de ataque e 2 de defesa, Safira 2 e 4, Esmeralda 3 e 3, Ametista 1 e 5 |
| 15 | A **ametista** é Vento (**provisório**) e o minério **defensivo** (1 de ataque, 5 de defesa) |
| 16 | **Dano na energia (aprovado):** o contra-ataque não absorvido pelas gemas de defesa tira **energia de exploração** (conversão sugerida ~3 pontos de energia por ponto de contra-ataque); o ataque é **recusado** se faltar energia, com **prévia** antes de confirmar; as gemas de defesa só são gastas na medida do que absorvem, as de ataque sempre |
| 17 | A primeira mina de ametista é segura pelo contra-ataque leve da faixa fácil e pelas gemas de defesa que o jogador já tem; a ametista ajuda nas minas seguintes |

## 3. Recomendações a confirmar

1. **Como minerar depois de quebrar a defesa: com uma energia PRÓPRIA de exploração (uma segunda barra), na primeira versão.** A mineração ativa continua usando
   a energia normal; o depósito usa a energia de exploração, que regenera à parte. Assim o depósito **soma** ao jogo em vez de competir com a mineração normal
   (cada mineração normal rende 1 quartzo/gema, então com energia compartilhada o jogador teria que escolher entre ela e o depósito, e um dos dois seria ignorado).
   - **Só a extração gasta energia de exploração.** O **ataque** já custa gemas (e, no futuro, a chance de perdê-las); cobrar energia também seria um terceiro custo.
   - **Tamanho e ritmo na configuração** (`Shards:Balance:Explore`: máximo e segundos por ponto), pequenos no início, porque cada ponto rende minério exclusivo;
     ajustar na simulação junto do estoque dos depósitos.
   - Sem espaço de mina, sem vagonete e sem produção automática. Uma versão futura pode deixar o depósito ocupar um espaço de mina com vagonete.
2. **A defesa quebrada fica assim** até o estoque acabar (não regenera); senão o jogador perderia o investimento.
3. **Expiração:** um depósito **intocado** (defesa intacta) expira ao fim da semana. Um depósito **já atacado** continua até se esgotar.
   Assim, o investimento nunca é perdido, e a rotação não vira armadilha.
4. **Rotacionar limitado** (ex.: 1 troca grátis por semana, extras pagas em quartzo, com custo crescente). Sem limite, o jogador trocaria até achar
   um elemento que suas gemas vencem, e a decisão some ("fishing").
5. **A ametista é o minério defensivo** (ataque 1, defesa 5; facetada ×8), de elemento Vento (provisório). Serve de defesa nos depósitos seguintes e é a candidata natural ao estoque defensivo futuro das minas do jogador. Fica **lapidável** quando for possível obtê-la (`Craftable`, só configuração).
6. **Sem desperdício de gemas:** o ataque aplica as gemas na ordem em que o jogador as lista e **para quando a defesa chega a zero**, consumindo só as necessárias.

## 4. Modelo (proposto)

- **`OreType.Amethyst = 7`** (valor novo; 2 e 3 seguem sem uso).
- **Atributos de gema (configuração, `Shards:Balance:Ores`):** elemento, ataque e defesa por estágio. Perfis atuais (polida; a facetada multiplica por 8):

| Gema | Elemento | Ataque polida | Defesa polida | Ataque facetada | Defesa facetada | Papel |
|---|---|---|---|---|---|---|
| Rubi | Fogo | 4 | 2 | 32 | 16 | Ofensiva |
| Safira | Água | 2 | 4 | 16 | 32 | Protetiva |
| Esmeralda | Vento | 3 | 3 | 24 | 24 | Equilibrada |
| Ametista | Vento (provisório) | 1 | 5 | 8 | 40 | Defensiva (exclusiva) |

  Todas somam o mesmo total (6 na polida). Gema **bruta não ataca nem defende**; o quartzo (moeda) não tem elemento, ataque nem defesa. **Só o quartzo é neutro.**
- **Matriz de efetividade (`Shards:Balance:Combat:Effectiveness`):** pares (atacante, defensor, multiplicador); par ausente vale 1; 0 = imune. Hoje: Fogo > Vento, Vento > Água, Água > Fogo com ×1,5, e os pares inversos com ×0,75.
  Cresce por configuração, junto do minério que introduz um elemento novo.
- **`DepositDTO`** (por jogador): `PlayerId`, `Slot`, `WeekStart`, `Element`, `DefenseMax`, `DefenseCurrent`, `StockMax`, `StockRemaining`, `Status`
  (`Sealed`, `Open`, `Depleted`, `Expired`), `RerollCount`, `xmin`. Índice único em (`PlayerId`, `WeekStart`, `Slot`).
- **Energia de exploração (`PlayerDTO`):** `ExploreEnergy`, `ExploreMaxEnergy` e `LastExploreEnergyUpdateUtc`, com **a mesma regra** da energia normal
  (regenerar sem perder o resto parcial), parametrizada por máximo e segundos por ponto. **Pequeno refactor necessário:** hoje `GameRules.RegenerateEnergy(energia, máximo, último)` lê os segundos por ponto
  da configuração da energia normal; precisa passar a receber esse valor como parâmetro (as duas barras compartilham o código, só mudam os números). `PlayerRes` ganha
  a segunda barra (`exploreEnergy`, `exploreMaxEnergy`, `exploreSecondsPerEnergy`, `nextExploreEnergyAtUtc`). Uma migration adiciona as colunas, e quem já joga
  começa com a barra cheia. Skills que melhorem essa barra ficam para depois.
- **Geração das ofertas:** **sob demanda**, na primeira consulta da semana (como as missões diárias), com sorteio determinístico por
  (jogador, semana, slot, nº de trocas): uma corrida de duas requisições gera a mesma oferta. Elemento, defesa e estoque saem de faixas da configuração;
  **estoque e defesa correlacionados** (mina mais rica, mais difícil) para a decisão "vale a pena?" ser calculável.

## 5. API (proposta)

| Rota | O que faz |
|---|---|
| `GET /state` | ganha `deposits[]` (elemento, defesa atual/máxima, estoque, status, estimativa de ganho) e os atributos de ataque e defesa e a matriz de efetividade |
| `POST /Shards/deposits/{id}/attack` | `{ "attackers": [...], "defenders": [...] }` (cada item: minério, estágio, quantidade). Dano = soma de (quantidade × ataque × multiplicador elemental). O contra-ataque do depósito é absorvido pela defesa dos defensores; o que não for absorvido **tira energia de exploração** (recusado se faltar). Há uma **prévia** antes de confirmar |
| `POST /Shards/deposits/{id}/mine` | `{ "times": 10 }`: gasta energia e rende ametista (e o que a tabela do depósito definir) enquanto houver estoque; exige defesa zero |
| `POST /Shards/deposits/{id}/rotate` | troca uma oferta **intocada** por outra, respeitando o limite de trocas |

Erros novos previstos: depósito inexistente, defesa ainda ativa, depósito esgotado/expirado, sem trocas disponíveis, gema/estágio sem ataque (bruta ou quartzo).

## 6. Riscos

1. **O prêmio precisa valer mais que a munição.** 1 ponto de ataque custa ~100 quartzos-equivalentes via polida. Um depósito de defesa 50 custa ~5.000 sem vantagem,
   ~3.300 com vantagem (×1,5). O estoque de ametista tem que pagar isso. **Depende de dar valor à ametista** (item 5 das recomendações); sem isso, não dá para equilibrar.
2. **Laço de retroalimentação:** ametista como munição e só obtida ao gastar munição. Aceitável se cada volta render só um pouco mais; precisa de simulação.
3. **Concorrência por energia:** minerar o depósito consome a mesma energia da produção normal. É proposital, mas o estoque tem que ser dimensionado em "dias de energia".
4. **Pesca por troca** (recomendação 4).
5. **Escopo:** é o maior bloco do jogo até agora (atributos, depósitos, ataque, mineração, rotação, missões, front). Por isso as fatias abaixo.

## 7. Fatias de implementação

> **Andamento:** a **fatia 1** (item 1 abaixo) está **pronta** (Evolução 8 em `PLANO.md`, seção 14; contrato do front em `PLANO-FRONT-CATALOGO.md`). As demais seguem em planejamento.

1. **Atributos e ametista:** `Amethyst`, elementos, ataque e defesa por estágio e a matriz de efetividade na configuração, expostos no `state`. Só dados e validação; **sem risco**.
2. **Depósitos e oferta semanal:** entidade, geração sob demanda, `deposits[]` no `state` e a troca (`rotate`).
3. **Ataque:** consumo de gemas, dano com vantagens elementais, defesa persistente.
4. **Minerar o depósito:** energia, estoque, esgotamento, expiração; missões novas.
5. **Depois (ideias para amadurecer):** defesa das gemas, estoque defensivo na mina do próprio jogador, minas finitas ocupando espaço de mina, combate entre jogadores, mercado.

## 8. Decisões em aberto

1. **Energia própria de exploração, só para a extração** (recomendação 1): confirma? E o tamanho inicial (sugestão: máximo 30, 1 ponto a cada 20 minutos, cerca de 10 h para encher; só um ponto de partida para simular).
2. **Utilidade da ametista: DECIDIDO.** Minério defensivo (1 de ataque, 5 de defesa), elemento Vento (provisório).
3. **Expiração:** depósito intocado expira na semana, atacado persiste (recomendação 3)?
4. **Limite de trocas:** 1 grátis por semana e as extras pagas em quartzo, com custo crescente (recomendação 4)?
5. **Números iniciais:** perfis da seção 4 (ataque e defesa por gema), multiplicadores ×1,5 e ×0,75, 3 depósitos por semana.
6. **Simulação de economia: FEITA** (`SIMULACAO-ECONOMIA.md`, ferramenta `Shards/tools/simulate_economy.py`). Resultados que alimentam a fatia 2:
   - O jogador "misto" (gemas só em 2 minas) produz ~100 pontos de ataque por dia (com os perfis atuais, Rubi facetado = 32 pontos); o que converte tudo em gema, ~33. O que limita é o **quartzo** da lapidação, não as gemas brutas (que sobram ~95%).
   - **Faixas sugeridas de defesa:** leve 200, média 400, difícil 800 (≈ 2, 4 e 8 dias de produção do jogador misto, sem vantagem elemental), com **~1,44 ametista de estoque por ponto de defesa** (≈ 290, 580 e 1.150). A faixa difícil não cabe numa semana, e tudo bem: o depósito atacado persiste.
   - A ametista precisa valer **~11 a 13 quartzos-equivalentes** para o depósito compensar: **falta definir o valor da ametista** (o uso já está decidido: minério defensivo) antes de fixar os números.
   - **Conversão de energia do contra-ataque: ~3 pontos de energia por ponto não absorvido** (empata com 1,5 a 4,7, conforme a gema; defender com Safira/Ametista compensa, com Rubi não).
   - Outros achados que pedem decisão (quartzo sem ralo depois de ~25 dias, gema bruta sem valor, vagonete nível 4 e 5, pontos de habilidade acabando em ~5 dias): ver as conclusões do relatório.

## 9. O depósito revida: modelos considerados (histórico)

> **Decisão posterior:** o contra-ataque **entra na versão 1** (leve na faixa fácil), com **dano na energia de exploração** (decisão 16). Os modelos A, B e C abaixo ficam como registro do raciocínio; vale o que está nas seções 2 a 4.

Quando o depósito puder **revidar**, a **defesa da gema** passa a ter função: é o quanto a gema aguenta do revide. O depósito ganharia **ataque e elemento**.

| | **Modelo A (versão 1, acima)** | **Modelo B (com revide)** |
|---|---|---|
| Depósito | passivo: só tem defesa | tem defesa e **ataque** (com elemento) |
| Gema | só **ataque** e elemento; **consumida** sempre | **ataque e defesa**; as gemas enviadas formam um **esquadrão** |
| Resolução | soma o dano e baixa a defesa | rodadas **determinísticas**: o esquadrão ataca, o depósito revida, com multiplicadores elementais |
| O que se perde | todas as gemas usadas | só as gemas **destruídas** (defesa esgotada); as **sobreviventes voltam** ao inventário |
| Defesa da gema | sem uso (adiar) | decide quem sobrevive |

Pontos a decidir quando for a hora do Modelo B:
- Ele **muda a economia**: se as sobreviventes voltam, o ralo de gemas fica menor (só as baixas), e o custo de abrir um depósito cai. O tamanho do ralo teria que ser reequilibrado.
- É um **mini auto-battle** (esquadrão, HP, rodadas): sem aleatoriedade, com **prévia** do resultado antes de confirmar.
- O que acontece com o dano causado quando o esquadrão cai antes de zerar a defesa (recomendo: a defesa do depósito **fica reduzida**).
- O **estoque defensivo das minas do jogador** (decisão 5) e um eventual combate entre jogadores usariam a mesma base (ataque, defesa, elemento).

Por isso a versão 1 já modela a defesa do depósito como **(elemento, pontos)** e os atributos das gemas em configuração: acrescentar defesa nas gemas e ataque nos depósitos depois é **aditivo**, sem refazer o que existir.

### Modelo C (ideia): cada gema usada tem uma **chance de ser gasta**

Meio-termo entre A e B, **sem rodadas nem pontos de vida**: no ataque, cada gema enviada ataca normalmente e, depois, **pode ser gasta** (vai embora) ou **sobreviver** (volta ao inventário).

- **Chance de gasto (proposta):** `A = ataque do depósito × multiplicador elemental` e `chance = A / (A + defesa da gema)`. Assim a defesa da gema tem papel claro:
  quanto mais defesa, menor a chance. Exemplo com ataque do depósito 10: polida (defesa 10) → 50%; facetada (defesa 40) → 20%. Vantagem elemental baixa a chance
  (ex.: ×0,75 em A); desvantagem sobe (×1,5 em A).
- **Sorteio no servidor**, sempre só para as gemas que **participaram** (o ataque para quando a defesa do depósito chega a zero).
- **Variância:** com poucas gemas (principalmente facetadas) a sorte pesa, e isso frustra. Mitigações: **prévia** com a chance de cada gema e a perda esperada; ou, sem sorteio,
  um **desgaste** determinístico (cada pilha acumula a chance fracionada e perde 1 gema quando passa de 1.0), que exige uma coluna a mais no inventário.
- **Economia:** o ralo passa a ser a **perda esperada** (menor que "gastar tudo"); os números precisam ser reequilibrados e simulados.

---

## 10. Como as minas chegam ao jogador (disponibilização) — proposta

Reaproveita o padrão das missões diárias (criação preguiçosa e determinística, sem jobs).

- **Liberação:** a partir do momento em que o jogador pode lapidar: skill `Workshop` no nível 1 ou mais (recomendado, não exige estado novo). Antes disso, `state.deposits` fica vazio ou nulo, como o `daily` antes da Missão 1.
- **Ciclo semanal:** a semana começa na **segunda-feira, na hora de virada já usada pelas diárias** (`Daily.ResetHourUtc`, 3 = meia-noite em Brasília). Tudo pelo relógio do servidor (`TimeProvider`).
- **Criação sob demanda:** o primeiro `GET /state` da semana cria as ofertas (como `CreateMissingNext` e as diárias). Quem não entra não gera linhas. O sorteio é determinístico por (jogador, semana, posição, nº de trocas), então duas leituras simultâneas geram a mesma oferta; a corrida cai no índice único e o `ConcurrencyRetry` resolve.
- **Quantas e quais:** **3 ofertas por semana** com **dificuldades diferentes** (leve, média e difícil), para haver uma escolha de risco e retorno e para um sorteio ruim não deixar a semana sem opção.
  Cada faixa tem em configuração: elemento (sorteado), defesa e estoque (correlacionados: mais rico, mais difícil) e tabela de drop. As faixas **escalam com o nível do jogador** (por degraus), para as ofertas nunca ficarem triviais nem impossíveis.
- **O que o jogador vê:** elemento, defesa atual e máxima, estoque e uma estimativa do ganho. O front calcula, com o inventário, quantas gemas seriam necessárias.
- **Rotação:** troca uma oferta **intocada** por outra, com limite (1 grátis por semana; extras pagas em quartzo, custo crescente) para evitar "pescar" o elemento ideal.
- **Expiração:** oferta **intocada** expira no fim da semana; depósito **já atacado** persiste até esgotar. Para não acumular lixo, um **limite de depósitos abertos** ao mesmo tempo (ex.: 3).
- **Histórico:** as linhas esgotadas ou expiradas ficam para estatística; limpeza depois, se for preciso.
- **API:** `state.deposits` (ofertas e abertos), mais as rotas `attack`, `mine` e `rotate` da seção 5.
- **Sem processo em segundo plano.** Nada aqui precisa de job ou timer, como a energia, o vagonete e as diárias:
  - A **criação** das ofertas acontece na primeira leitura da semana.
  - A **expiração** é calculada na leitura: uma oferta intocada de semana passada é tratada como expirada (`attack`, `mine` e `rotate` a recusam); não é preciso gravar nada para isso.
  - A virada da semana é uma função do relógio (`GameRules.GameWeek`, irmã de `DailyDay`), e não um evento.
  - O crescimento de dados é pequeno (~3 linhas por semana por jogador **ativo**); uma limpeza das linhas antigas pode ser feita junto da criação da semana nova, ou por uma rotina de manutenção bem mais tarde.

---

## 11. Recompensas dos depósitos: só ametista, ou também os outros minérios? — proposta (a confirmar)

A recompensa de um depósito vira uma **tabela de drop por faixa na configuração** (como as `Productions`: minério, peso, XP), e não "sempre ametista". Isso é só configuração; o código de extração sorteia a tabela a cada unidade e baixa o estoque.

**Regras para não criar uma "máquina de dinheiro":**
1. **Valor da recompensa por unidade menor que o custo da munição** (a simulação calcula isso). Senão, abrir depósitos vira lucro infinito.
2. **Recompensa só em minério bruto.** Gema polida ou facetada de recompensa pularia a oficina e voltaria como munição.
3. **A ametista continua sendo o prêmio exclusivo** (e a razão de atacar).

**O que cada minério acrescenta (e o que a simulação diz):**

| Minério | Efeito | Observação |
|---|---|---|
| **Ametista** | O prêmio exclusivo | Mantém a razão de atacar |
| **Gema bruta** (Rubi, Safira, Esmeralda) | Variedade de elementos: dá a quem só tem uma gema acesso às outras sem pagar a troca de produção (500, 1.200 ou 3.000) | **As brutas já sobram (~95%)**: sozinhas valem pouco, então funcionam como complemento, não como prêmio principal |
| **Quartzo** | Nova fonte de quartzo (já pedida) | **Perigo:** a munição custa quartzo, então um depósito que dá quartzo cria um laço. Deixar de fora na primeira versão e decidir junto com o mercado |

**Sugestão de desenho (a confirmar):** cada depósito tem o **elemento da defesa** e guarda a **gema desse elemento** (Fogo guarda Rubi, Água guarda Safira, Vento guarda Esmeralda), mais ametista. O depósito Neutral guarda só ametista. Isso cria uma decisão natural: para atacar uma defesa de Fogo com vantagem usa-se Água (Safira), mas se ganha Rubi.
Proporção por faixa (exemplo, a simular): leve 70% gema do elemento e 30% ametista; média 50/50; difícil 20% gema e 80% ametista.

**Pontos em aberto:**
- As gemas vindas de depósito entram nas missões de coleção (`Collector`) e nas diárias de "gemas brutas obtidas minerando ou coletando"? (Sugestão: sim em `Collector`, não nas diárias.)
- A simulação precisa de um valor para cada minério de recompensa; o relatório (`SIMULACAO-ECONOMIA.md`) usa, por enquanto, só a ametista.

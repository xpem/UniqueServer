# Ancestral Shards — Simulação da economia

Simulação com **valores esperados** (sem sorteio), em passos de 1 hora, sobre o balanceamento **real** do jogo (`Shards:Balance` do `appsettings.json`).
Serve para dimensionar as **minas finitas** (defesa, estoque, energia de exploração) antes de implementá-las. Ferramenta: `Shards/tools/simulate_economy.py`.

```bash
python Shards/tools/simulate_economy.py --days 90 --markdown     # regenera as tabelas desta página
python Shards/tools/simulate_economy.py --raw-gem-value 2.5 --explore-max 30 --explore-minutes-per-point 20
```

> **Como ler:** é um jogador "otimizador" que compra tudo assim que pode (minas, produção em gema, vagonete) e lapida só com o que **sobra**. Não é a média dos jogadores:
> serve para achar **gargalos e ordens de grandeza**. Refazer a simulação a cada mudança de balanceamento.

## Conclusões

1. **Depois de ~25 dias, o quartzo não tem mais onde ser gasto.** Quem joga só com quartzo compra tudo (5 minas, vagonetes no nível 5) em ~23 dias (4 visitas por dia) ou ~28 dias (2 visitas) e acumula ~3.000 quartzos por dia sem destino (159 mil no dia 90). O quartzo **inflaciona**. As minas finitas (munição em quartzo) e o mercado são justamente os ralos que faltam.
2. **Converter minas em gema hoje é prejuízo.** Uma mina em gema dá só 10% de quartzo; o jogador que converte todas as minas (2 a 5) cai de ~3.060 para ~730 quartzos por dia (−76%), leva ~83 dias para completar os vagonetes e, com a política "compras estruturais primeiro" do simulador, nunca chega a lapidar. As gemas só se pagam por missões; **o retorno vai ter que vir dos depósitos e do mercado**.
3. **As gemas brutas sobram (~95%).** Uma mina em gema rende ~427 gemas por dia; uma facetada gasta só 15 brutas. O que limita o ataque é o **quartzo** da lapidação (700 por facetada), não as gemas. Sem mercado, a gema bruta vale ~0.
4. **A estratégia mista é a mais forte.** Gemas só em 2 minas, com a mineração ativa e as outras minas em quartzo: ~2.200 quartzos e ~850 gemas por dia, o que sustenta **~100 pontos de ataque por dia** (≈ 3 facetadas de Rubi, de 32 pontos cada), contra ~33 de quem converte tudo. O jogo premia **equilibrar** quartzo e gema, o que é bom.
5. **Os vagonetes dos níveis 4 e 5 só pagam para quem visita ~1 vez por dia, e pouco.** Com 4 visitas por dia o nível 1 já basta; com 2, o nível 3. O nível 5 (12.000 quartzos) rende ~+5% sobre o nível 4 e só para quem visita uma vez ao dia. Funciona como ralo de quartzo, mas não como progressão.
6. **Os pontos de habilidade acabam na primeira semana.** As 4 skills somam 16 pontos (nível 16 = 2.040 XP) e o jogador ganha ~400 XP por dia: em ~5 dias todas estão no máximo. Depois disso o nível não tem efeito. Skills novas (por exemplo, de depósitos) ou uma curva de XP mais íngreme dariam sentido ao nível por mais tempo.

## Dimensionamento sugerido para os depósitos (a validar)

Com os **perfis das gemas** (polida; a facetada multiplica por 8): Rubi 4 de ataque e 2 de defesa, Safira 2 e 4, Esmeralda 3 e 3, Ametista 1 e 5.
Base: o jogador "misto" (4 visitas por dia) produz **~100 pontos de ataque por dia** com a lapidação (Rubi facetado); quem converte todas as minas, ~33.

| Faixa | Defesa | Dias de produção do jogador misto (sem vantagem) | Com vantagem ×1,5 | Estoque de ametista (≈ 1,44 por ponto de defesa) |
|---|---|---|---|---|
| Leve | 200 | ~2 dias | ~1,3 dia | ~290 (4 dias de energia de exploração) |
| Média | 400 | ~4 dias | ~2,7 dias | ~580 (8 dias) |
| Difícil | 800 | ~8 dias | ~5,3 dias | ~1.150 (16 dias) |

- **Defesa e estoque proporcionais** mantêm a "eficiência" igual nas três faixas; a difícil só rende mais no total e leva mais tempo.
- **A faixa difícil não cabe numa semana** de produção (~8 dias). Tudo bem: um depósito já atacado **persiste** (decisão do plano), então o progresso atravessa a semana. A difícil vira meta de longo prazo, não tarefa semanal.
- **Energia de exploração (30 de máximo, 1 ponto a cada 20 minutos = 72 por dia):** extrair a faixa leve (290) leva ~4 dias; a média, ~8; a difícil, ~16. Com 3 ofertas por semana e no máximo ~504 extrações por semana, o jogador **escolhe** onde gastar.
- **Valor mínimo da ametista** para o depósito empatar com o custo da munição: ~11 a 13 quartzos-equivalentes por unidade (tabela 3.2). **A ametista agora é o minério defensivo** (ataque 1 e defesa 5), então o valor dela vem do papel de defesa, e ainda precisa ser definido.

## Contra-ataque: defender com gemas ou tomar o dano na energia?

Regra aprovada: o contra-ataque que as gemas de defesa **não absorvem** tira **energia de exploração**; o ataque é recusado se faltar energia (com prévia). As gemas de defesa só são gastas na medida do que absorvem.

Para o jogador **usar** defesa, tomar o dano na energia tem que custar mais que defender. Pela tabela 3.3, cada ponto de defesa custa **de 18 a 56 quartzos-equivalentes**, e um ponto de energia vale ~12, ou seja, **empata com 1,5 a 4,7 pontos de energia por ponto de contra-ataque**:

| Gema | Empata com (energia por ponto) | Leitura |
|---|---|---|
| Ametista | 1,5 a 1,9 | A defesa mais barata |
| Safira | 1,9 a 2,3 | Defensora especializada |
| Esmeralda | 2,6 a 3,1 | Equilibrada |
| Rubi | 3,8 a 4,7 | Atacante: defender com ela é caro (de propósito) |

**Conversão sugerida: ~3 pontos de energia por ponto de contra-ataque não absorvido.** Com 3, defender com Safira e Ametista compensa, com Esmeralda fica no limite e com Rubi não compensa, o que **preserva os papéis**. É um parâmetro de configuração; ajustar quando o valor da ametista for definido.

## Recomendações

1. **Definir o valor da ametista antes de fixar defesa e estoque** (ela é o minério defensivo; precisa valer ≥ ~12 quartzos-equivalentes para os depósitos compensarem).
2. **Decidir se a gema bruta deve importar.** Hoje a lapidação é pesada em quartzo (100 por polida, 400 por facetada) e leve em gemas (5 e 3). Se as gemas brutas devem ter peso como munição, inverter o balanço (mais gemas, menos quartzo por receita) é uma mudança de configuração, não de código. **Rodar de novo a simulação** depois de decidir.
3. **Revisar os níveis 4 e 5 do vagonete** (custo 5.000 e 12.000 para quase nenhum ganho): ou encurtar, ou dar-lhes outro benefício (por exemplo, no depósito).
4. **Dar mais profundidade às skills** (pontos acabam em ~5 dias): novas skills de depósito/exploração ou uma curva de XP mais íngreme.
5. **Fixar a conversão de energia do contra-ataque em ~3 por ponto** (e recalibrar quando o valor da ametista for definido).
6. **Manter a simulação** como parte do fluxo: toda mudança de balanceamento (mercado, ametista, depósitos) deve rodar de novo antes de ir para a configuração.

## O que NÃO está modelado
Recompensas de missões e diárias (estimativa: cerca de 3.600 quartzos em missões únicas e algumas centenas por dia nas diárias), o mercado (não existe), os depósitos (só a análise paramétrica da seção 3), o tempo que o jogador demora para decidir, a variância (tudo é valor esperado) e jogadores que não seguem a estratégia otimizadora.

---


## 1. Renda por mina e por dia (valores esperados)

| visitas/dia | nível do vagonete | capacidade | entre visitas | minérios por coleta | minérios por dia (1 mina) |
|---|---|---|---|---|---|
| 2 | 0 | 100 | 12 h | 100 | 200 |
| 2 | 5 | 600 | 12 h | 238 | 475 |
| 4 | 0 | 100 | 6 h | 100 | 400 |
| 4 | 1 | 150 | 6 h | 119 | 475 |
| 4 | 2 | 220 | 6 h | 119 | 475 |
| 4 | 3 | 320 | 6 h | 119 | 475 |
| 4 | 4 | 450 | 6 h | 119 | 475 |
| 4 | 5 | 600 | 6 h | 119 | 475 |
| 6 | 0 | 100 | 4 h | 79 | 475 |
| 6 | 5 | 600 | 4 h | 79 | 475 |

Observação: com poucas visitas, o vagonete (não a energia) limita a renda; ampliar o vagonete só rende para quem visita pouco.

Mina em gema: por minério, 90% de gema e 10% de quartzo (a mina de quartzo dá 100% de quartzo). Ao trocar uma mina para gema, a renda de quartzo dela cai 90%.
Energia: 100 pontos, 16.67 por hora = 400 minerações ativas por dia (se todas forem gastas).

## 2. Linha do tempo de um jogador (90 dias)


### só quartzo, 4 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 11 | QQ | 157 | 957 | 0 | 0/0/0 |
| 3 | 12 | 3 | 222 | QQQ | 310 | 1.470 | 0 | 0/0/0 |
| 7 | 18 | 4 | 3332 | QQQQ | 1.300 | 2.043 | 0 | 0/0/0 |
| 14 | 26 | 5 | 44333 | QQQQQ | 1.540 | 2.906 | 0 | 0/0/0 |
| 30 | 39 | 5 | 55444 | QQQQQ | 11.500 | 3.060 | 0 | 0/0/0 |
| 60 | 56 | 5 | 55555 | QQQQQ | 67.300 | 3.060 | 0 | 0/0/0 |
| 90 | 68 | 5 | 55555 | QQQQQ | 159.100 | 3.060 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; vagonete nível 2: dia 2; mina 4: dia 4; vagonete nível 3: dia 5; mina 5: dia 10; vagonete nível 4: dia 12; vagonete nível 5: dia 23

### só quartzo, 2 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 4 | 2 | 10 | QQ | 0 | 500 | 0 | 0/0/0 |
| 3 | 8 | 3 | 111 | QQQ | 450 | 717 | 0 | 0/0/0 |
| 7 | 13 | 4 | 2222 | QQQQ | 1.275 | 1.182 | 0 | 0/0/0 |
| 14 | 18 | 5 | 33333 | QQQQQ | 1.185 | 2.287 | 0 | 0/0/0 |
| 30 | 27 | 5 | 54444 | QQQQQ | 7.705 | 2.720 | 0 | 0/0/0 |
| 60 | 39 | 5 | 55555 | QQQQQ | 41.305 | 2.720 | 0 | 0/0/0 |
| 90 | 48 | 5 | 55555 | QQQQQ | 122.905 | 2.720 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 3; vagonete nível 2: dia 4; mina 4: dia 6; vagonete nível 3: dia 8; mina 5: dia 13; vagonete nível 4: dia 16; vagonete nível 5: dia 28

### gemas em todas as minas 2+ (ativa na gema), lapida, 4 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 11 | QQ | 157 | 957 | 0 | 0/0/0 |
| 3 | 16 | 3 | 211 | QSQ | 347 | 1.116 | 358 | 1.076/0/0 |
| 7 | 29 | 3 | 222 | QSR | 1.154 | 993 | 847 | 5.932/0/0 |
| 14 | 43 | 4 | 3322 | QSRQ | 1.405 | 1.022 | 1.480 | 16.289/0/0 |
| 30 | 65 | 5 | 33333 | QSREQ | 1.207 | 931 | 1.904 | 44.529/0/0 |
| 60 | 93 | 5 | 44443 | QSREE | 874 | 731 | 2.329 | 113.662/0/0 |
| 90 | 114 | 5 | 54444 | QSREE | 5.798 | 731 | 2.329 | 183.538/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; mina 2 em Sapphire: dia 2; vagonete nível 2: dia 3; mina 3 em Ruby: dia 6; mina 4: dia 9; vagonete nível 3: dia 11; mina 4 em Emerald: dia 20; mina 5: dia 27; mina 5 em Emerald: dia 32; vagonete nível 4: dia 39; vagonete nível 5: dia 83

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 4 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 11 | QQ | 157 | 957 | 0 | 0/0/0 |
| 3 | 12 | 3 | 211 | QSQ | 786 | 1.262 | 177 | 531/0/0 |
| 7 | 18 | 4 | 2222 | QSRQ | 946 | 1.378 | 470 | 3.292/0/0 |
| 14 | 26 | 5 | 33330 | QSRQQ | 115 | 1.738 | 850 | 9.239/0/0 |
| 30 | 39 | 5 | 44444 | QSRQQ | 7.363 | 2.210 | 850 | 22.833/0/0 |
| 60 | 56 | 5 | 55555 | QSRQQ | 1.575 | 2.210 | 850 | 48.056/2/17 |
| 90 | 68 | 5 | 55555 | QSRQQ | 1.587 | 2.210 | 850 | 72.129/0/112 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; mina 2 em Sapphire: dia 2; vagonete nível 2: dia 3; mina 3 em Ruby: dia 5; mina 4: dia 6; vagonete nível 3: dia 8; mina 5: dia 14; vagonete nível 4: dia 18; vagonete nível 5: dia 33; primeira polida: dia 55; primeira facetada: dia 55

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 2 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 4 | 2 | 10 | QQ | 0 | 500 | 0 | 0/0/0 |
| 3 | 8 | 3 | 110 | QSQ | 250 | 717 | 0 | 0/0/0 |
| 7 | 13 | 3 | 222 | QSR | 168 | 853 | 190 | 1.332/0/0 |
| 14 | 18 | 4 | 3322 | QSRQ | 1.129 | 1.123 | 794 | 6.891/0/0 |
| 30 | 27 | 5 | 44433 | QSRQQ | 827 | 1.867 | 853 | 20.512/0/0 |
| 60 | 39 | 5 | 55544 | QSRQQ | 10.831 | 1.867 | 853 | 46.108/0/0 |
| 90 | 48 | 5 | 55555 | QSRQQ | 1.535 | 1.867 | 853 | 70.819/0/59 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 3; mina 2 em Sapphire: dia 3; vagonete nível 2: dia 5; mina 3 em Ruby: dia 7; mina 4: dia 10; vagonete nível 3: dia 12; mina 5: dia 20; vagonete nível 4: dia 25; vagonete nível 5: dia 42; primeira polida: dia 68; primeira facetada: dia 69

## 2b. Quanto de ataque o jogador consegue produzir por dia (regime estável)

1 facetada = 15 gemas brutas + 700 quartzos = 32 pontos de ataque (0.47 brutas e 21.9 quartzos por ponto, com Rubi, o atacante). Usa a renda média dos últimos 7 dias, depois de todas as compras estruturais.
| estratégia | quartzo/dia | gemas brutas/dia | pontos de ataque/dia | limitado por | dias p/ defesa 200 (neutro) | defesa 400 | defesa 800 |
|---|---|---|---|---|---|---|---|
| só quartzo, 4 visitas/dia | 3.060 | 0 | 0 | gemas brutas | - | - | - |
| só quartzo, 2 visitas/dia | 2.720 | 0 | 0 | gemas brutas | - | - | - |
| gemas em todas as minas 2+ (ativa na gema), lapida, 4 visitas/dia | 731 | 2.329 | 33 | quartzo | 6.0 | 12.0 | 23.9 |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 4 visitas/dia | 2.210 | 850 | 101 | quartzo | 2.0 | 4.0 | 7.9 |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 2 visitas/dia | 1.867 | 853 | 85 | quartzo | 2.3 | 4.7 | 9.4 |

Leitura: o que limita o ataque é o quartzo da lapidação (700 por facetada), não as gemas brutas, que sobram. Por isso o jogador que converte todas as minas em gema fica pobre em quartzo; a estratégia mista rende mais ataque.

## 3. Depósitos (análise paramétrica; ainda não implementados)

Valor de referência de 1 gema bruta: 2.5 quartzos (hipótese; o mercado não existe). Munição de ataque: Rubi (polida 4, facetada 32); vantagem x1.5, desvantagem x0.75 (da matriz de efetividade).

### 3.1 Custo para quebrar a defesa (quartzo-equivalente), com facetadas

| defesa (pontos) | com vantagem x1,5 | neutro x1 | com desvantagem x0,75 |
|---|---|---|---|
| 100 | 3 fac. = 2.100 q + 45 brutas (2.212 eq.) | 4 fac. = 2.800 q + 60 brutas (2.950 eq.) | 5 fac. = 3.500 q + 75 brutas (3.688 eq.) |
| 200 | 5 fac. = 3.500 q + 75 brutas (3.688 eq.) | 7 fac. = 4.900 q + 105 brutas (5.162 eq.) | 9 fac. = 6.300 q + 135 brutas (6.638 eq.) |
| 400 | 9 fac. = 6.300 q + 135 brutas (6.638 eq.) | 13 fac. = 9.100 q + 195 brutas (9.588 eq.) | 17 fac. = 11.900 q + 255 brutas (12.538 eq.) |
| 800 | 17 fac. = 11.900 q + 255 brutas (12.538 eq.) | 25 fac. = 17.500 q + 375 brutas (18.438 eq.) | 34 fac. = 23.800 q + 510 brutas (25.075 eq.) |
| 1600 | 34 fac. = 23.800 q + 510 brutas (25.075 eq.) | 50 fac. = 35.000 q + 750 brutas (36.875 eq.) | 67 fac. = 46.900 q + 1.005 brutas (49.412 eq.) |

### 3.2 Estoque e energia de exploração

Energia de exploração: máximo 30, 1 ponto a cada 20 min = 72 pontos por dia (enche em 10 h). Supondo 1 ametista por ponto.
| estoque | valor mínimo da ametista (quartzo-eq. por unidade) para defesa 200 | defesa 400 | defesa 800 |
|---|---|---|---|
| 2 dias de energia = 144 ametistas | 25.6 | 46.1 | 87.1 |
| 4 dias de energia = 288 ametistas | 12.8 | 23.0 | 43.5 |
| 8 dias de energia = 576 ametistas | 6.4 | 11.5 | 21.8 |
| 16 dias de energia = 1.152 ametistas | 3.2 | 5.8 | 10.9 |

Leitura: o "valor mínimo" é quanto cada ametista precisa valer para o depósito empatar com o custo da munição (com vantagem elemental). Se a ametista valer menos que isso, ninguém compensa abrir o depósito.

### 3.3 Contra-ataque: defender com gemas ou tomar o dano na energia?

Custo de 1 gema lapidada (quartzo-equivalente, bruta a 2.5): polida 112, facetada 738. Um ponto de energia de exploração vale ~12 (1 ametista de extração perdida; hipótese).
| gema | estágio | defesa | custo da gema | custo por ponto de defesa | energia por ponto de contra-ataque para empatar |
|---|---|---|---|---|---|
| Sapphire | polida | 4 | 112 | 28.1 | 2.3 |
| Sapphire | facetada | 32 | 738 | 23.0 | 1.9 |
| Emerald | polida | 3 | 112 | 37.5 | 3.1 |
| Emerald | facetada | 24 | 738 | 30.7 | 2.6 |
| Ruby | polida | 2 | 112 | 56.2 | 4.7 |
| Ruby | facetada | 16 | 738 | 46.1 | 3.8 |
| Amethyst | polida | 5 | 112 | 22.5 | 1.9 |
| Amethyst | facetada | 40 | 738 | 18.4 | 1.5 |

Leitura: se cada ponto de contra-ataque não absorvido custar menos energia que a coluna da direita, é mais barato tomar o dano que se defender (ninguém usa defesa). Com defesa ~1.5 a 4.7 de energia por ponto, uma conversão de ~3 por ponto deixa as duas opções equivalentes na média.

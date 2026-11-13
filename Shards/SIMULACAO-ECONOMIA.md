# Ancestral Shards — Simulação da economia

Simulação com **valores esperados** (sem sorteio), em passos de 1 hora, sobre o balanceamento **real** do jogo (`Shards:Balance` do `appsettings.json`).
Serve para dimensionar as **minas finitas** (defesa, estoque, energia de exploração) antes de implementá-las. Ferramenta: `Shards/tools/simulate_economy.py`.

```bash
python Shards/tools/simulate_economy.py --days 90 --markdown     # regenera as tabelas desta página
python Shards/tools/simulate_economy.py --raw-gem-value 2.5 --explore-max 30 --explore-minutes-per-point 20
```

> **Como ler:** é um jogador "otimizador" que compra tudo assim que pode (minas, produção em gema, vagonete) e lapida só com o que **sobra**. Não é a média dos jogadores:
> serve para achar **gargalos e ordens de grandeza**. Refazer a simulação a cada mudança de balanceamento.
>
> **Perfis de visita:** a maioria dos jogadores visita **1 vez por dia** ou **a cada 2 dias**; só alguns visitam várias vezes por dia. Em cada visita o jogador gasta toda a energia (normal e de exploração) e as barras só acumulam até o teto. Por isso os perfis **assíduo (4x por dia), diário (1x) e a cada 2 dias** aparecem lado a lado (seções 2b e 3.6). **O diário é a referência de calibração.**

## Conclusões

1. **Depois de ~25 dias, o quartzo não tem mais onde ser gasto.** Quem joga só com quartzo compra tudo (5 minas, vagonetes no nível 5) em ~23 dias (4 visitas por dia) ou ~28 dias (2 visitas) e acumula ~3.000 quartzos por dia sem destino (159 mil no dia 90). O quartzo **inflaciona**. As minas finitas (munição em quartzo) e o mercado são justamente os ralos que faltam.
2. **Converter minas em gema hoje é prejuízo.** Uma mina em gema dá só 10% de quartzo; o jogador que converte todas as minas (2 a 5) cai de ~3.060 para ~730 quartzos por dia (−76%), leva ~83 dias para completar os vagonetes e, com a política "compras estruturais primeiro" do simulador, nunca chega a lapidar. As gemas só se pagam por missões; **o retorno vai ter que vir dos depósitos e do mercado**.
3. **As gemas brutas sobram (~95%).** Uma mina em gema rende ~427 gemas por dia; uma facetada gasta só 15 brutas. O que limita o ataque é o **quartzo** da lapidação (700 por facetada), não as gemas. Sem mercado, a gema bruta vale ~0.
4. **A estratégia mista é a mais forte.** Gemas só em 2 minas, com a mineração ativa e as outras minas em quartzo: ~2.200 quartzos e ~850 gemas por dia, o que sustenta **~100 pontos de ataque por dia** (≈ 3 facetadas de Rubi, de 32 pontos cada), contra ~33 de quem converte tudo. O jogo premia **equilibrar** quartzo e gema, o que é bom.
5. **O vagonete é a progressão de quem visita pouco; só o nível 5 sobra.** Para o assíduo o nível 1 já basta; para o **diário** o nível 4 é o último que rende (+130 minérios por dia por mina, retorno em ~38 dias) e o nível 5 rende só +25 por dia (~240 dias de retorno com o custo atual de 6.000), porque o vagonete já passa de um dia de produção (475); para quem visita **a cada 2 dias** todos os níveis rendem (o nível 5 dá +75 por dia, ~80 dias com o custo atual). O nível 5 só serve a quem passa mais de 1 dia fora (tabela 3.6).
6. **Quem visita pouco produz bem menos, e o gargalo muda.** O jogador "misto" faz ~101 pontos de ataque por dia visitando 4x por dia, **~75 visitando 1x** e **~23 visitando a cada 2 dias** (tabela 2b). Para o diário, a mineração ativa são ~100 minérios por dia (a energia normal só acumula 100), contra ~475 do vagonete de UMA mina: o vagonete pesa muito mais que a energia normal. Na energia de exploração, o teto de 30 limita o diário a **30 unidades por dia** (e o casual a 15), contra os 72 por dia que o assíduo regenera (tabela 3.6): com estoque de 2,5 por ponto, a faixa leve levaria ~17 dias para o diário e ~33 para o casual.
7. **Os pontos de habilidade acabam na primeira semana.** As 4 skills somam 16 pontos (nível 16 = 2.040 XP) e o jogador ganha ~400 XP por dia: em ~5 dias todas estão no máximo. Depois disso o nível não tem efeito. Skills novas (por exemplo, de depósitos) ou uma curva de XP mais íngreme dariam sentido ao nível por mais tempo.

## Dimensionamento sugerido para os depósitos (a validar)

Com os **perfis das gemas** (polida; a facetada multiplica por 8): Rubi 4 de ataque e 2 de defesa, Safira 2 e 4, Esmeralda 3 e 3, Ametista 1 e 5.
Base: o jogador "misto" produz **~100 pontos de ataque por dia** visitando 4x por dia, **~75 visitando 1x por dia** (a referência) e **~23 a cada 2 dias**, com a lapidação (Rubi facetado); quem converte todas as minas, ~33 (4x por dia). Os "dias de produção" abaixo valem para o assíduo; para o diário multiplique por ~1,3 (leve ~2,7 dias, média ~5,3, difícil ~10,6) e para o casual por ~4,3.

| Faixa | Defesa | Dias de produção do jogador misto (sem vantagem) | Com vantagem ×1,5 | Estoque de ametista (2,5 por ponto de defesa desde a fatia 4; era 1,44) |
|---|---|---|---|---|
| Leve | 200 | ~2 dias | ~1,3 dia | ~500 (7 dias de energia de exploração) |
| Média | 400 | ~4 dias | ~2,7 dias | ~1.000 (14 dias) |
| Difícil | 800 | ~8 dias | ~5,3 dias | ~2.000 (28 dias) |

- **Defesa e estoque proporcionais** mantêm a "eficiência" igual nas três faixas; a difícil só rende mais no total e leva mais tempo.
- **A faixa difícil não cabe numa semana** de produção (~8 dias). Tudo bem: um depósito já atacado **persiste** (decisão do plano), então o progresso atravessa a semana. A difícil vira meta de longo prazo, não tarefa semanal.
- **Energia de exploração (1 ponto a cada 20 minutos = 72 por dia, se o jogador usar tudo):** com teto de 30 só o assíduo aproveita os 72 por dia; o diário extrai 30 e o casual 15. Com o **teto de 72 (valor atual desde a revisão por perfil)** o diário extrai 72 por dia e a faixa leve (500) leva ~7 dias, a média ~14 e a difícil ~28; o casual extrai 36 por dia (leve em ~14 dias). Ver a tabela 3.6.
- **Valor mínimo da ametista** para o depósito empatar com o custo da munição: ~11 a 13 quartzos-equivalentes por unidade (tabela 3.2). **A ametista agora é o minério defensivo** (ataque 1 e defesa 5), então o valor dela vem do papel de defesa, e o valor foi **decidido em 12 quartzos-equivalentes** (decisão 18 de `PLANO-MINAS-FINITAS.md`). Com 100% de ametista na recompensa (decisão 19), o estoque de ~1,44 por ponto de defesa continua valendo como foi simulado.

## Contra-ataque: defender com gemas ou tomar o dano na energia?

Regra aprovada: o contra-ataque que as gemas de defesa **não absorvem** tira **energia de exploração**; o ataque é recusado se faltar energia (com prévia). As gemas de defesa só são gastas na medida do que absorvem.

Para o jogador **usar** defesa, tomar o dano na energia tem que custar mais que defender. Pela tabela 3.3, cada ponto de defesa custa **de 18 a 56 quartzos-equivalentes**, e um ponto de energia vale ~12, ou seja, **empata com 1,5 a 4,7 pontos de energia por ponto de contra-ataque**:

| Gema | Empata com (energia por ponto) | Leitura |
|---|---|---|
| Ametista | 1,5 a 1,9 | A defesa mais barata |
| Safira | 1,9 a 2,3 | Defensora especializada |
| Esmeralda | 2,6 a 3,1 | Equilibrada |
| Rubi | 3,8 a 4,7 | Atacante: defender com ela é caro (de propósito) |

**Conversão sugerida: ~3 pontos de energia por ponto de contra-ataque não absorvido.** Com 3, defender com Safira e Ametista compensa, com Esmeralda fica no limite e com Rubi não compensa, o que **preserva os papéis**. É um parâmetro de configuração (`Combat.EnergyPerCounterPoint`, hoje 3, com a ametista a 12 quartzos-equivalentes).

**Implementado na fatia 3 (Evolução 11):** o contra-ataque é proporcional ao dano causado, com razão por faixa (**leve 5%, média 10%, difícil 20%**, `Deposits.Tiers[].CounterRatio`). A tabela 3.4 abaixo confirma a calibração: a faixa leve cabe numa barra de exploração (tomar o dano na energia sai mais barato que defender); a média empata; na difícil defender compensa. O custo da defesa fica entre 8% e 20% do custo de atacar.

**Achado da fatia 4 (tabela 3.5), já corrigido:** com a ametista a 12 quartzos-equivalentes e estoque de 1,44 por ponto de defesa o prêmio da extração NÃO pagava a munição (de 0,59x a 0,94x do custo de atacar mais defender). O estoque foi subido para **2,5 por ponto de defesa** (`Deposits.StockPerDefense`): o depósito neutro passa a empatar e, com vantagem elemental, rende cerca de 1,4x. Contrapartida: a extração leva mais tempo (leve ~7 dias de energia de exploração, média ~14, difícil ~28), o que reforça a escolha de onde gastar a energia.

## Recomendações

1. **Definir o valor da ametista antes de fixar defesa e estoque** (ela é o minério defensivo; precisa valer ≥ ~12 quartzos-equivalentes para os depósitos compensarem).
2. **Decidir se a gema bruta deve importar.** Hoje a lapidação é pesada em quartzo (100 por polida, 400 por facetada) e leve em gemas (5 e 3). Se as gemas brutas devem ter peso como munição, inverter o balanço (mais gemas, menos quartzo por receita) é uma mudança de configuração, não de código. **Rodar de novo a simulação** depois de decidir.
3. **Nível 5 do vagonete: ajustado** de 12.000 para 6.000 quartzos (só serve a quem passa mais de 1 dia fora; retorno de ~80 dias para quem visita a cada 2 dias). O nível 4 está bom para o diário. Ver a tabela 3.6.
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
| 1 | 0 | 100 | 24 h | 100 | 100 |
| 1 | 1 | 150 | 24 h | 150 | 150 |
| 1 | 2 | 220 | 24 h | 220 | 220 |
| 1 | 3 | 320 | 24 h | 320 | 320 |
| 1 | 4 | 450 | 24 h | 450 | 450 |
| 1 | 5 | 600 | 24 h | 475 | 475 |
| 0.5 | 0 | 100 | 48 h | 100 | 50 |
| 0.5 | 1 | 150 | 48 h | 150 | 75 |
| 0.5 | 2 | 220 | 48 h | 220 | 110 |
| 0.5 | 3 | 320 | 48 h | 320 | 160 |
| 0.5 | 4 | 450 | 48 h | 450 | 225 |
| 0.5 | 5 | 600 | 48 h | 600 | 300 |

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
| 30 | 39 | 5 | 55555 | QQQQQ | 5.500 | 3.060 | 0 | 0/0/0 |
| 60 | 56 | 5 | 55555 | QQQQQ | 97.300 | 3.060 | 0 | 0/0/0 |
| 90 | 68 | 5 | 55555 | QQQQQ | 189.100 | 3.060 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; vagonete nível 2: dia 2; mina 4: dia 4; vagonete nível 3: dia 5; mina 5: dia 10; vagonete nível 4: dia 12; vagonete nível 5: dia 21

### só quartzo, 2 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 4 | 2 | 10 | QQ | 0 | 500 | 0 | 0/0/0 |
| 3 | 8 | 3 | 111 | QQQ | 450 | 717 | 0 | 0/0/0 |
| 7 | 13 | 4 | 2222 | QQQQ | 1.275 | 1.182 | 0 | 0/0/0 |
| 14 | 18 | 5 | 33333 | QQQQQ | 1.185 | 2.287 | 0 | 0/0/0 |
| 30 | 27 | 5 | 55544 | QQQQQ | 1.705 | 2.720 | 0 | 0/0/0 |
| 60 | 39 | 5 | 55555 | QQQQQ | 71.305 | 2.720 | 0 | 0/0/0 |
| 90 | 48 | 5 | 55555 | QQQQQ | 152.905 | 2.720 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 3; vagonete nível 2: dia 4; mina 4: dia 6; vagonete nível 3: dia 8; mina 5: dia 13; vagonete nível 4: dia 16; vagonete nível 5: dia 25

### gemas em todas as minas 2+ (ativa na gema), lapida, 4 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 11 | QQ | 157 | 957 | 0 | 0/0/0 |
| 3 | 16 | 3 | 211 | QSQ | 347 | 1.116 | 358 | 1.076/0/0 |
| 7 | 29 | 3 | 222 | QSR | 1.154 | 993 | 847 | 5.932/0/0 |
| 14 | 43 | 4 | 3322 | QSRQ | 1.405 | 1.022 | 1.480 | 16.289/0/0 |
| 30 | 65 | 5 | 33333 | QSREQ | 1.207 | 931 | 1.904 | 44.529/0/0 |
| 60 | 93 | 5 | 44443 | QSREE | 874 | 731 | 2.329 | 113.662/0/0 |
| 90 | 114 | 5 | 55444 | QSREE | 5.798 | 731 | 2.329 | 183.538/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; mina 2 em Sapphire: dia 2; vagonete nível 2: dia 3; mina 3 em Ruby: dia 6; mina 4: dia 9; vagonete nível 3: dia 11; mina 4 em Emerald: dia 20; mina 5: dia 27; mina 5 em Emerald: dia 32; vagonete nível 4: dia 39; vagonete nível 5: dia 74

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 4 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 11 | QQ | 157 | 957 | 0 | 0/0/0 |
| 3 | 12 | 3 | 211 | QSQ | 786 | 1.262 | 177 | 531/0/0 |
| 7 | 18 | 4 | 2222 | QSRQ | 946 | 1.378 | 470 | 3.292/0/0 |
| 14 | 26 | 5 | 33330 | QSRQQ | 115 | 1.738 | 850 | 9.239/0/0 |
| 30 | 39 | 5 | 54444 | QSRQQ | 1.363 | 2.210 | 850 | 22.833/0/0 |
| 60 | 56 | 5 | 55555 | QSRQQ | 1.575 | 2.210 | 850 | 47.416/1/60 |
| 90 | 68 | 5 | 55555 | QSRQQ | 1.587 | 2.210 | 850 | 71.469/6/154 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 2; mina 2 em Sapphire: dia 2; vagonete nível 2: dia 3; mina 3 em Ruby: dia 5; mina 4: dia 6; vagonete nível 3: dia 8; mina 5: dia 14; vagonete nível 4: dia 18; vagonete nível 5: dia 30; primeira polida: dia 41; primeira facetada: dia 42

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 2 visitas/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 4 | 2 | 10 | QQ | 0 | 500 | 0 | 0/0/0 |
| 3 | 8 | 3 | 110 | QSQ | 250 | 717 | 0 | 0/0/0 |
| 7 | 13 | 3 | 222 | QSR | 168 | 853 | 190 | 1.332/0/0 |
| 14 | 18 | 4 | 3322 | QSRQ | 1.129 | 1.123 | 794 | 6.891/0/0 |
| 30 | 27 | 5 | 44433 | QSRQQ | 827 | 1.867 | 853 | 20.512/0/0 |
| 60 | 39 | 5 | 55555 | QSRQQ | 1.531 | 1.867 | 853 | 45.763/6/21 |
| 90 | 48 | 5 | 55555 | QSRQQ | 1.535 | 1.867 | 853 | 70.159/6/101 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 1; mina 3: dia 3; mina 2 em Sapphire: dia 3; vagonete nível 2: dia 5; mina 3 em Ruby: dia 7; mina 4: dia 10; vagonete nível 3: dia 12; mina 5: dia 20; vagonete nível 4: dia 25; vagonete nível 5: dia 39; primeira polida: dia 52; primeira facetada: dia 53

### só quartzo, 1 visita/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 3 | 2 | 00 | QQ | 0 | 200 | 0 | 0/0/0 |
| 3 | 5 | 2 | 11 | QQ | 50 | 283 | 0 | 0/0/0 |
| 7 | 9 | 3 | 211 | QQQ | 200 | 386 | 0 | 0/0/0 |
| 14 | 13 | 4 | 2222 | QQQQ | 1.275 | 796 | 0 | 0/0/0 |
| 30 | 19 | 5 | 43333 | QQQQQ | 3.905 | 1.738 | 0 | 0/0/0 |
| 60 | 27 | 5 | 55555 | QQQQQ | 25.985 | 2.550 | 0 | 0/0/0 |
| 90 | 34 | 5 | 55555 | QQQQQ | 102.485 | 2.550 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 2; mina 3: dia 5; vagonete nível 2: dia 7; mina 4: dia 12; vagonete nível 3: dia 15; mina 5: dia 24; vagonete nível 4: dia 28; vagonete nível 5: dia 41

### só quartzo, 1 visita a cada 2 dias

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 3 | 2 | 00 | QQ | 0 | 200 | 0 | 0/0/0 |
| 3 | 4 | 2 | 10 | QQ | 0 | 167 | 0 | 0/0/0 |
| 7 | 6 | 2 | 11 | QQ | 450 | 179 | 0 | 0/0/0 |
| 14 | 9 | 3 | 211 | QQQ | 200 | 207 | 0 | 0/0/0 |
| 30 | 13 | 4 | 3222 | QQQQ | 315 | 431 | 0 | 0/0/0 |
| 60 | 19 | 5 | 43333 | QQQQQ | 3.905 | 798 | 0 | 0/0/0 |
| 90 | 24 | 5 | 55544 | QQQQQ | 710 | 1.146 | 0 | 0/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 3; mina 3: dia 9; vagonete nível 2: dia 13; mina 4: dia 23; vagonete nível 3: dia 29; mina 5: dia 47; vagonete nível 4: dia 55; vagonete nível 5: dia 81

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita/dia

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 3 | 2 | 00 | QQ | 0 | 200 | 0 | 0/0/0 |
| 3 | 5 | 2 | 11 | QQ | 50 | 283 | 0 | 0/0/0 |
| 7 | 9 | 3 | 111 | QSQ | 315 | 359 | 19 | 135/0/0 |
| 14 | 13 | 3 | 222 | QSR | 168 | 493 | 171 | 1.332/0/0 |
| 30 | 19 | 4 | 3332 | QSRQ | 1.017 | 750 | 447 | 8.028/0/0 |
| 60 | 27 | 5 | 44444 | QSRQQ | 1.474 | 1.429 | 810 | 27.531/0/0 |
| 90 | 34 | 5 | 55555 | QSRQQ | 1.504 | 1.695 | 855 | 52.341/3/28 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 2; mina 3: dia 5; mina 2 em Sapphire: dia 6; vagonete nível 2: dia 9; mina 3 em Ruby: dia 14; mina 4: dia 19; vagonete nível 3: dia 24; mina 5: dia 38; vagonete nível 4: dia 45; vagonete nível 5: dia 63; primeira polida: dia 79; primeira facetada: dia 79

### mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita a cada 2 dias

| dia | nível | minas | vagonetes | produção | quartzo guardado | quartzo/dia (média 7d) | gemas brutas/dia | gemas bruta/polida/facetada |
|---|---|---|---|---|---|---|---|---|
| 1 | 3 | 2 | 00 | QQ | 0 | 200 | 0 | 0/0/0 |
| 3 | 4 | 2 | 10 | QQ | 0 | 167 | 0 | 0/0/0 |
| 7 | 6 | 2 | 11 | QQ | 450 | 179 | 0 | 0/0/0 |
| 14 | 9 | 3 | 111 | QSQ | 315 | 181 | 19 | 135/0/0 |
| 30 | 13 | 3 | 222 | QSR | 562 | 221 | 113 | 1.728/0/0 |
| 60 | 19 | 4 | 3332 | QSRQ | 1.017 | 331 | 221 | 8.028/0/0 |
| 90 | 24 | 5 | 43333 | QSRQQ | 772 | 514 | 247 | 16.668/0/0 |

Marcos (dia em que acontece): mina 2: dia 1; vagonete nível 1: dia 3; mina 3: dia 9; mina 2 em Sapphire: dia 11; vagonete nível 2: dia 17; mina 3 em Ruby: dia 27; mina 4: dia 37; vagonete nível 3: dia 47; mina 5: dia 75; vagonete nível 4: dia 89

## 2b. Quanto de ataque o jogador consegue produzir por dia (regime estável)

1 facetada = 15 gemas brutas + 700 quartzos = 32 pontos de ataque (0.47 brutas e 21.9 quartzos por ponto, com Rubi, o atacante). Usa a renda média dos últimos 7 dias, depois de todas as compras estruturais.
| estratégia | quartzo/dia | gemas brutas/dia | pontos de ataque/dia | limitado por | dias p/ defesa 200 (neutro) | defesa 400 | defesa 800 |
|---|---|---|---|---|---|---|---|
| só quartzo, 4 visitas/dia | 3.060 | 0 | 0 | gemas brutas | - | - | - |
| só quartzo, 2 visitas/dia | 2.720 | 0 | 0 | gemas brutas | - | - | - |
| gemas em todas as minas 2+ (ativa na gema), lapida, 4 visitas/dia | 731 | 2.329 | 33 | quartzo | 6.0 | 12.0 | 23.9 |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 4 visitas/dia | 2.210 | 850 | 101 | quartzo | 2.0 | 4.0 | 7.9 |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 2 visitas/dia | 1.867 | 853 | 85 | quartzo | 2.3 | 4.7 | 9.4 |
| só quartzo, 1 visita/dia | 2.550 | 0 | 0 | gemas brutas | - | - | - |
| só quartzo, 1 visita a cada 2 dias | 1.146 | 0 | 0 | gemas brutas | - | - | - |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita/dia | 1.695 | 855 | 77 | quartzo | 2.6 | 5.2 | 10.3 |
| mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita a cada 2 dias | 514 | 247 | 23 | quartzo | 8.5 | 17.0 | 34.1 |

Leitura: o que limita o ataque é o quartzo da lapidação (700 por facetada), não as gemas brutas, que sobram. Por isso o jogador que converte todas as minas em gema fica pobre em quartzo; a estratégia mista rende mais ataque.

## 3. Depósitos (análise paramétrica sobre o balanceamento real)

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

### 3.4 Contra-ataque por faixa (implementado): quebrar a defesa inteira de uma vez

O depósito contra-ataca com (dano efetivo x razão da faixa) pontos; o que as gemas defensoras não absorvem tira 3 de energia de exploração por ponto (barra de 30). A gema defensora absorve defesa / multiplicador (depósito contra gema; aqui, elemento neutro, x1). Um ponto de energia vale ~12 quartzos-equivalentes (hipótese).
| faixa | defesa | razão | contra-ataque total | energia se ninguém defender | defesa necessária | custo da defesa (q-eq.) | a energia equivale a (q-eq.) | custo da defesa / custo de atacar |
|---|---|---|---|---|---|---|---|---|
| 1 Light | 200 | 5% | 10 | 30 (1.0 barras) | Sapphire facetada x1 | 738 | 360 | 14% |
| 1 Light | 200 | 5% | 10 | 30 (1.0 barras) | Amethyst facetada x1 | 738 | 360 | 14% |
| 2 Medium | 400 | 10% | 40 | 120 (4.0 barras) | Sapphire facetada x2 | 1.475 | 1.440 | 15% |
| 2 Medium | 400 | 10% | 40 | 120 (4.0 barras) | Amethyst facetada x1 | 738 | 1.440 | 8% |
| 3 Hard | 800 | 20% | 160 | 480 (16.0 barras) | Sapphire facetada x5 | 3.688 | 5.760 | 20% |
| 3 Hard | 800 | 20% | 160 | 480 (16.0 barras) | Amethyst facetada x4 | 2.950 | 5.760 | 16% |

Leitura: a faixa leve cabe na barra de exploração (nem precisa de defesa); a média e a difícil custam mais energia do que a barra comporta de uma vez, então o jogador defende com gemas (barato perto do custo de atacar) ou divide o ataque em vários dias.

### 3.5 Fechamento do ciclo: o prêmio da extração paga a munição? (implementado)

Cada ponto de energia de exploração (1 por unidade) rende 1 ametista, que vale ~12 quartzos-equivalentes. Estoque = defesa x 2.5. Custo = atacar com facetadas de Rubi (elemento neutro, x1; com vantagem x1.5 sai mais barato) mais a defesa que cobre o contra-ataque (ametista facetada, a melhor defensora). Dias = estoque / (72 pontos por dia).
| faixa | elemento | estoque (ametistas) | dias de energia | valor do prêmio (q-eq.) | custo (atacar + defender) | saldo | prêmio / custo | estoque por ponto de defesa para empatar |
|---|---|---|---|---|---|---|---|---|
| 1 Light | neutro | 500 | 6.9 | 6.000 | 5.900 | 100 | 1.02x | 2.46 |
| 1 Light | vantagem | 500 | 6.9 | 6.000 | 4.425 | 1.575 | 1.36x | 1.84 |
| 2 Medium | neutro | 1.000 | 13.9 | 12.000 | 10.325 | 1.675 | 1.16x | 2.15 |
| 2 Medium | vantagem | 1.000 | 13.9 | 12.000 | 7.375 | 4.625 | 1.63x | 1.54 |
| 3 Hard | neutro | 2.000 | 27.8 | 24.000 | 21.388 | 2.612 | 1.12x | 2.23 |
| 3 Hard | vantagem | 2.000 | 27.8 | 24.000 | 15.488 | 8.512 | 1.55x | 1.61 |

Leitura: o prêmio paga o custo em todas as combinações (de 1.02x a 1.63x). A energia de exploração limita o ritmo da extração e o laço "ametista vira munição" só fecha se o prêmio superar a munição.

### 3.6 Perfis de visita: vagonete, energia normal e energia de exploração

O jogador gasta **toda** a energia (normal e de exploração) em cada visita. O assíduo visita a cada 6 h; o diário, a cada 24 h; o casual, a cada 48 h. A barra enche até o teto e o que passa dele se perde.

**Retorno de cada upgrade do vagonete, por perfil** (mina de quartzo; ganho marginal em minérios por dia sobre o nível anterior; retorno = custo ÷ ganho, em dias):

| nível | capacidade | custo (quartzo) | assíduo (4x por dia) | diário (1x por dia) | a cada 2 dias |
|---|---|---|---|---|---|
| 1 | 150 | 300 | +75/dia (4 d) | +50/dia (6 d) | +25/dia (12 d) |
| 2 | 220 | 800 | +0/dia (nenhum) | +70/dia (11 d) | +35/dia (23 d) |
| 3 | 320 | 2.000 | +0/dia (nenhum) | +100/dia (20 d) | +50/dia (40 d) |
| 4 | 450 | 5.000 | +0/dia (nenhum) | +130/dia (38 d) | +65/dia (77 d) |
| 5 | 600 | 6.000 | +0/dia (nenhum) | +25/dia (238 d) | +75/dia (80 d) |

Leitura: para o diário o nível 4 é o último que rende (o vagonete já enche em ~24 h); o nível 5 só ajuda quem passa mais de 1 dia fora (o casual). Para o casual todos os níveis rendem, mas o retorno é lento.

**Energia normal (mineração ativa) por perfil** (teto e ritmo da configuração):

| perfil | minerações ativas por visita | por dia |
|---|---|---|
| assíduo (4x por dia) | 100 | 400 |
| diário (1x por dia) | 100 | 100 |
| a cada 2 dias | 100 | 50 |

Leitura: a mineração ativa rende 1 minério por ponto. Para o diário são ~100 por dia, contra ~475 do vagonete de UMA mina: o vagonete pesa muito mais que a energia normal para quem visita pouco.

**Energia de exploração: dias para extrair um depósito inteiro** (estoque = defesa x 2.5, 1 ponto por unidade, 1 ponto a cada 20 min):

| teto da barra | perfil | unidades por visita | por dia | dias p/ faixa Light (500) | dias p/ faixa Medium (1.000) | dias p/ faixa Hard (2.000) |
|---|---|---|---|---|---|---|
| 30 | assíduo (4x por dia) | 18 | 72 | 6.9 | 13.9 | 27.8 |
| 30 | diário (1x por dia) | 30 | 30 | 16.7 | 33.3 | 66.7 |
| 30 | a cada 2 dias | 30 | 15 | 33.3 | 66.7 | 133.3 |
| 72 | assíduo (4x por dia) | 18 | 72 | 6.9 | 13.9 | 27.8 |
| 72 | diário (1x por dia) | 72 | 72 | 6.9 | 13.9 | 27.8 |
| 72 | a cada 2 dias | 72 | 36 | 13.9 | 27.8 | 55.6 |
| 144 | assíduo (4x por dia) | 18 | 72 | 6.9 | 13.9 | 27.8 |
| 144 | diário (1x por dia) | 72 | 72 | 6.9 | 13.9 | 27.8 |
| 144 | a cada 2 dias | 144 | 72 | 6.9 | 13.9 | 27.8 |

**Contra-ataque sem defesa, em barras de exploração** (quebrar a defesa inteira de uma vez, sem gemas defensoras):

| faixa | energia perdida | barras com teto 30 | barras com teto 72 | barras com teto 144 |
|---|---|---|---|---|
| 1 Light | 30 | 1.00 | 0.42 | 0.21 |
| 2 Medium | 120 | 4.00 | 1.67 | 0.83 |
| 3 Hard | 480 | 16.00 | 6.67 | 3.33 |

Leitura: o teto da barra decide quanto o jogador extrai por visita. Com 30, o diário extrai 30 por dia; com 72 (um dia cheio de regeneração) aproveita tudo o que acumulou. O contra-ataque da faixa leve passa a caber folgado na barra maior.

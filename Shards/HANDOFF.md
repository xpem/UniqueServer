# Ancestral Shards — Nota de passagem para a próxima sessão

Leia primeiro, nesta ordem: este arquivo, `Shards/PLANO-MINAS-FINITAS.md` (o que vem a seguir), `Shards/SIMULACAO-ECONOMIA.md` (números) e as seções 14 e 15 de `Shards/PLANO.md`.

## Onde estamos
- **Backend do jogo pronto e testado:** 338 testes passando (`dotnet test ShardsTests/ShardsTests.csproj`). Tem minas, vagonete, produção em gema, oficina de lapidação, missões, diárias, catálogo de minérios (com ataque, defesa e elemento) e a matriz de efetividade. Detalhes por evolução em `PLANO.md`.
- **Última evolução (15 no plano):** ametista em Vento (provisório), defesa por estágio, perfis das gemas (Rubi 4/2, Safira 2/4, Esmeralda 3/3, Ametista 1/5), matriz de efetividade no lugar do ciclo. Só dados: nenhuma regra de jogo mudou.
- **Próxima fatia (2):** a entidade do depósito (minas finitas) com oferta semanal e rotação. Tudo já decidido está na tabela de decisões de `PLANO-MINAS-FINITAS.md` (17 decisões). Depois: ataque com contra-ataque e dano na energia de exploração, e a extração de ametista.
- **Mercado:** planejado e **adiado de propósito** (`PLANO-MERCADO.md`).

## Pendências abertas
1. **O host `UniqueServer` não compila** com os pacotes atuais: o `UniqueServer.csproj` (modificado por fora, sem commit) tem `Microsoft.OpenApi` 3.11.0 com `Microsoft.AspNetCore.OpenApi` 10.0.14, e o gerador de comentários XML quebra (`IOpenApiMediaType.Example` somente leitura). Provável correção: voltar `Microsoft.OpenApi` para a série 2.x (era 2.9.0) ou remover a referência explícita. **Nada disso vem do código do Shards.** Perguntar ao dono do projeto antes de mexer.
2. **Sem commit:** tudo da Evolução 8 e 9, os planos novos (`PLANO-MINAS-FINITAS.md`, `PLANO-MERCADO.md`, `PLANO-FRONT-CATALOGO.md`, `SIMULACAO-ECONOMIA.md`), `Shards/tools/simulate_economy.py` e `ShardsTests/OreCatalogTests.cs`. Os `.csproj` modificados são atualizações de pacote do dono do projeto.
3. **Migrations:** as do Shards (`Init`, `AddMineProduction`, `AddMineCartLevel`, `AddItemStage`, entre outras) devem ser aplicadas pelo dono no banco. A fatia 2 vai precisar de migrations novas (depósito e energia de exploração).
4. **Decisões de balanceamento que a simulação pediu e ainda estão abertas:** o valor da ametista (precisa ~11 a 13 quartzos-equivalentes); se a gema bruta deve importar (a oficina é muito pesada em quartzo e leve em gemas); os níveis 4 e 5 do vagonete quase não rendem; os pontos de habilidade acabam em ~5 dias.
5. **Front:** o formato de `state.combat` mudou (agora `effectiveness[]`; antes `elementCycle`). Avisar a sessão do front. Contrato em `PLANO-FRONT-CATALOGO.md`.
6. Pequenas: a mensagem de `InvalidGem` cita "Ruby, Sapphire ou Emerald" em texto fixo; o `DailyTracker` conta como "gema" todo minério que não é quartzo.

## Decisões de projeto que valem lembrar
- Idioma: português em textos, respostas e documentação. Enums e rotas em inglês.
- Nenhum balanceamento fica no código: tudo vem do `appsettings.json` (`Shards:Balance`), e o validador (`GameBalanceOptionsValidator`) derruba a subida com mensagem clara se estiver errado.
- Um minério novo é uma linha no catálogo (`Ores`) mais o valor no enum `OreType`. Um elemento novo entra junto do minério que o introduz.
- Sem processo em segundo plano: tudo é calculado na leitura (como energia, vagonete e diárias). Ofertas semanais nascem na primeira leitura da semana e a expiração é calculada na leitura.
- Sempre rodar a simulação de novo ao mudar balanceamento: `python Shards/tools/simulate_economy.py --markdown`.

## Dicas de ferramenta (aprendidas na prática)
- **Build bloqueado pelo IIS Express:** se o Visual Studio estiver rodando a API, as DLLs de `UniqueServer/bin` ficam travadas. Compilar o host para uma pasta temporária: `dotnet build UniqueServer/UniqueServer.csproj -o <pasta temporária>`. Não encerrar o processo do usuário.
- **`dotnet ef migrations` com o IIS travando:** definir as variáveis de ambiente `OutputPath=<pasta>/` e `OutDir=<pasta>/net10.0/` ao rodar o comando (e `ASPNETCORE_ENVIRONMENT=Development`). Conferir o SQL com `dotnet ef migrations script <anterior>` e `has-pending-model-changes`. **Cuidado com migrations vazias:** o EF cria uma migration vazia se carregar uma DLL antiga.
- **Scripts Python no shell:** heredocs de Python com muitas aspas e barras invertidas falham no shell do Windows. Gravar o script com a ferramenta de escrita e executar com `python <arquivo>`.
- **Verificação por HTTP sem tocar no banco remoto:** montar um host isolado (projeto web que compila `ShardsController.cs`, `BaseController.cs` e `ShardsRateLimit.cs`, referencia `Shards` e `BaseModels`, usa EF InMemory e uma autenticação de teste por cabeçalho `X-Uid`) na pasta temporária da sessão. Alinhar as versões dos pacotes EF com as do `Shards.csproj` (hoje 10.0.14).
- **Testes de serviço:** usam EF InMemory (sem índices únicos nem `xmin`); a concorrência real só se confirma no Postgres.

# fabulasLA — análise dos dados do Fábulas Computacionais

Pacote R para ler, validar e analisar os dados de learning analytics gerados pelo jogo
**Fábulas Computacionais**: eventos (`events_*.jsonl`), estado da solução a cada tentativa e
expressões faciais (`affect_*.csv`).

## Instalação

A partir da raiz do repositório:

```r
install.packages("analysis/fabulasLA", repos = NULL, type = "source")
```

Dependências: dplyr, tidyr, purrr, tibble, stringr, readr, jsonlite e rlang. Para o relatório, também
rmarkdown, ggplot2, lme4, survival, pROC, ranger, patchwork e scales. Para conferir checksums, openssl.

## Uso

```r
library(fabulasLA)

# 1. Ler — pastas LearningAnalytics e/ou .zip exportados pelo Painel do Pesquisador (vários de uma vez)
coleta <- ler_coleta(c("PILOTO26/escola_A", "PILOTO26/escola_B/coleta_PILOTO26_ESC-B_PC-02.zip"))
coleta

# 2. Validar — integridade, duplicatas, lacunas, sessões interrompidas, catálogo, câmera
validar_coleta(coleta)

# 3. Tabelas prontas para análise
tb <- tabelas_analise(coleta)
tb$sessoes      # uma linha por sessão: escola, turma, condição, perfil, desfecho
tb$fases        # participante x fase: tentativas, tempo, conclusão, abandono
tb$tentativas   # estado da solução a cada tentativa: distância, posições alteradas...
tb$afeto_sessao # cobertura e médias faciais por sessão

# 4. Relatório completo (HTML) e livro de códigos
gerar_relatorio("PILOTO26", "relatorio_PILOTO26.html", titulo_estudo = "Piloto 2026")   # estudo em grupo
gerar_relatorio_individual("K7M2X", "PILOTO26")                                   # um estudante
exportar_codebook("codebook_PILOTO26.csv")
```

### Dados do servidor central (Supabase)

Se o jogo envia os dados ao servidor (ver `server/supabase`), baixe tudo de todas as escolas de uma vez.
Guarde a **chave de leitura** do estudo no `.Renviron` (`FABULAS_CHAVE_LEITURA=lei_...`), não no script:

```r
url <- "https://<projeto>.supabase.co"; chave_publica <- "sb_publishable_..."
status_servidor(url, chave_publica, "PILOTO26")   # sessões recebidas, eventos, amostras, fim da sessão
coleta <- ler_coleta_servidor(url, chave_publica, "PILOTO26", pasta = "PILOTO26/servidor")
# mesmo objeto de ler_coleta(); dá para juntar com .zip de escolas sem internet:
coleta <- ler_coleta(c("PILOTO26/servidor", "PILOTO26/escola_sem_internet.zip"))
```

### Relatórios: estudo em grupo e estudante individual

```r
gerar_relatorio("PILOTO26", "relatorio_grupo.html")              # todos os estudantes
gerar_relatorio_individual("K7M2X", "PILOTO26")                  # um estudante (relatorio_K7M2X.html)
# direto do servidor (chave de leitura em FABULAS_CHAVE_LEITURA):
gerar_relatorio_individual("K7M2X", servidor_url = url, chave_publica = chave_publica, estudo = "PILOTO26")
copiar_modelos_relatorio("meus_relatorios")   # .Rmd editáveis: ajuste 'params:' no cabeçalho e clique em Knit
```

O relatório individual responde, para uma criança: por onde andou e em que ordem; em que fases foi melhor ou
pior que os colegas; se corrigia aos poucos (depuração) ou mudava tudo (tentativa e erro); que erros repetiu;
quanto ficou parado; e **o que o rosto fazia em cada cena e logo depois de cada erro ou acerto**.

```r
# A mesma integração, como tabela, para qualquer análise:
afeto_por_evento(coleta, eventos = "solution_snapshot", antes_ms = 2000, depois_ms = 5000)

# Estados afetivos de aprendizagem por janela de 10 s (regras v0.1; ver regras_estados())
estados <- estados_faciais(coleta)
estados |> dplyr::count(estado)
```

Os estados são **calculados na análise** (nada é gravado no jogo ou no servidor) e aparecem nos dois
relatórios. São indicadores heurísticos: valide com observação em sala (BROMP) antes de tirar conclusões.

### Painel Shiny (direto do Supabase)

```r
fabulasLA::abrir_painel()   # conecta sozinho ao Supabase (credenciais em inst/painel/credenciais.R)
```

As credenciais ficam em `inst/painel/credenciais.R`, **fora do Git** porque contém a chave de leitura
(modelo em `credenciais_exemplo.R`). Depois de editá-las, reinstale o pacote. Ao abrir, o painel conecta e
carrega tudo sozinho: o monitor se atualiza a cada 15 s a 5 min, e as abas de grupo e de estudante recarregam
a coleta a cada 10 min.

Os dados são lidos **direto do Supabase**, sem baixar nem enviar arquivos. Três módulos:

| Módulo | Para quê |
|---|---|
| **Monitor da coleta** | Acompanhar a coleta enquanto as crianças jogam: sessões em andamento, encerradas, interrompidas ou sem dados novos, completude da sequência de eventos, dados faciais, alertas e os últimos eventos de cada sessão. Atualiza sozinho (15 s a 5 min). |
| **Estudo em grupo** | As análises do relatório de grupo com filtros (escola, turma, condição, série): qualidade, desempenho por fase, estratégias, domínio dos conceitos, estados afetivos, expressões e A/B. Botão para baixar o relatório completo em HTML. |
| **Estudante** | As análises do relatório individual do estudante escolhido: percurso, desempenho comparado ao grupo, tentativas, domínio dos conceitos, estados e expressões em cada ação. Botão para baixar o relatório individual. |

Para treinamento sem servidor: `abrir_painel(demonstracao = TRUE)` (dados simulados).

**Publicar no shinyapps.io.** O painel é autossuficiente: as funções de análise ficam copiadas em
`inst/painel/R/pacote_*.R` (e os dados de referência em `inst/painel/extdata/`), então o shinyapps.io só
precisa de pacotes do CRAN — o `fabulasLA` não está no CRAN e não pode ser instalado lá. Abra
`inst/painel/app.R` no RStudio e use o botão **Publish**, ou rode `Pesquisa/publicar_painel.R`.
Sempre que mudar uma função em `R/`, rode `analysis/sincronizar_painel.R` para atualizar a cópia
(um teste avisa se ela estiver desatualizada). O painel publicado não tem senha: qualquer pessoa com o
link vê os dados.

### Domínio dos conceitos (modelo de conhecimento)

```r
dom <- dominio_habilidades(coleta)
dom                 # parâmetros por conceito (L0, T, G, S, AUC com validação cruzada) e situação das crianças
dom$final           # estudante x conceito: probabilidade de domínio e situação (domina / em progresso / ...)
dom$trajetoria      # probabilidade atualizada a cada fase ou tentativa
mapa_habilidades()  # que fase exercita que conceito (copie o CSV, edite e passe em `mapa =`)
```

Cada conceito tem um modelo BKT (Corbett & Anderson, 1995) ajustado por máxima verossimilhança. Nas fases,
a evidência é o crédito parcial da **primeira tentativa** (posições certas antes de corrigir); na repetição,
o uso correto do laço; na depuração, se a nova tentativa melhorou. Nos dados simulados, o domínio estimado
tem correlação ≈ 0,8 com a habilidade verdadeira de cada criança. Aparece nos dois relatórios.

### Sem dados reais ainda?

```r
pasta <- simular_coleta(n_alunos = 100)   # dados fictícios no formato do jogo
gerar_relatorio(NULL)                     # relatório de demonstração (grupo)
gerar_relatorio_individual()              # demonstração do relatório individual
```

### Códigos de participante

```r
gerar_codigos(30)              # mesmo algoritmo do jogo
codigo_valido(c("K7M2X", "k7m2x", "K7M3X"))
```

## Funções

| Função | O que faz |
|---|---|
| `ler_coleta()` | Lê pastas e .zip, junta escolas, remove duplicatas de exportações e confere SHA-256 com o manifesto. |
| `ler_coleta_servidor()` | Baixa eventos e amostras faciais do Supabase (paginado), recria os arquivos e lê como `ler_coleta()`. |
| `status_servidor()` | Sessões do estudo no servidor: escola, condição, eventos, amostras, se terminou. |
| `validar_coleta()` | Relatório de qualidade com status ok / aviso / erro. |
| `tabelas_analise()` | Sessões, fases, tentativas e afeto por sessão. |
| `gerar_relatorio()` | **Estudo em grupo**: relatório HTML com validação, estudo/escolas, 20+ análises e comparação A/B. |
| `gerar_relatorio_individual()` | **Um estudante**: sessões, percurso, desempenho por fase comparado com o grupo, estratégia de resolução, erros recorrentes, engajamento e expressões faciais integradas a cada ação (escore z em relação ao próprio estudante). |
| `abrir_painel()` | Painel Shiny ligado ao Supabase: monitor da coleta em tempo real, estudo em grupo e estudante individual. |
| `dominio_habilidades()` / `mapa_habilidades()` | Domínio de cada conceito de pensamento computacional (sequência, repetição, classificação, algoritmos, raciocínio espacial, depuração) por estudante, com *Bayesian Knowledge Tracing*; mapa fase→conceito editável em `inst/extdata/mapa_habilidades.csv`. |
| `estados_faciais()` / `regras_estados()` | Estados de aprendizagem (confusão, frustração, tédio, engajamento, encantamento) a cada 10 s, combinando rosto e comportamento; regras versionadas e documentadas. Indicadores, não diagnósticos. |
| `afeto_por_evento()` | Expressão facial antes/depois de cada evento (janela de tempo), com escore z pela linha de base do participante. |
| `copiar_modelos_relatorio()` | Copia os dois `.Rmd` (grupo e individual) para uma pasta, para editar e rodar com *Knit* no RStudio. |
| `codebook()` / `exportar_codebook()` | Dicionário de variáveis a partir do catálogo de eventos do jogo. |
| `catalogo_eventos()` | O catálogo (mesmo arquivo usado no Unity). |
| `simular_coleta()` | Gera dados fictícios no formato do jogo (para testes e demonstrações). |
| `gerar_codigos()` / `codigo_valido()` | Códigos de participante com dígito verificador. |

## Testes

```r
testthat::test_dir("analysis/fabulasLA/tests/testthat", package = "fabulasLA", load_package = "installed")
```

Com a variável de ambiente `FABULAS_ZIP_TESTE` apontando para um `.zip` exportado pelo jogo, o teste de
leitura de exportações reais também é executado.

O catálogo em `inst/extdata/catalogo_eventos.json` é uma cópia de
`Assets/LearningAnalytics/catalogo_eventos.json`; um teste no Unity confere se os dois estão iguais.

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
gerar_relatorio("PILOTO26", "relatorio_PILOTO26.html", titulo_estudo = "Piloto 2026")
exportar_codebook("codebook_PILOTO26.csv")
```

### Sem dados reais ainda?

```r
pasta <- simular_coleta(n_alunos = 100)   # dados fictícios no formato do jogo
gerar_relatorio(NULL)                     # relatório de demonstração
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
| `validar_coleta()` | Relatório de qualidade com status ok / aviso / erro. |
| `tabelas_analise()` | Sessões, fases, tentativas e afeto por sessão. |
| `gerar_relatorio()` | Relatório HTML com validação, estudo/escolas, 20+ análises e comparação A/B. |
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

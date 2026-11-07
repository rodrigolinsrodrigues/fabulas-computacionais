#' Gera o relatório completo de learning analytics (HTML).
#'
#' @param caminhos pastas e/ou .zip exportados pelo jogo. Use NULL para gerar um relatório
#'   de demonstração com dados simulados.
#' @param arquivo arquivo HTML de saída
#' @param titulo_estudo subtítulo do relatório (ex.: "Estudo piloto — Escola A, 2026")
#' @param n_alunos,semente usados só com dados simulados
#' @param abrir abre o relatório no navegador ao terminar
#' @return caminho do HTML, invisível
gerar_relatorio <- function(caminhos = NULL, arquivo = "relatorio_fabulas.html", titulo_estudo = "",
                            n_alunos = 100, semente = 2026, abrir = interactive()) {
  for (p in c("rmarkdown", "knitr", "ggplot2", "lme4", "survival", "pROC", "ranger", "patchwork", "scales"))
    if (!requireNamespace(p, quietly = TRUE)) stop("Instale o pacote '", p, "' para gerar o relatório: install.packages(\"", p, "\")")
  modelo <- system.file("relatorio", "relatorio.Rmd", package = "fabulasLA")
  if (modelo == "") stop("Modelo do relatório não encontrado no pacote.")

  trabalho <- tempfile("relatorio_")
  dir.create(trabalho)
  file.copy(list.files(dirname(modelo), full.names = TRUE), trabalho)
  saida <- normalizePath(arquivo, mustWork = FALSE)
  params <- list(caminhos = if (is.null(caminhos)) "" else paste(normalizePath(caminhos, mustWork = TRUE), collapse = ";"),
                 n_alunos = n_alunos, semente = semente, titulo_estudo = titulo_estudo)
  rmarkdown::render(file.path(trabalho, "relatorio.Rmd"), params = params,
                    output_file = basename(saida), output_dir = dirname(saida),
                    intermediates_dir = trabalho, envir = new.env(parent = globalenv()), quiet = TRUE)
  if (abrir) utils::browseURL(saida)
  invisible(saida)
}

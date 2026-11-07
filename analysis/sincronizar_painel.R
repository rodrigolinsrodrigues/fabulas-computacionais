# Copia as funções do pacote fabulasLA para dentro da pasta do painel Shiny
# (analysis/fabulasLA/inst/painel), para que o painel funcione sozinho no shinyapps.io,
# que não consegue instalar o fabulasLA (ele não está no CRAN).
#
# Rode este script SEMPRE que mudar alguma função em analysis/fabulasLA/R/ e antes de publicar:
#   Rscript analysis/sincronizar_painel.R          (a partir da raiz do projeto)
# ou, no RStudio, abra o arquivo e clique em "Source".
# Um teste do pacote confere se as cópias estão em dia.

local({
  raiz <- if (requireNamespace("rstudioapi", quietly = TRUE) && rstudioapi::isAvailable() &&
              nzchar(rstudioapi::getSourceEditorContext()$path)) dirname(dirname(rstudioapi::getSourceEditorContext()$path)) else getwd()
  pacote <- file.path(raiz, "analysis", "fabulasLA")
  if (!dir.exists(pacote)) pacote <- file.path(raiz, "fabulasLA")
  painel <- file.path(pacote, "inst", "painel")
  stopifnot(dir.exists(pacote), dir.exists(painel))

  # Funções usadas pelo painel (relatorio.R e painel.R ficam de fora: dependem do pacote instalado)
  fora <- c("relatorio.R", "painel.R")
  origem <- setdiff(list.files(file.path(pacote, "R"), pattern = "[.]R$"), fora)
  antigos <- list.files(file.path(painel, "R"), pattern = "^pacote_.*[.]R$", full.names = TRUE)
  file.remove(antigos)
  for (f in origem) {
    txt <- readLines(file.path(pacote, "R", f), encoding = "UTF-8", warn = FALSE)
    txt <- gsub('system.file\\("extdata", "([^"]+)", package = "fabulasLA"\\)', 'file.path(pasta_extdata(), "\\1")', txt)
    txt <- gsub("fabulasLA:::?", "", txt)
    cab <- c("# GERADO AUTOMATICAMENTE por analysis/sincronizar_painel.R a partir de",
             paste0("# analysis/fabulasLA/R/", f, ". NÃO EDITE AQUI: edite o original e rode o script de novo."), "")
    writeLines(c(cab, txt), file.path(painel, "R", paste0("pacote_", f)), useBytes = TRUE)
  }
  dir.create(file.path(painel, "extdata"), showWarnings = FALSE)
  extdata <- list.files(file.path(pacote, "inst", "extdata"), full.names = TRUE)
  file.copy(extdata, file.path(painel, "extdata"), overwrite = TRUE)
  message("Painel sincronizado: ", length(origem), " arquivos de funções e ", length(extdata), " arquivos de dados.")
})

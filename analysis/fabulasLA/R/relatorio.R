#' Relatório do ESTUDO EM GRUPO (todos os estudantes) em HTML.
#'
#' Validação da coleta, caracterização da amostra, desempenho, engajamento, estado da solução,
#' expressões faciais, condições A/B e um painel individual de exemplo.
#'
#' @param caminhos pastas e/ou .zip exportados pelo jogo. NULL (e sem servidor) = dados simulados.
#' @param arquivo arquivo HTML de saída
#' @param titulo_estudo subtítulo do relatório (ex.: "Estudo piloto — Escola A, 2026")
#' @param servidor_url,chave_publica,estudo para ler direto do servidor central (Supabase); a chave
#'   de leitura vem da variável de ambiente `FABULAS_CHAVE_LEITURA` (ver [ler_coleta_servidor()])
#' @param n_alunos,semente usados só com dados simulados
#' @param abrir abre o relatório no navegador ao terminar
#' @return caminho do HTML, invisível
#' @export
gerar_relatorio <- function(caminhos = NULL, arquivo = "relatorio_fabulas.html", titulo_estudo = "",
                            servidor_url = "", chave_publica = "", estudo = "",
                            n_alunos = 100, semente = 2026, abrir = interactive()) {
  .verificar_pacotes(c("rmarkdown", "knitr", "ggplot2", "lme4", "survival", "pROC", "ranger", "patchwork", "scales"))
  params <- list(caminhos = .juntar_caminhos(caminhos), servidor_url = servidor_url, chave_publica = chave_publica,
                 estudo = estudo, n_alunos = n_alunos, semente = semente, titulo_estudo = titulo_estudo)
  .renderizar("relatorio.Rmd", params, arquivo, abrir)
}

#' Relatório INDIVIDUAL de um estudante em HTML.
#'
#' Sessões, percurso no jogo, desempenho em cada fase comparado com o grupo, como resolveu
#' (estado da solução a cada tentativa), erros recorrentes, engajamento e expressões faciais
#' integradas às ações (antes/depois de cada tentativa, por cena, em relação à linha de base
#' do próprio estudante).
#'
#' @param participante código do participante (ex.: "K7M2X"). NULL = escolhe automaticamente o
#'   estudante com mais tentativas (útil para a demonstração com dados simulados).
#' @inheritParams gerar_relatorio
#' @param arquivo arquivo HTML de saída (padrão: `relatorio_<participante>.html`)
#' @return caminho do HTML, invisível
#' @examples
#' \dontrun{
#' gerar_relatorio_individual("K7M2X", "PILOTO26")                  # pastas/.zip
#' gerar_relatorio_individual("K7M2X", servidor_url = "https://x.supabase.co",
#'                            chave_publica = "sb_publishable_...", estudo = "PILOTO26")
#' gerar_relatorio_individual()                                     # demonstração
#' }
#' @export
gerar_relatorio_individual <- function(participante = NULL, caminhos = NULL, arquivo = NULL,
                                       servidor_url = "", chave_publica = "", estudo = "",
                                       n_alunos = 30, semente = 2026, abrir = interactive()) {
  .verificar_pacotes(c("rmarkdown", "knitr", "ggplot2", "patchwork", "scales"))
  cod <- if (is.null(participante)) "" else .normalizar_codigo(participante)
  arquivo <- arquivo %||% paste0("relatorio_", if (nzchar(cod)) cod else "individual", ".html")
  params <- list(participante = cod, caminhos = .juntar_caminhos(caminhos), servidor_url = servidor_url,
                 chave_publica = chave_publica, estudo = estudo, n_alunos = n_alunos, semente = semente)
  .renderizar("relatorio_individual.Rmd", params, arquivo, abrir)
}

#' Copia os modelos R Markdown (grupo e individual) para uma pasta, para editar no RStudio.
#'
#' Os arquivos copiados funcionam sozinhos: abra no RStudio, ajuste os parâmetros no
#' cabeçalho (`params:`) e clique em *Knit*. Útil para acrescentar análises próprias.
#' @param pasta pasta de destino (criada se não existir)
#' @return os caminhos copiados, invisível
#' @export
copiar_modelos_relatorio <- function(pasta = "relatorios_fabulas") {
  origem <- system.file("relatorio", package = "fabulasLA")
  if (origem == "") stop("Modelos não encontrados no pacote.")
  dir.create(pasta, recursive = TRUE, showWarnings = FALSE)
  arqs <- list.files(origem, full.names = TRUE)
  file.copy(arqs, pasta, overwrite = FALSE)
  destino <- file.path(pasta, basename(arqs))
  message("Modelos copiados para ", normalizePath(pasta), ":\n  ",
          "relatorio.Rmd             -> estudo em grupo\n  ",
          "relatorio_individual.Rmd  -> um estudante\n",
          "Abra no RStudio, ajuste 'params:' no cabeçalho e clique em Knit.")
  invisible(destino)
}

.verificar_pacotes <- function(pacotes) {
  for (p in pacotes)
    if (!requireNamespace(p, quietly = TRUE)) stop("Instale o pacote '", p, "' para gerar o relatório: install.packages(\"", p, "\")")
}

.juntar_caminhos <- function(caminhos) {
  if (is.null(caminhos) || length(caminhos) == 0) "" else paste(normalizePath(caminhos, mustWork = TRUE), collapse = ";")
}

.renderizar <- function(modelo_nome, params, arquivo, abrir) {
  modelo <- system.file("relatorio", modelo_nome, package = "fabulasLA")
  if (modelo == "") stop("Modelo do relatório não encontrado no pacote: ", modelo_nome)
  trabalho <- tempfile("relatorio_")
  dir.create(trabalho)
  file.copy(list.files(dirname(modelo), full.names = TRUE), trabalho)
  saida <- normalizePath(arquivo, mustWork = FALSE)
  rmarkdown::render(file.path(trabalho, modelo_nome), params = params,
                    output_file = basename(saida), output_dir = dirname(saida),
                    intermediates_dir = trabalho, envir = new.env(parent = globalenv()), quiet = TRUE)
  if (abrir) utils::browseURL(saida)
  invisible(saida)
}

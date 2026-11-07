#' Abre o painel Shiny de learning analytics.
#'
#' Três módulos:
#' \describe{
#'   \item{Monitor da coleta}{o que já chegou ao servidor: sessões em andamento, encerradas,
#'     interrompidas ou sem dados novos, completude da sequência de eventos, dados faciais e os
#'     últimos eventos de cada sessão. Atualiza sozinho (15 s a 5 min).}
#'   \item{Estudo em grupo}{as análises do relatório de grupo, com filtros por escola, turma,
#'     condição e série, e download do relatório completo em HTML.}
#'   \item{Estudante}{as análises do relatório individual para o estudante escolhido, e
#'     download do relatório completo.}
#' }
#' O painel conecta sozinho ao Supabase ao abrir. As credenciais vêm, nesta ordem, dos argumentos
#' desta função, do arquivo `credenciais.R` na pasta do painel (`inst/painel/`, fora do Git; modelo
#' em `credenciais_exemplo.R`) ou das variáveis de ambiente `FABULAS_URL`, `FABULAS_CHAVE_PUBLICA`,
#' `FABULAS_ESTUDO` e `FABULAS_CHAVE_LEITURA`. O monitor se atualiza a cada 15 s a 5 min; as abas
#' de grupo e de estudante recarregam a coleta a cada 10 min.
#'
#' @param servidor_url,chave_publica,estudo,chave_leitura conexão com o Supabase
#' @param caminhos alternativamente, pastas ou .zip exportados pelo jogo
#' @param demonstracao `TRUE` abre com dados simulados (sem servidor), para treinamento
#' @param n_alunos número de estudantes no modo de demonstração
#' @param porta,abrir repassados a [shiny::runApp()]
#' @examples
#' \dontrun{
#' abrir_painel()                      # conecta com credenciais.R
#' abrir_painel(demonstracao = TRUE)    # dados simulados
#' abrir_painel(servidor_url = "https://<projeto>.supabase.co", chave_publica = "sb_publishable_...",
#'              estudo = "PILOTO26", chave_leitura = Sys.getenv("FABULAS_CHAVE_LEITURA"))
#' abrir_painel(caminhos = "C:/PILOTO26")
#' }
#' @export
abrir_painel <- function(servidor_url = NULL, chave_publica = NULL, estudo = NULL, chave_leitura = NULL,
                         caminhos = NULL, demonstracao = FALSE, n_alunos = 60, porta = getOption("shiny.port"), abrir = interactive()) {
  .verificar_pacotes(c("shiny", "bslib", "bsicons", "DT", "ggplot2", "scales"))
  app <- system.file("painel", package = "fabulasLA")
  if (app == "") stop("Painel não encontrado no pacote.")
  modo <- if (!is.null(servidor_url)) "servidor" else if (!is.null(caminhos)) "pasta" else if (isTRUE(demonstracao)) "demo" else NULL
  op <- options(fabulasLA.painel = list(modo = modo, servidor_url = servidor_url, chave_publica = chave_publica, estudo = estudo,
                                        chave_leitura = chave_leitura, caminhos = caminhos, n_alunos = n_alunos))
  on.exit(options(op), add = TRUE)
  shiny::runApp(app, port = porta, launch.browser = abrir)
}

# Painel de Learning Analytics — Fábulas Computacionais
# Abra no RStudio com o botão "Run App" neste arquivo (ou com abrir_painel() do pacote).
# O painel é autossuficiente: as funções de análise estão copiadas em R/pacote_*.R (geradas por
# analysis/sincronizar_painel.R), por isso pode ser publicado no shinyapps.io com o botão "Publish".
# Os arquivos da pasta R/ ao lado deste são carregados automaticamente pelo Shiny.
#
# Os dados vêm direto do Supabase, com as credenciais de credenciais.R (nesta pasta, fora do Git;
# modelo em credenciais_exemplo.R). Ao abrir, o painel conecta e carrega tudo sozinho:
#  - Monitor da coleta: resumo das sessões, atualizado a cada 15 s a 5 min (escolha na tela);
#  - Estudo em grupo e Estudante: coleta completa, recarregada a cada RECARGA_MIN minutos.

suppressPackageStartupMessages({
  library(shiny); library(bslib); library(dplyr); library(ggplot2)
  library(tidyr); library(purrr); library(tibble); library(stringr); library(readr)
})

RECARGA_MIN <- 10   # de quanto em quanto tempo o grupo e o estudante são recarregados do servidor

ui <- page_navbar(
  window_title = "Painel Fábulas Computacionais",
  title = tags$span(bsicons::bs_icon("controller"), " Fábulas Computacionais — Painel de Learning Analytics"),
  theme = bs_theme(version = 5, bootswatch = "flatly", "navbar-bg" = "#1F4E79"),
  fillable = FALSE,
  nav_panel("Monitor da coleta", icon = bsicons::bs_icon("broadcast"), monitorUI("monitor")),
  nav_panel("Estudo em grupo", icon = bsicons::bs_icon("people"), grupoUI("grupo")),
  nav_panel("Estudante", icon = bsicons::bs_icon("person-badge"), individualUI("individual")),
  nav_spacer(),
  nav_item(uiOutput("fonte_rotulo"))
)

server <- function(input, output, session) {
  fonte <- reactiveVal(fonte_padrao())
  analise <- reactiveVal(NULL)

  if (isolate(fonte())$modo == "sem_credenciais") {
    showModal(modalDialog(
      title = "Conexão com o Supabase não configurada", easyClose = FALSE, footer = NULL,
      p("O painel não encontrou as credenciais do servidor. Crie o arquivo ", code("credenciais.R"),
        " na pasta do painel (copie ", code("credenciais_exemplo.R"), "), preencha a chave de leitura e abra o painel de novo."),
      p(class = "text-muted small", "Pasta do painel: ", code(normalizePath(getwd())))
    ))
  }

  carregar <- function(silencioso = FALSE) {
    f <- fonte()
    if (f$modo == "sem_credenciais") return(invisible())
    corpo <- function(prog) {
      tryCatch({
        prog(0.1, "baixando eventos e amostras faciais do Supabase")
        coleta <- carregar_coleta(f)
        a <- preparar_analise(coleta, prog)
        analise(a)
        if (!silencioso) showNotification(paste0("Dados carregados: ", n_distinct(coleta$eventos$participant), " estudantes, ",
                                                 format(nrow(coleta$eventos), big.mark = "."), " eventos."), type = "message")
      }, error = function(e) showNotification(paste("Não foi possível carregar os dados do servidor:", conditionMessage(e)),
                                              type = "error", duration = 20))
    }
    if (silencioso) corpo(function(v, msg) NULL)
    else withProgress(message = "Carregando a coleta…", value = 0.05, corpo(function(v, msg) setProgress(v, detail = msg)))
  }

  # Carga ao abrir e recarga periódica (o monitor tem a própria atualização, mais frequente)
  observeEvent(TRUE, carregar(), once = TRUE)
  observe({
    invalidateLater(RECARGA_MIN * 60 * 1000)
    if (!is.null(isolate(analise()))) carregar(silencioso = TRUE)
  })

  output$fonte_rotulo <- renderUI(tags$span(class = "navbar-text small text-light me-2",
                                            bsicons::bs_icon("hdd-network"), " ", descricao_fonte(fonte()),
                                            if (!is.null(analise())) paste0(" · análise de ", format(analise()$carregado_em, "%H:%M"),
                                                                            " (recarrega a cada ", RECARGA_MIN, " min)")))

  monitorServer("monitor", fonte, analise)
  grupoServer("grupo", analise, fonte)
  individualServer("individual", analise, fonte)
}

shinyApp(ui, server)

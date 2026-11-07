# Módulo 1 — Monitor da coleta: o que já chegou ao servidor, em tempo (quase) real.

monitorUI <- function(id) {
  ns <- shiny::NS(id)
  bslib::layout_sidebar(
    sidebar = bslib::sidebar(
      width = 280, title = "Atualização",
      shiny::selectInput(ns("intervalo"), "Atualizar automaticamente", c("a cada 15 s" = 15, "a cada 30 s" = 30, "a cada 1 min" = 60,
                                                                          "a cada 5 min" = 300, "desligado" = 0), selected = 30),
      shiny::actionButton(ns("agora"), "Atualizar agora", icon = shiny::icon("rotate"), class = "btn-primary w-100"),
      shiny::uiOutput(ns("hora")),
      shiny::hr(),
      shiny::numericInput(ns("limite_min"), "Alertar sessão aberta sem dados novos há (min)", 10, min = 1, max = 120),
      shiny::selectInput(ns("escola"), "Escola", "Todas"),
      shiny::selectInput(ns("status"), "Situação", c("Todas", "em andamento", "sem notícias", "encerrada", "interrompida"))
    ),
    bslib::layout_column_wrap(
      width = 1 / 6, fill = FALSE,
      bslib::value_box("Sessões", shiny::textOutput(ns("vb_sessoes")), showcase = bsicons::bs_icon("collection-play"), theme = "primary"),
      bslib::value_box("Em andamento", shiny::textOutput(ns("vb_andamento")), showcase = bsicons::bs_icon("broadcast"), theme = "success"),
      bslib::value_box("Estudantes", shiny::textOutput(ns("vb_estudantes")), showcase = bsicons::bs_icon("people"), theme = "info"),
      bslib::value_box("Eventos", shiny::textOutput(ns("vb_eventos")), showcase = bsicons::bs_icon("list-check"), theme = "secondary"),
      bslib::value_box("Amostras faciais", shiny::textOutput(ns("vb_amostras")), showcase = bsicons::bs_icon("camera-video"), theme = "secondary"),
      bslib::value_box("Último dado", shiny::textOutput(ns("vb_ultimo")), showcase = bsicons::bs_icon("clock-history"), theme = "light")
    ),
    shiny::uiOutput(ns("alertas")),
    bslib::layout_columns(
      col_widths = c(8, 4),
      bslib::card(full_screen = TRUE, bslib::card_header("Chegada dos dados por sessão"),
                  shiny::plotOutput(ns("g_linha"), height = "330px")),
      bslib::card(full_screen = TRUE, bslib::card_header("Sessões por escola e situação"),
                  shiny::plotOutput(ns("g_escolas"), height = "330px"))
    ),
    bslib::card(full_screen = TRUE,
                bslib::card_header("Sessões recebidas", shiny::span(class = "text-muted small ms-2", "clique numa linha para ver os eventos da sessão")),
                DT::DTOutput(ns("tabela"))),
    shiny::uiOutput(ns("detalhe"))
  )
}

monitorServer <- function(id, fonte, analise) {
  shiny::moduleServer(id, function(input, output, session) {
    gatilho <- shiny::reactiveVal(0)
    shiny::observeEvent(input$agora, gatilho(gatilho() + 1))
    shiny::observe({
      seg <- as.numeric(input$intervalo)
      if (seg > 0) { shiny::invalidateLater(seg * 1000); shiny::isolate(gatilho(gatilho() + 1)) }
    })

    sessoes <- shiny::reactive({
      gatilho(); f <- fonte()
      r <- tryCatch(visao_sessoes(f), error = function(e) structure(sessoes_vazias(), erro = conditionMessage(e)))
      attr(r, "hora") <- Sys.time()
      r
    })

    classificada <- shiny::reactive({
      s <- sessoes(); lim <- as.numeric(input$limite_min %||% 10) * 60
      agora <- if (fonte()$modo == "demo") max(s$ultimo, na.rm = TRUE) else Sys.time()
      s |> dplyr::mutate(
        situacao = dplyr::case_when(.data$encerrada & .data$motivo %in% "interrupted" ~ "interrompida",
                                    .data$encerrada ~ "encerrada",
                                    as.numeric(difftime(agora, .data$ultimo, units = "secs")) > lim ~ "sem notícias",
                                    TRUE ~ "em andamento"),
        completude = ifelse(.data$ultimo_seq > 0, pmin(1, .data$eventos / .data$ultimo_seq), NA),
        minutos = as.numeric(difftime(.data$ultimo, .data$primeiro, units = "mins")),
        parado_min = as.numeric(difftime(agora, .data$ultimo, units = "mins")))
    })

    shiny::observe({
      esc <- sort(unique(stats::na.omit(sessoes()$escola)))
      shiny::updateSelectInput(session, "escola", choices = c("Todas", esc), selected = shiny::isolate(input$escola) %||% "Todas")
    })

    filtrada <- shiny::reactive({
      s <- classificada()
      if (!is.null(input$escola) && input$escola != "Todas") s <- s[s$escola %in% input$escola, ]
      if (!is.null(input$status) && input$status != "Todas") s <- s[s$situacao == input$status, ]
      s
    })

    output$hora <- shiny::renderUI({
      s <- sessoes(); err <- attr(s, "erro")
      if (!is.null(err)) return(shiny::div(class = "alert alert-danger mt-2 small", shiny::strong("Erro ao consultar: "), err))
      shiny::div(class = "text-muted small mt-2", "Atualizado às ", format(attr(s, "hora"), "%H:%M:%S"), shiny::br(), descricao_fonte(fonte()))
    })

    output$vb_sessoes <- shiny::renderText(num(nrow(filtrada()), 0))
    output$vb_andamento <- shiny::renderText(num(sum(filtrada()$situacao == "em andamento"), 0))
    output$vb_estudantes <- shiny::renderText(num(dplyr::n_distinct(filtrada()$participant), 0))
    output$vb_eventos <- shiny::renderText(num(sum(filtrada()$eventos, na.rm = TRUE), 0))
    output$vb_amostras <- shiny::renderText(num(sum(filtrada()$amostras, na.rm = TRUE), 0))
    output$vb_ultimo <- shiny::renderText({
      s <- filtrada(); if (nrow(s) == 0) return("—")
      if (fonte()$modo == "demo") return(format(max(s$ultimo), "%d/%m %H:%M"))
      m <- as.numeric(difftime(Sys.time(), max(s$ultimo), units = "mins"))
      if (m < 1) "agora" else if (m < 60) paste0("há ", round(m), " min") else format(max(s$ultimo), "%d/%m %H:%M")
    })

    output$alertas <- shiny::renderUI({
      s <- classificada(); itens <- list()
      sem <- s[s$situacao == "sem notícias", ]
      if (nrow(sem)) itens <- c(itens, list(shiny::tags$li(shiny::strong(nrow(sem), " sessão(ões) sem dados novos"), " — aberta(s) e sem envio há mais de ",
        input$limite_min, " min (internet caiu, jogo fechado sem sincronizar ou estudante parado): ",
        paste(utils::head(paste0(sem$participant, " (", sem$maquina %||% "?", ")"), 6), collapse = ", "),
        ". Os dados continuam no computador e serão enviados depois.")))
      inc <- s[!is.na(s$completude) & s$completude < 0.98 & s$situacao != "em andamento", ]
      if (nrow(inc)) itens <- c(itens, list(shiny::tags$li(shiny::strong(nrow(inc), " sessão(ões) incompleta(s)"),
        " — faltam eventos na sequência (ainda chegando ou aguardando nova sincronização): ",
        paste(utils::head(inc$participant, 6), collapse = ", "), ".")))
      cam <- s[s$encerrada & s$amostras == 0 & s$eventos > 20, ]
      if (nrow(cam) && any(s$amostras > 0)) itens <- c(itens, list(shiny::tags$li(shiny::strong(nrow(cam), " sessão(ões) sem dados faciais"),
        " enquanto outras têm — confira a webcam ou o consentimento nessas máquinas: ",
        paste(utils::head(unique(stats::na.omit(cam$maquina)), 6), collapse = ", "), ".")))
      itr <- s[s$situacao == "interrompida", ]
      if (nrow(itr)) itens <- c(itens, list(shiny::tags$li(shiny::strong(nrow(itr), " sessão(ões) interrompida(s)"),
        " (queda de energia ou travamento; o fim foi reconstruído na abertura seguinte).")))
      if (length(itens) == 0) return(shiny::div(class = "alert alert-success py-2", shiny::icon("circle-check"), " Nenhum problema detectado na coleta."))
      shiny::div(class = "alert alert-warning py-2", shiny::strong(shiny::icon("triangle-exclamation"), " Atenção"), shiny::tags$ul(class = "mb-0", itens))
    })

    output$g_linha <- shiny::renderPlot({
      s <- filtrada()
      if (nrow(s) == 0) return(vazio("Nenhuma sessão recebida ainda"))
      s <- s |> dplyr::arrange(.data$primeiro) |> utils::tail(40) |>
        dplyr::mutate(rotulo = factor(paste(.data$participant, "·", .data$maquina %||% ""), levels = unique(paste(.data$participant, "·", .data$maquina %||% ""))))
      ggplot2::ggplot(s, ggplot2::aes(y = .data$rotulo)) +
        ggplot2::geom_segment(ggplot2::aes(x = .data$primeiro, xend = .data$ultimo, yend = .data$rotulo, color = .data$situacao), linewidth = 4, lineend = "round") +
        ggplot2::geom_point(ggplot2::aes(x = .data$ultimo, color = .data$situacao), size = 3) +
        ggplot2::scale_color_manual(values = c(`em andamento` = "#2CA02C", `sem notícias` = "#E0A800", encerrada = "#1F4E79", interrompida = "#C00000")) +
        ggplot2::labs(x = NULL, y = NULL, color = NULL, subtitle = "do primeiro ao último dado recebido (até 40 sessões mais recentes)") + tema
    })

    output$g_escolas <- shiny::renderPlot({
      s <- filtrada()
      if (nrow(s) == 0) return(vazio("—"))
      s$escola[is.na(s$escola) | s$escola == ""] <- "(sem escola)"
      ggplot2::ggplot(s, ggplot2::aes(y = .data$escola, fill = .data$situacao)) + ggplot2::geom_bar() +
        ggplot2::scale_fill_manual(values = c(`em andamento` = "#2CA02C", `sem notícias` = "#E0A800", encerrada = "#1F4E79", interrompida = "#C00000")) +
        ggplot2::labs(x = "Sessões", y = NULL, fill = NULL) + tema
    })

    output$tabela <- DT::renderDT({
      s <- filtrada() |> dplyr::arrange(dplyr::desc(.data$ultimo)) |>
        dplyr::transmute(Situação = .data$situacao, Participante = .data$participant, Escola = .data$escola, Turma = .data$turma,
                         Máquina = .data$maquina, Condição = .data$condicao, Eventos = .data$eventos,
                         `Completude` = ifelse(is.na(.data$completude), "—", pct(.data$completude)),
                         `Amostras faciais` = .data$amostras, `Duração (min)` = round(.data$minutos, 1),
                         `Último dado` = format(.data$ultimo, "%d/%m %H:%M:%S"), Sessão = .data$session)
      tabela_dt(s, n = 15, selection = "single") |>
        DT::formatStyle("Situação", backgroundColor = DT::styleEqual(c("em andamento", "sem notícias", "encerrada", "interrompida"),
                                                                      c("#E3F4E6", "#FFF3CD", "#E7EEF7", "#F8D7DA")))
    })

    output$detalhe <- shiny::renderUI({
      i <- input$tabela_rows_selected
      if (is.null(i)) return(NULL)
      s <- filtrada() |> dplyr::arrange(dplyr::desc(.data$ultimo))
      sel <- s[i, ]
      ev <- tryCatch(eventos_da_sessao(fonte(), sel$session, if (!is.null(analise())) analise()$coleta), error = function(e) tibble::tibble(erro = conditionMessage(e)))
      ultimos <- utils::head(ev, 40)
      contagem <- if ("event" %in% names(ev)) ev |> dplyr::count(.data$event, sort = TRUE) else tibble::tibble()
      bslib::card(
        bslib::card_header(shiny::strong("Sessão ", sel$session), " — ", sel$participant, " · ", sel$escola, " · ", sel$maquina),
        bslib::layout_columns(col_widths = c(8, 4),
          shiny::div(shiny::h6("Últimos 40 eventos recebidos"), shiny::renderTable(ultimos |> dplyr::select(dplyr::any_of(c("seq", "t", "scene", "event", "detalhe"))), striped = TRUE, spacing = "xs")),
          shiny::div(shiny::h6("Eventos por tipo"), shiny::renderTable(contagem, striped = TRUE, spacing = "xs")))
      )
    })

    classificada
  })
}

# Módulo 3 — Estudante (baseado no relatório individual).

individualUI <- function(id) {
  ns <- shiny::NS(id)
  bslib::layout_sidebar(
    sidebar = bslib::sidebar(
      width = 300, title = "Estudante",
      shiny::selectizeInput(ns("participante"), "Código do participante", choices = NULL,
                            options = list(placeholder = "Digite ou escolha um código")),
      shiny::uiOutput(ns("ficha")),
      shiny::hr(),
      shiny::uiOutput(ns("botao_relatorio"))
    ),
    shiny::uiOutput(ns("aviso")),
    bslib::layout_column_wrap(
      width = 1 / 6, fill = FALSE,
      bslib::value_box("Sessões", shiny::textOutput(ns("vb_ses")), showcase = bsicons::bs_icon("collection-play"), theme = "primary"),
      bslib::value_box("Tempo de jogo", shiny::textOutput(ns("vb_tempo")), showcase = bsicons::bs_icon("stopwatch"), theme = "info"),
      bslib::value_box("Fases concluídas", shiny::textOutput(ns("vb_concl")), showcase = bsicons::bs_icon("trophy"), theme = "success"),
      bslib::value_box("Fases abandonadas", shiny::textOutput(ns("vb_aband")), showcase = bsicons::bs_icon("door-open"), theme = "danger"),
      bslib::value_box("Tentativas por fase", shiny::textOutput(ns("vb_tent")), showcase = bsicons::bs_icon("arrow-repeat"), theme = "secondary"),
      bslib::value_box("Conceitos dominados", shiny::textOutput(ns("vb_dom")), showcase = bsicons::bs_icon("mortarboard"), theme = "light")
    ),
    shiny::uiOutput(ns("destaques")),
    bslib::navset_card_tab(
      full_screen = TRUE,
      bslib::nav_panel("Percurso", shiny::plotOutput(ns("g_percurso"), height = "300px")),
      bslib::nav_panel("Desempenho x grupo", bslib::layout_columns(col_widths = c(7, 5), shiny::plotOutput(ns("g_desempenho"), height = "420px"), DT::DTOutput(ns("t_fases")))),
      bslib::nav_panel("Como resolveu", shiny::plotOutput(ns("g_curvas"), height = "380px"), DT::DTOutput(ns("t_tentativas"))),
      bslib::nav_panel("Domínio dos conceitos", bslib::layout_columns(col_widths = c(6, 6), shiny::plotOutput(ns("g_dominio"), height = "360px"), shiny::plotOutput(ns("g_dom_traj"), height = "360px"))),
      bslib::nav_panel("Estados afetivos", shiny::plotOutput(ns("g_estados_tempo"), height = "260px"), shiny::plotOutput(ns("g_estados_dist"), height = "220px")),
      bslib::nav_panel("Expressões e ações", shiny::plotOutput(ns("g_afeto"), height = "340px"), DT::DTOutput(ns("t_acoes")))
    )
  )
}

individualServer <- function(id, analise, fonte) {
  shiny::moduleServer(id, function(input, output, session) {
    shiny::observe({
      a <- analise(); shiny::req(a)
      s <- a$tb$sessoes |> dplyr::group_by(.data$participant) |> dplyr::summarise(rot = paste0(dplyr::first(.data$participant), " — ", dplyr::first(.data$escola), " ", dplyr::first(.data$turma)), .groups = "drop")
      escolhido <- shiny::isolate(input$participante)
      # sugestão: o estudante com mais tentativas
      sug <- a$tb$fases |> dplyr::count(.data$participant, wt = .data$tentativas) |> dplyr::slice_max(.data$n, n = 1, with_ties = FALSE) |> dplyr::pull(.data$participant)
      shiny::updateSelectizeInput(session, "participante", choices = stats::setNames(s$participant, s$rot),
                                  selected = if (!is.null(escolhido) && escolhido %in% s$participant) escolhido else sug, server = TRUE)
    })

    d <- shiny::reactive({
      a <- analise(); shiny::req(a, input$participante)
      p <- input$participante
      ev <- a$coleta$eventos[a$coleta$eventos$participant == p, ]
      list(p = p, ses = a$tb$sessoes[a$tb$sessoes$participant == p, ], fases = a$tb$fases[a$tb$fases$participant == p, ],
           tent = a$tb$tentativas[a$tb$tentativas$participant == p, ], ev = ev,
           af = if (nrow(a$coleta$afeto)) a$coleta$afeto[a$coleta$afeto$participant == p, ] else tibble::tibble(),
           dom = if (!is.null(a$dom)) a$dom$final[a$dom$final$participant == p, ], dom_traj = if (!is.null(a$dom)) a$dom$trajetoria[a$dom$trajetoria$participant == p, ],
           est = if (!is.null(a$est)) a$est[a$est$participant == p, ], grupo = a)
    })

    output$aviso <- shiny::renderUI({
      if (is.null(analise())) shiny::div(class = "alert alert-info", shiny::icon("spinner", class = "fa-spin"), " Carregando a coleta do servidor…")
    })
    output$ficha <- shiny::renderUI({
      x <- d(); s <- x$ses; shiny::req(nrow(s) > 0)
      shiny::div(class = "small", shiny::strong("Escola: "), s$escola[1], shiny::br(), shiny::strong("Turma: "), s$turma[1], shiny::br(),
                 shiny::strong("Série / idade: "), s$serie[1], " · ", s$idade[1], " anos", shiny::br(),
                 shiny::strong("Condição: "), s$condicao[1] %||% "—", shiny::br(), shiny::strong("Dispositivo: "), s$dispositivo[1])
    })

    output$vb_ses <- shiny::renderText(nrow(d()$ses))
    output$vb_tempo <- shiny::renderText(paste(num(sum(d()$ses$duracao_min, na.rm = TRUE), 0), "min"))
    output$vb_concl <- shiny::renderText(paste0(sum(d()$fases$concluiu), " de ", nrow(d()$fases)))
    output$vb_aband <- shiny::renderText(sum(d()$fases$abandonou, na.rm = TRUE))
    output$vb_tent <- shiny::renderText({ x <- d()$fases; x <- x[x$concluiu, ]; if (nrow(x) == 0) "—" else num(mean(x$tentativas), 1) })
    output$vb_dom <- shiny::renderText({ x <- d()$dom; if (is.null(x) || nrow(x) == 0) "—" else paste0(sum(x$situacao == "domina"), " de ", nrow(x)) })

    output$destaques <- shiny::renderUI({
      x <- d(); itens <- list()
      if (!is.null(x$dom) && nrow(x$dom)) {
        dom <- x$dom$rotulo[x$dom$situacao == "domina"]; fraco <- x$dom$rotulo[x$dom$situacao == "ainda não domina"]
        if (length(dom)) itens <- c(itens, list(shiny::tags$li(shiny::strong("Domina: "), paste(dom, collapse = ", "))))
        if (length(fraco)) itens <- c(itens, list(shiny::tags$li(shiny::strong("Precisa de apoio em: "), paste(fraco, collapse = ", "))))
      }
      t <- x$tent[!is.na(x$tent$n_changed) & x$tent$n_changed >= 0 & x$tent$attempt > 1 & !x$tent$success, ]
      if (nrow(t) >= 3) itens <- c(itens, list(shiny::tags$li(shiny::strong("Estratégia: "),
        if (mean(t$n_changed <= 2) >= .6) "depuração — corrige poucas posições por vez" else "tentativa e erro — muda muitas posições de uma vez")))
      ab <- x$fases$scene[which(x$fases$abandonou)]
      if (length(ab)) itens <- c(itens, list(shiny::tags$li(shiny::strong("Abandonou: "), paste(ab, collapse = ", "))))
      if (!is.null(x$est) && nrow(x$est)) {
        e <- x$est[x$est$estado != "neutro", ]
        if (nrow(e)) { top <- names(sort(table(e$estado), decreasing = TRUE))[1]
          itens <- c(itens, list(shiny::tags$li(shiny::strong("Estado mais frequente (fora o neutro): "), nomes_estado[top], " — indicador, não diagnóstico"))) }
      }
      if (length(itens) == 0) return(NULL)
      shiny::div(class = "alert alert-light border py-2", shiny::tags$ul(class = "mb-0", itens))
    })

    cenas <- shiny::reactive({
      ev <- d()$ev
      ev |> dplyr::filter(.data$event %in% c("scene_enter", "session_end")) |> dplyr::group_by(.data$session) |> dplyr::arrange(.data$t_ms, .by_group = TRUE) |>
        dplyr::mutate(fim = dplyr::lead(.data$t_ms)) |> dplyr::ungroup() |> dplyr::filter(.data$event == "scene_enter", !is.na(.data$fim)) |>
        dplyr::transmute(session = .data$session, scene = .data$scene, ini = .data$t_ms / 60000, fim = .data$fim / 60000, tipo = puzzle_de(.data$scene))
    })

    output$g_percurso <- shiny::renderPlot({
      ce <- cenas(); t <- d()$tent
      if (nrow(ce) == 0) return(vazio("Sem cenas registradas"))
      ggplot2::ggplot(ce) + ggplot2::geom_rect(ggplot2::aes(xmin = .data$ini, xmax = .data$fim, ymin = 0, ymax = 1, fill = .data$tipo), alpha = .45, color = "white") +
        ggplot2::geom_text(data = ce[ce$fim - ce$ini > 2.5, ], ggplot2::aes(x = (.data$ini + .data$fim) / 2, y = .85, label = sub(" Fase ", " ", .data$scene)), size = 3) +
        ggplot2::geom_point(data = t, ggplot2::aes(.data$t_ms / 60000, .4, color = .data$success), size = 2.6) +
        ggplot2::scale_fill_manual(values = cores_puzzle, na.value = "grey85") +
        ggplot2::scale_color_manual(values = c(`TRUE` = "#2CA02C", `FALSE` = "#C00000"), labels = c(`TRUE` = "acerto", `FALSE` = "erro")) +
        ggplot2::facet_wrap(~session, ncol = 1, scales = "free_x") +
        ggplot2::labs(x = "Minutos de sessão", y = NULL, fill = NULL, color = NULL) + tema +
        ggplot2::theme(axis.text.y = ggplot2::element_blank(), panel.grid = ggplot2::element_blank())
    })

    output$g_desempenho <- shiny::renderPlot({
      x <- d(); g <- x$grupo$tb$fases; g <- g[g$concluiu & g$scene %in% x$fases$scene, ]
      if (nrow(g) == 0) return(vazio("Sem fases concluídas"))
      ggplot2::ggplot(g, ggplot2::aes(.data$tentativas, stats::reorder(.data$scene, dplyr::desc(.data$scene)))) +
        ggplot2::geom_boxplot(fill = "grey90", outlier.alpha = .3) +
        ggplot2::geom_point(data = x$fases[x$fases$concluiu, ], color = "#C00000", size = 3.5) +
        ggplot2::labs(title = "Tentativas até concluir (vermelho: estudante; caixas: grupo)", x = "Tentativas", y = NULL) + tema
    })
    output$t_fases <- DT::renderDT({
      x <- d(); g <- x$grupo$tb$fases
      med <- g[g$concluiu, ] |> dplyr::group_by(.data$scene) |> dplyr::summarise(mediana = stats::median(.data$tentativas), .groups = "drop")
      tabela_dt(x$fases |> dplyr::arrange(.data$t_ini) |> dplyr::left_join(med, by = "scene") |>
                  dplyr::transmute(Fase = .data$scene, Tentativas = .data$tentativas, `Mediana grupo` = .data$mediana,
                                   `Tempo (min)` = round(.data$tempo_s / 60, 1),
                                   Resultado = dplyr::case_when(.data$concluiu ~ "✅", .data$abandonou ~ "❌ abandonou", TRUE ~ "—")), n = 12)
    })

    output$g_curvas <- shiny::renderPlot({
      t <- d()$tent; t <- t[!is.na(t$dist_rel), ]
      if (nrow(t) == 0) return(vazio("Sem tentativas"))
      ggplot2::ggplot(t, ggplot2::aes(.data$attempt, .data$dist_rel, color = .data$puzzle)) + ggplot2::geom_line(linewidth = 1) +
        ggplot2::geom_point(ggplot2::aes(shape = .data$success), size = 2.6) + ggplot2::facet_wrap(~scene) +
        ggplot2::scale_color_manual(values = cores_puzzle, guide = "none") + ggplot2::scale_shape_manual(values = c(`TRUE` = 16, `FALSE` = 1), labels = c(`TRUE` = "acerto", `FALSE` = "erro")) +
        ggplot2::scale_y_continuous(labels = scales::percent) + ggplot2::labs(x = "Tentativa", y = "Posições erradas", shape = NULL) + tema
    })
    output$t_tentativas <- DT::renderDT({
      txt <- function(x) vapply(x, function(v) paste(unlist(v), collapse = " · "), "")
      t <- d()$tent
      tabela_dt(t |> dplyr::transmute(Fase = .data$scene, `#` = .data$attempt, Montou = txt(.data$slots), Esperado = txt(.data$expected),
                                      `Posições erradas` = .data$distance, Acerto = ifelse(.data$success, "✅", "❌")), n = 8)
    })

    output$g_dominio <- shiny::renderPlot({
      x <- d(); dm <- x$dom
      if (is.null(dm) || nrow(dm) == 0) return(vazio("Sem dados para o modelo de domínio"))
      med <- x$grupo$dom$final |> dplyr::group_by(.data$habilidade) |> dplyr::summarise(mediana = stats::median(.data$p_dominio), .groups = "drop")
      dm <- dm |> dplyr::left_join(med, by = "habilidade")
      ggplot2::ggplot(dm, ggplot2::aes(.data$p_dominio, stats::reorder(.data$rotulo, .data$p_dominio))) +
        ggplot2::geom_col(ggplot2::aes(fill = .data$situacao), width = .6) +
        ggplot2::geom_point(ggplot2::aes(x = .data$mediana), shape = 124, size = 7) +
        ggplot2::geom_vline(xintercept = .95, linetype = 2, color = "grey40") +
        ggplot2::scale_fill_manual(values = cores_sit) + ggplot2::scale_x_continuous(labels = scales::percent, limits = c(0, 1)) +
        ggplot2::labs(title = "Probabilidade de dominar cada conceito", subtitle = "traço: mediana do grupo · tracejado: 95%", x = NULL, y = NULL, fill = NULL) + tema
    })
    output$g_dom_traj <- shiny::renderPlot({
      x <- d(); tr <- x$dom_traj
      if (is.null(tr) || nrow(tr) == 0) return(vazio("—"))
      tr <- tr |> dplyr::left_join(x$grupo$dom$mapa |> dplyr::select("habilidade", "rotulo"), by = "habilidade")
      ggplot2::ggplot(tr, ggplot2::aes(.data$ordem, .data$p_dominio)) + ggplot2::geom_hline(yintercept = .95, linetype = 2, color = "grey50") +
        ggplot2::geom_line(color = "#1F4E79", linewidth = .9) + ggplot2::geom_point(ggplot2::aes(color = .data$correto), size = 2) +
        ggplot2::scale_color_gradient(low = "#C00000", high = "#2CA02C", limits = c(0, 1), labels = scales::percent) +
        ggplot2::scale_y_continuous(labels = scales::percent, limits = c(0, 1)) + ggplot2::facet_wrap(~rotulo, scales = "free_x") +
        ggplot2::labs(title = "Evolução a cada oportunidade", x = "Oportunidade", y = NULL, color = "Acerto") + tema
    })

    output$g_estados_tempo <- shiny::renderPlot({
      e <- d()$est; t <- d()$tent
      if (is.null(e) || nrow(e) == 0) return(vazio("Sem dados"))
      ggplot2::ggplot(e) + ggplot2::geom_rect(ggplot2::aes(xmin = .data$t_ini / 60000, xmax = .data$t_fim / 60000, ymin = 0, ymax = 1, fill = .data$estado)) +
        ggplot2::geom_point(data = t, ggplot2::aes(.data$t_ms / 60000, 1.12, color = .data$success), size = 1.8) +
        ggplot2::scale_fill_manual(values = cores_estado, labels = nomes_estado) +
        ggplot2::scale_color_manual(values = c(`TRUE` = "#2CA02C", `FALSE` = "#C00000"), labels = c(`TRUE` = "acerto", `FALSE` = "erro")) +
        ggplot2::facet_wrap(~session, ncol = 1, scales = "free_x") +
        ggplot2::labs(title = "Estado predominante a cada 10 s (pontos: tentativas)", x = "Minutos de sessão", y = NULL, fill = NULL, color = NULL) + tema +
        ggplot2::theme(axis.text.y = ggplot2::element_blank(), panel.grid = ggplot2::element_blank())
    })
    output$g_estados_dist <- shiny::renderPlot({
      x <- d(); e <- x$est; g <- x$grupo$est
      if (is.null(e) || nrow(e) == 0) return(vazio("—"))
      meu <- e |> dplyr::count(.data$estado) |> dplyr::mutate(p = .data$n / sum(.data$n), quem = "Estudante")
      grp <- g |> dplyr::count(.data$estado) |> dplyr::mutate(p = .data$n / sum(.data$n), quem = "Grupo")
      dplyr::bind_rows(meu, grp) |> dplyr::mutate(estado = factor(.data$estado, names(cores_estado))) |>
        ggplot2::ggplot(ggplot2::aes(.data$p, .data$quem, fill = .data$estado)) + ggplot2::geom_col(position = ggplot2::position_stack(reverse = TRUE), width = .6) +
        ggplot2::scale_fill_manual(values = cores_estado, labels = nomes_estado) + ggplot2::scale_x_continuous(labels = scales::percent) +
        ggplot2::labs(x = NULL, y = NULL, fill = NULL) + tema
    })

    output$g_afeto <- shiny::renderPlot({
      x <- d(); af <- x$af
      if (nrow(af) == 0 || !any(af$face_present == 1, na.rm = TRUE)) return(vazio("Sem dados faciais para este estudante"))
      r <- af[af$face_present == 1, ]
      for (i in names(nomes_indice)) r[[i]] <- (r[[i]] - mean(r[[i]], na.rm = TRUE)) / stats::sd(r[[i]], na.rm = TRUE)
      r <- r |> dplyr::group_by(.data$session) |> dplyr::arrange(.data$t_ms, .by_group = TRUE) |>
        dplyr::mutate(dplyr::across(dplyr::all_of(names(nomes_indice)), ~ as.numeric(stats::filter(tidyr::replace_na(.x, 0), rep(1 / 15, 15), sides = 1)))) |>
        dplyr::ungroup() |> tidyr::pivot_longer(dplyr::all_of(names(nomes_indice))) |> dplyr::mutate(name = nomes_indice[.data$name])
      ggplot2::ggplot(r, ggplot2::aes(.data$t_ms / 60000, .data$value, color = .data$name)) +
        ggplot2::geom_rect(data = cenas(), ggplot2::aes(xmin = .data$ini, xmax = .data$fim, ymin = -Inf, ymax = Inf, fill = .data$tipo), inherit.aes = FALSE, alpha = .12) +
        ggplot2::geom_hline(yintercept = 0, color = "grey50") + ggplot2::geom_line(linewidth = .6, na.rm = TRUE) +
        ggplot2::geom_point(data = x$tent, ggplot2::aes(.data$t_ms / 60000, -2.2, shape = .data$success), inherit.aes = FALSE, size = 2) +
        ggplot2::scale_color_manual(values = cores_indice) + ggplot2::scale_fill_manual(values = cores_puzzle, guide = "none", na.value = "grey90") +
        ggplot2::scale_shape_manual(values = c(`TRUE` = 16, `FALSE` = 4), labels = c(`TRUE` = "acerto", `FALSE` = "erro")) +
        ggplot2::coord_cartesian(ylim = c(-2.5, 3)) + ggplot2::facet_wrap(~session, ncol = 1, scales = "free_x") +
        ggplot2::labs(title = "Expressões ao longo do jogo (escore z em relação ao próprio estudante)", x = "Minutos", y = "Desvios", color = NULL, shape = NULL) + tema
    })
    output$t_acoes <- DT::renderDT({
      x <- d(); shiny::req(nrow(x$af) > 0)
      ac <- afeto_por_evento(x$grupo$coleta, eventos = c("puzzle_start", "solution_snapshot", "puzzle_complete", "jewel_earned", "idle_start", "pause_open"),
                                        participante = x$p)
      ac <- ac |> dplyr::left_join(x$tent |> dplyr::select("session", "t_ms", "success"), by = c("session", "t_ms"))
      tabela_dt(ac |> dplyr::transmute(Minuto = round(.data$t_ms / 60000, 1), Cena = .data$scene,
                                      Ação = dplyr::case_when(.data$event == "solution_snapshot" & .data$success %in% TRUE ~ "tentativa ✅",
                                                              .data$event == "solution_snapshot" ~ "tentativa ❌",
                                                              TRUE ~ dplyr::recode(.data$event, puzzle_start = "iniciou a fase", puzzle_complete = "concluiu a fase",
                                                                                   jewel_earned = "ganhou joia", idle_start = "ficou parado", pause_open = "pausou")),
                                      Rosto = pct(.data$rosto), Sorriso = round(.data$z_smile, 1), `Sobr. franzida` = round(.data$z_brow_down, 1),
                                      `Lábios press.` = round(.data$z_mouth_press, 1)), n = 10,
                caption = "Expressão de 1 s antes a 3 s depois de cada ação (escore z em relação ao próprio estudante)")
    })

    output$botao_relatorio <- shiny::renderUI({
      if (!relatorios_disponiveis()) return(shiny::div(class = "text-muted small", "O relatório completo em HTML é gerado no RStudio, com a função gerar_relatorio_individual() do pacote."))
      shiny::tagList(shiny::downloadButton(session$ns("relatorio"), "Relatório individual (HTML)", class = "w-100"),
                     shiny::div(class = "text-muted small mt-2", "Gera o relatório completo deste estudante em R Markdown (cerca de 1 min)."))
    })

    output$relatorio <- shiny::downloadHandler(
      filename = function() paste0("relatorio_", input$participante, ".html"),
      content = function(arquivo) {
        a <- analise(); shiny::req(a, input$participante)
        shiny::withProgress(message = paste("Gerando o relatório de", input$participante, "…"), value = 0.3, {
          funcao_relatorio("gerar_relatorio_individual")(input$participante, caminhos_da_analise(a, fonte()), arquivo = arquivo, abrir = FALSE)
        })
      })
  })
}

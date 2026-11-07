# Módulo 2 — Estudo em grupo (baseado no relatório de grupo).

grupoUI <- function(id) {
  ns <- shiny::NS(id)
  bslib::layout_sidebar(
    sidebar = bslib::sidebar(
      width = 280, title = "Filtros",
      shiny::selectInput(ns("escola"), "Escola", "Todas"),
      shiny::selectInput(ns("turma"), "Turma", "Todas"),
      shiny::selectInput(ns("condicao"), "Condição", "Todas"),
      shiny::selectInput(ns("serie"), "Série", "Todas"),
      shiny::checkboxInput(ns("sem_interrompidas"), "Excluir sessões interrompidas", FALSE),
      shiny::hr(),
      shiny::uiOutput(ns("botao_relatorio"))
    ),
    shiny::uiOutput(ns("aviso")),
    bslib::layout_column_wrap(
      width = 1 / 6, fill = FALSE,
      bslib::value_box("Estudantes", shiny::textOutput(ns("vb_est")), showcase = bsicons::bs_icon("people"), theme = "primary"),
      bslib::value_box("Escolas", shiny::textOutput(ns("vb_esc")), showcase = bsicons::bs_icon("building"), theme = "info"),
      bslib::value_box("Fases concluídas", shiny::textOutput(ns("vb_concl")), showcase = bsicons::bs_icon("trophy"), theme = "success"),
      bslib::value_box("Tentativas por fase", shiny::textOutput(ns("vb_tent")), showcase = bsicons::bs_icon("arrow-repeat"), theme = "secondary"),
      bslib::value_box("Tempo médio de jogo", shiny::textOutput(ns("vb_tempo")), showcase = bsicons::bs_icon("stopwatch"), theme = "secondary"),
      bslib::value_box("Cobertura da câmera", shiny::textOutput(ns("vb_cam")), showcase = bsicons::bs_icon("camera-video"), theme = "light")
    ),
    bslib::navset_card_tab(
      full_screen = TRUE,
      bslib::nav_panel("Qualidade da coleta", DT::DTOutput(ns("validacao")), shiny::plotOutput(ns("g_eventos"), height = "320px")),
      bslib::nav_panel("Desempenho por fase",
        bslib::layout_columns(col_widths = c(7, 5), shiny::plotOutput(ns("g_tentativas"), height = "420px"), shiny::plotOutput(ns("g_conclusao"), height = "420px"))),
      bslib::nav_panel("Como resolvem",
        bslib::layout_columns(col_widths = c(6, 6), shiny::plotOutput(ns("g_curvas"), height = "420px"), shiny::plotOutput(ns("g_estrategia"), height = "420px"))),
      bslib::nav_panel("Domínio dos conceitos",
        bslib::layout_columns(col_widths = c(7, 5), shiny::plotOutput(ns("g_dominio"), height = "380px"), DT::DTOutput(ns("t_bkt")))),
      bslib::nav_panel("Estados afetivos",
        shiny::div(class = "text-muted small mb-2", "Estimados por regras (estados_faciais(), versão ", regras_estados()$versao,
                   "): indicadores, não diagnósticos."),
        bslib::layout_columns(col_widths = c(6, 6), shiny::plotOutput(ns("g_estados"), height = "380px"), DT::DTOutput(ns("t_transicoes")))),
      bslib::nav_panel("Expressões faciais", shiny::plotOutput(ns("g_afeto"), height = "400px")),
      bslib::nav_panel("Condições A/B", shiny::uiOutput(ns("ab")))
    )
  )
}

grupoServer <- function(id, analise, fonte) {
  shiny::moduleServer(id, function(input, output, session) {
    shiny::observe({
      a <- analise(); shiny::req(a)
      s <- a$tb$sessoes
      upd <- function(campo, col) shiny::updateSelectInput(session, campo, choices = c("Todas", sort(unique(stats::na.omit(as.character(s[[col]]))))))
      upd("escola", "escola"); upd("turma", "turma"); upd("condicao", "condicao"); upd("serie", "serie")
    })

    f <- shiny::reactive({
      a <- analise(); shiny::req(a)
      s <- a$tb$sessoes
      for (p in list(c("escola", "escola"), c("turma", "turma"), c("condicao", "condicao"), c("serie", "serie"))) {
        v <- input[[p[1]]]; if (!is.null(v) && v != "Todas") s <- s[as.character(s[[p[2]]]) %in% v, ]
      }
      if (isTRUE(input$sem_interrompidas)) s <- s[!s$interrompida, ]
      ses <- s$session; par <- unique(s$participant)
      filtra <- function(d, col = "session", ids = ses) if (is.null(d) || nrow(d) == 0) d else d[d[[col]] %in% ids, ]
      list(sessoes = s, fases = filtra(a$tb$fases), tent = filtra(a$tb$tentativas), afeto_sessao = filtra(a$tb$afeto_sessao),
           eventos = filtra(a$coleta$eventos), afeto = filtra(a$coleta$afeto),
           dom = if (!is.null(a$dom)) filtra(a$dom$final, "participant", par), dom_par = a$dom$parametros,
           est = filtra(a$est), validacao = a$validacao)
    })

    output$aviso <- shiny::renderUI({
      if (is.null(analise())) return(shiny::div(class = "alert alert-info", shiny::icon("spinner", class = "fa-spin"),
                                                " Carregando a coleta do servidor… (a primeira carga pode levar alguns minutos com muitos dados faciais)"))
      if (fonte()$modo == "demo") shiny::div(class = "alert alert-warning py-2", shiny::strong("Dados simulados (demonstração)."))
    })

    output$vb_est <- shiny::renderText(num(dplyr::n_distinct(f()$sessoes$participant), 0))
    output$vb_esc <- shiny::renderText(num(dplyr::n_distinct(f()$sessoes$escola), 0))
    output$vb_concl <- shiny::renderText({ x <- f()$fases; if (nrow(x) == 0) "—" else pct(mean(x$concluiu)) })
    output$vb_tent <- shiny::renderText({ x <- f()$fases; x <- x[x$concluiu, ]; if (nrow(x) == 0) "—" else num(mean(x$tentativas), 1) })
    output$vb_tempo <- shiny::renderText({ s <- f()$sessoes; if (nrow(s) == 0) "—" else paste(num(mean(s$duracao_min, na.rm = TRUE), 0), "min") })
    output$vb_cam <- shiny::renderText({ a <- f()$afeto_sessao; if (is.null(a) || nrow(a) == 0) "sem câmera" else pct(mean(a$cobertura, na.rm = TRUE)) })

    output$validacao <- DT::renderDT({
      v <- f()$validacao; shiny::req(v)
      tabela_dt(tibble::as_tibble(v) |> dplyr::transmute(Situação = dplyr::recode(.data$status, ok = "✅ ok", aviso = "⚠️ aviso", erro = "❌ erro"),
                                                        Verificação = .data$verificacao, Detalhe = .data$detalhe), n = 12)
    })
    output$g_eventos <- shiny::renderPlot({
      e <- f()$eventos; shiny::req(nrow(e) > 0)
      e |> dplyr::count(.data$event, sort = TRUE) |> utils::head(20) |>
        ggplot2::ggplot(ggplot2::aes(.data$n, stats::reorder(.data$event, .data$n))) + ggplot2::geom_col(fill = "#1F4E79") +
        ggplot2::labs(title = "Eventos por tipo (20 mais frequentes)", x = NULL, y = NULL) + tema
    })

    output$g_tentativas <- shiny::renderPlot({
      x <- f()$fases; x <- x[x$concluiu & !is.na(x$puzzle), ]
      if (nrow(x) == 0) return(vazio("Sem fases concluídas"))
      ggplot2::ggplot(x, ggplot2::aes(.data$tentativas, stats::reorder(.data$scene, dplyr::desc(.data$scene)), fill = .data$puzzle)) +
        ggplot2::geom_boxplot(outlier.alpha = .3, show.legend = FALSE) + ggplot2::scale_fill_manual(values = cores_puzzle) +
        ggplot2::labs(title = "Tentativas até concluir cada fase", x = "Tentativas", y = NULL) + tema
    })
    output$g_conclusao <- shiny::renderPlot({
      x <- f()$fases; x <- x[!is.na(x$puzzle), ]
      if (nrow(x) == 0) return(vazio("—"))
      x |> dplyr::group_by(.data$scene) |> dplyr::summarise(Concluiu = mean(.data$concluiu), Abandonou = mean(.data$abandonou, na.rm = TRUE)) |>
        tidyr::pivot_longer(-"scene") |>
        ggplot2::ggplot(ggplot2::aes(.data$value, stats::reorder(.data$scene, dplyr::desc(.data$scene)), fill = .data$name)) +
        ggplot2::geom_col(position = "dodge") + ggplot2::scale_fill_manual(values = c(Concluiu = "#2CA02C", Abandonou = "#C00000")) +
        ggplot2::scale_x_continuous(labels = scales::percent) + ggplot2::labs(title = "Conclusão e abandono", x = NULL, y = NULL, fill = NULL) + tema
    })

    output$g_curvas <- shiny::renderPlot({
      t <- f()$tent; t <- t[!is.na(t$dist_rel) & t$attempt <= 8, ]
      if (nrow(t) == 0) return(vazio("Sem tentativas"))
      t |> dplyr::group_by(.data$puzzle, .data$attempt) |> dplyr::summarise(d = mean(.data$dist_rel), .groups = "drop") |>
        ggplot2::ggplot(ggplot2::aes(.data$attempt, .data$d, color = .data$puzzle)) + ggplot2::geom_line(linewidth = 1.1) + ggplot2::geom_point() +
        ggplot2::scale_color_manual(values = cores_puzzle) + ggplot2::scale_y_continuous(labels = scales::percent) +
        ggplot2::labs(title = "Curvas de aprendizagem: distância até a solução", x = "Tentativa", y = "Posições erradas", color = NULL) + tema
    })
    output$g_estrategia <- shiny::renderPlot({
      t <- f()$tent; t <- t[!is.na(t$n_changed) & t$n_changed >= 0 & t$attempt > 1 & !t$success, ]
      if (nrow(t) == 0) return(vazio("—"))
      t |> dplyr::mutate(estrategia = ifelse(.data$n_changed <= 2, "Depuração (muda poucas posições)", "Tentativa e erro (muda muitas)")) |>
        dplyr::count(.data$puzzle, .data$estrategia) |> dplyr::group_by(.data$puzzle) |> dplyr::mutate(p = .data$n / sum(.data$n)) |>
        ggplot2::ggplot(ggplot2::aes(.data$p, .data$puzzle, fill = .data$estrategia)) + ggplot2::geom_col() +
        ggplot2::scale_fill_manual(values = c("#1F77B4", "#E07B00")) + ggplot2::scale_x_continuous(labels = scales::percent) +
        ggplot2::labs(title = "Estratégia nas novas tentativas", x = NULL, y = NULL, fill = NULL) + tema + ggplot2::guides(fill = ggplot2::guide_legend(nrow = 2))
    })

    output$g_dominio <- shiny::renderPlot({
      d <- f()$dom; if (is.null(d) || nrow(d) == 0) return(vazio("Sem dados para o modelo de domínio"))
      d |> dplyr::count(.data$rotulo, .data$situacao) |> dplyr::group_by(.data$rotulo) |> dplyr::mutate(p = .data$n / sum(.data$n)) |> dplyr::ungroup() |>
        dplyr::mutate(situacao = factor(.data$situacao, names(cores_sit))) |>
        ggplot2::ggplot(ggplot2::aes(.data$p, .data$rotulo, fill = .data$situacao)) + ggplot2::geom_col(position = ggplot2::position_stack(reverse = TRUE)) +
        ggplot2::scale_fill_manual(values = cores_sit, drop = FALSE) + ggplot2::scale_x_continuous(labels = scales::percent) +
        ggplot2::labs(title = "Situação das crianças em cada conceito (BKT)", x = NULL, y = NULL, fill = NULL) + tema
    })
    output$t_bkt <- DT::renderDT({
      p <- f()$dom_par; shiny::req(p)
      tabela_dt(p |> dplyr::transmute(Conceito = .data$rotulo, `Já dominava` = pct(.data$L0), `Aprende/oport.` = pct(.data$T),
                                      Chute = pct(.data$G), Deslize = pct(.data$S), Estudantes = .data$estudantes), n = 6)
    })

    output$g_estados <- shiny::renderPlot({
      e <- f()$est; if (is.null(e) || nrow(e) == 0) return(vazio("Sem dados"))
      e |> dplyr::mutate(puzzle = dplyr::coalesce(puzzle_de(.data$scene), "Outras")) |> dplyr::count(.data$puzzle, .data$estado) |>
        dplyr::group_by(.data$puzzle) |> dplyr::mutate(p = .data$n / sum(.data$n)) |> dplyr::ungroup() |>
        dplyr::mutate(estado = factor(.data$estado, names(cores_estado))) |>
        ggplot2::ggplot(ggplot2::aes(.data$p, .data$puzzle, fill = .data$estado)) + ggplot2::geom_col(position = ggplot2::position_stack(reverse = TRUE)) +
        ggplot2::scale_fill_manual(values = cores_estado, labels = nomes_estado) + ggplot2::scale_x_continuous(labels = scales::percent) +
        ggplot2::labs(title = "Tempo em cada estado, por puzzle", x = NULL, y = NULL, fill = NULL) + tema
    })
    output$t_transicoes <- DT::renderDT({
      e <- f()$est; shiny::req(!is.null(e), nrow(e) > 0)
      sq <- e |> dplyr::filter(.data$estado != "neutro") |> dplyr::arrange(.data$session, .data$t_ini) |> dplyr::group_by(.data$session) |>
        dplyr::mutate(proximo = dplyr::lead(.data$estado)) |> dplyr::ungroup() |> dplyr::filter(!is.na(.data$proximo), .data$proximo != .data$estado)
      shiny::req(nrow(sq) > 0)
      base <- sq |> dplyr::count(.data$proximo) |> dplyr::mutate(pb = .data$n / sum(.data$n)) |> dplyr::select(-"n")
      tr <- sq |> dplyr::count(.data$estado, .data$proximo) |> dplyr::group_by(.data$estado) |> dplyr::mutate(pc = .data$n / sum(.data$n)) |> dplyr::ungroup() |>
        dplyr::left_join(base, by = "proximo") |> dplyr::mutate(L = round((.data$pc - .data$pb) / (1 - .data$pb), 2))
      tabela_dt(tr |> dplyr::transmute(De = nomes_estado[.data$estado], Para = nomes_estado[.data$proximo], L = .data$L) |>
                  tidyr::pivot_wider(names_from = "Para", values_from = "L"), n = 6,
                caption = "Transições entre estados (L de D'Mello & Graesser; > 0 = mais que o acaso)")
    })

    output$g_afeto <- shiny::renderPlot({
      a <- f()$afeto; if (is.null(a) || nrow(a) == 0) return(vazio("Sem dados faciais nesta coleta"))
      a <- a[a$face_present == 1, ]
      a |> dplyr::mutate(puzzle = puzzle_de(.data$scene)) |> dplyr::filter(!is.na(.data$puzzle)) |>
        dplyr::group_by(.data$participant, .data$puzzle) |> dplyr::summarise(dplyr::across(c("smile", "brow_down", "mouth_press"), ~ mean(.x, na.rm = TRUE)), .groups = "drop") |>
        tidyr::pivot_longer(c("smile", "brow_down", "mouth_press")) |> dplyr::mutate(name = nomes_indice[.data$name]) |>
        ggplot2::ggplot(ggplot2::aes(.data$puzzle, .data$value, fill = .data$puzzle)) + ggplot2::geom_boxplot(show.legend = FALSE, outlier.alpha = .2) +
        ggplot2::facet_wrap(~name, scales = "free_y") + ggplot2::scale_fill_manual(values = cores_puzzle) +
        ggplot2::labs(title = "Índices faciais por puzzle (média por criança)", x = NULL, y = "Índice (0–1)") + tema
    })

    output$ab <- shiny::renderUI({
      s <- f()$sessoes
      if (dplyr::n_distinct(s$condicao[!is.na(s$condicao) & s$condicao != ""]) < 2) return(shiny::p(class = "text-muted", "Esta coleta não tem duas ou mais condições experimentais."))
      shiny::tagList(shiny::plotOutput(session$ns("g_ab"), height = "360px"), DT::DTOutput(session$ns("t_ab")))
    })
    output$g_ab <- shiny::renderPlot({
      x <- f()$fases; x <- x[x$concluiu & !is.na(x$condicao) & x$condicao != "", ]
      ggplot2::ggplot(x, ggplot2::aes(.data$condicao, .data$tentativas, fill = .data$condicao)) + ggplot2::geom_boxplot(show.legend = FALSE) +
        ggplot2::facet_wrap(~puzzle, nrow = 1) + ggplot2::labs(title = "Tentativas por fase concluída, por condição", x = "Condição", y = "Tentativas") + tema
    })
    output$t_ab <- DT::renderDT({
      x <- f()$fases
      tabela_dt(x |> dplyr::filter(!is.na(.data$condicao), .data$condicao != "") |> dplyr::group_by(Condição = .data$condicao) |>
                  dplyr::summarise(Estudantes = dplyr::n_distinct(.data$participant), `Fases concluídas` = pct(mean(.data$concluiu)),
                                   `Tentativas (concluídas)` = round(mean(.data$tentativas[.data$concluiu]), 2),
                                   `Abandono` = pct(mean(.data$abandonou, na.rm = TRUE))), n = 5)
    })

    output$botao_relatorio <- shiny::renderUI({
      if (!relatorios_disponiveis()) return(shiny::div(class = "text-muted small", "O relatório completo em HTML é gerado no RStudio, com a função gerar_relatorio() do pacote."))
      shiny::tagList(shiny::downloadButton(session$ns("relatorio"), "Relatório completo (HTML)", class = "w-100"),
                     shiny::div(class = "text-muted small mt-2", "Gera o relatório de grupo em R Markdown com todas as análises (pode levar 1–2 min)."))
    })

    output$relatorio <- shiny::downloadHandler(
      filename = function() paste0("relatorio_grupo_", format(Sys.Date(), "%Y%m%d"), ".html"),
      content = function(arquivo) {
        a <- analise(); shiny::req(a)
        shiny::withProgress(message = "Gerando o relatório de grupo…", value = 0.3, {
          caminhos <- caminhos_da_analise(a, fonte())
          funcao_relatorio("gerar_relatorio")(caminhos, arquivo, abrir = FALSE, n_alunos = fonte()$n_alunos)
        })
      })
  })
}

# Pastas com os arquivos da coleta carregada (para gerar os relatórios em R Markdown)
caminhos_da_analise <- function(a, f) {
  switch(f$modo,
         servidor = attr(a$coleta, "pasta"),
         pasta = trimws(strsplit(f$caminhos, ";")[[1]]),
         demo = file.path(tempdir(), paste0("painel_demo_", f$n_alunos)))
}

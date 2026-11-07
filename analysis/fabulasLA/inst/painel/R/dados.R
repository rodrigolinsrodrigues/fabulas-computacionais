# Camada de dados do painel. Os dados vêm direto do Supabase, com as credenciais de
# credenciais.R (nesta pasta, fora do Git). Pasta local e demonstração ficam só para testes.

# Credenciais: argumentos de abrir_painel() > credenciais.R > variáveis de ambiente.
ler_credenciais <- function(pasta_app = getwd()) {
  arq <- file.path(pasta_app, "credenciais.R")
  if (!file.exists(arq)) return(list())
  amb <- new.env()
  tryCatch(sys.source(arq, envir = amb), error = function(e) warning("credenciais.R com erro: ", conditionMessage(e)))
  amb$FABULAS_PAINEL %||% list()
}

fonte_padrao <- function(pasta_app = getwd()) {
  o <- getOption(paste0("fabulas", "LA.painel"), list())
  cr <- ler_credenciais(pasta_app)
  env <- function(n) Sys.getenv(n, "")
  vazio_ou <- function(...) { for (v in list(...)) if (!is.null(v) && length(v) && nzchar(v)) return(v); "" }
  f <- list(
    servidor_url = vazio_ou(o$servidor_url, cr$servidor_url, env("FABULAS_URL")),
    chave_publica = vazio_ou(o$chave_publica, cr$chave_publica, env("FABULAS_CHAVE_PUBLICA")),
    estudo = vazio_ou(o$estudo, cr$estudo, env("FABULAS_ESTUDO")),
    chave_leitura = vazio_ou(o$chave_leitura, cr$chave_leitura, env("FABULAS_CHAVE_LEITURA")),
    caminhos = paste(o$caminhos %||% character(), collapse = ";"),
    n_alunos = o$n_alunos %||% 60
  )
  completo <- nzchar(f$servidor_url) && nzchar(f$chave_publica) && nzchar(f$estudo) && nzchar(f$chave_leitura)
  f$modo <- o$modo %||% (if (completo) "servidor" else if (nzchar(f$caminhos)) "pasta" else "sem_credenciais")
  f
}

# Arquivos de dados (catálogo de eventos, mapa de conceitos) copiados para a pasta do painel.
pasta_extdata <- function() normalizePath(file.path(getwd(), "extdata"), mustWork = FALSE)

# Os relatórios completos em R Markdown precisam do pacote instalado (no RStudio, sim; no
# shinyapps.io, não). O nome é montado em partes para o shinyapps.io não tentar instalá-lo.
pacote_relatorios <- paste0("fabulas", "LA")
relatorios_disponiveis <- function() nzchar(system.file(package = pacote_relatorios))
funcao_relatorio <- function(nome) getExportedValue(pacote_relatorios, nome)

# Datas do servidor vêm em UTC; o painel mostra no fuso do computador.
local_tz <- function(x) { attr(x, "tzone") <- ""; x }

`%||%` <- function(a, b) if (is.null(a) || length(a) == 0) b else a

descricao_fonte <- function(f) {
  switch(f$modo,
         servidor = paste0("Supabase · estudo ", f$estudo),
         pasta = paste0("Pasta local · ", basename(f$caminhos)),
         demo = paste0("Demonstração · ", f$n_alunos, " estudantes simulados"),
         sem_credenciais = "Sem conexão configurada")
}

# ---- Visão rápida das sessões (monitor): leve, pode ser chamada a cada poucos segundos
visao_sessoes <- function(f) {
  if (f$modo == "sem_credenciais") stop("credenciais do Supabase não configuradas (arquivo credenciais.R)")
  if (f$modo == "servidor") {
    s <- status_servidor(f$servidor_url, f$chave_publica, f$estudo, f$chave_leitura)
    if (nrow(s) == 0) return(sessoes_vazias())
    num <- function(x) suppressWarnings(as.numeric(x))
    hora <- function(x) as.POSIXct(sub("(\\.\\d+)?([+-]\\d{2}:?\\d{2}|Z)$", "", sub("T", " ", x)), tz = "UTC") |> local_tz()
    return(tibble::tibble(
      session = s$session, participant = s$participant %||% NA, escola = s$school_id %||% NA, turma = s$class_id %||% NA,
      maquina = s$machine_id %||% NA, condicao = s$condition %||% NA,
      encerrada = as.logical(s$ended), motivo = s$end_reason %||% NA,
      ultimo_seq = num(s$last_seq), eventos = num(s$n_events), amostras = num(s$n_affect),
      primeiro = hora(s$first_received), ultimo = hora(s$last_received)))
  }
  coleta <- carregar_coleta(f, leve = TRUE)
  ev <- coleta$eventos
  if (nrow(ev) == 0) return(sessoes_vazias())
  ini <- ev |> dplyr::filter(.data$event == "session_start")
  hora_ev <- function(x) as.POSIXct(sub("Z$", "", sub("T", " ", x)), tz = "UTC") |> local_tz()
  vs <- ev |> dplyr::group_by(.data$session) |>
    dplyr::summarise(participant = dplyr::first(.data$participant), ultimo_seq = max(.data$seq, na.rm = TRUE),
                     eventos = dplyr::n(), encerrada = any(.data$event == "session_end"),
                     motivo = dplyr::last(.data$reason[.data$event == "session_end"]) %||% NA_character_,
                     primeiro = min(hora_ev(.data$t), na.rm = TRUE), ultimo = max(hora_ev(.data$t), na.rm = TRUE), .groups = "drop")
  meta <- ini |> dplyr::transmute(session = .data$session, escola = .data$school_id, turma = .data$class_id,
                                  maquina = .data$machine_id, condicao = .data$condition)
  am <- if (nrow(coleta$afeto)) coleta$afeto |> dplyr::count(.data$session, name = "amostras") else tibble::tibble(session = character(), amostras = integer())
  vs |> dplyr::left_join(meta, by = "session") |> dplyr::left_join(am, by = "session") |>
    dplyr::mutate(amostras = tidyr::replace_na(.data$amostras, 0L)) |>
    dplyr::select("session", "participant", "escola", "turma", "maquina", "condicao", "encerrada", "motivo",
                  "ultimo_seq", "eventos", "amostras", "primeiro", "ultimo")
}

sessoes_vazias <- function() tibble::tibble(session = character(), participant = character(), escola = character(), turma = character(),
                                            maquina = character(), condicao = character(), encerrada = logical(), motivo = character(),
                                            ultimo_seq = numeric(), eventos = numeric(), amostras = numeric(),
                                            primeiro = as.POSIXct(character()), ultimo = as.POSIXct(character()))

# ---- Coleta completa (grupo e individual)
.cache_demo <- new.env()
carregar_coleta <- function(f, leve = FALSE) {
  switch(f$modo,
    servidor = ler_coleta_servidor(f$servidor_url, f$chave_publica, f$estudo, f$chave_leitura,
                                              afeto = !leve, verificar_integridade = FALSE),
    pasta = ler_coleta(trimws(strsplit(f$caminhos, ";")[[1]]), verificar_integridade = FALSE),
    demo = {
      chave <- paste0("n", f$n_alunos)
      if (is.null(.cache_demo[[chave]])) {
        pasta <- simular_coleta(n_alunos = f$n_alunos, seed = 2026, pasta = file.path(tempdir(), paste0("painel_demo_", f$n_alunos)))
        .cache_demo[[chave]] <- ler_coleta(pasta)
      }
      .cache_demo[[chave]]
    })
}

# Tudo que os módulos de grupo e individual precisam, calculado uma vez por carga.
preparar_analise <- function(coleta, progresso = function(v, msg) NULL) {
  progresso(0.3, "Montando tabelas de análise")
  tb <- tabelas_analise(coleta)
  progresso(0.5, "Estimando o domínio dos conceitos (BKT)")
  dom <- tryCatch(dominio_habilidades(coleta, validacao_cruzada = FALSE), error = function(e) NULL)
  progresso(0.75, "Estimando estados afetivos")
  est <- tryCatch(estados_faciais(coleta), error = function(e) NULL)
  progresso(0.9, "Validando a coleta")
  val <- tryCatch(validar_coleta(coleta), error = function(e) NULL)
  list(coleta = coleta, tb = tb, dom = dom, est = est, validacao = val, carregado_em = Sys.time())
}

# Últimos eventos de uma sessão (detalhe no monitor)
eventos_da_sessao <- function(f, sessao, coleta = NULL) {
  if (f$modo != "servidor") {
    ev <- (coleta %||% carregar_coleta(f, leve = TRUE))$eventos
    return(ev |> dplyr::filter(.data$session == sessao) |> dplyr::arrange(dplyr::desc(.data$seq)))
  }
  todos <- list(); depois <- 0
  repeat {
    pg <- .post_supabase(f$servidor_url, f$chave_publica, "fc_export_events",
                                     list(p_study = f$estudo, p_key = f$chave_leitura, p_after_session = sessao,
                                          p_after_seq = depois, p_limit = 1000))
    pg <- Filter(function(e) identical(e$session, sessao), pg)
    if (length(pg) == 0) break
    todos <- c(todos, pg); depois <- pg[[length(pg)]]$seq
    if (length(pg) < 1000) break
  }
  # p_after_session = sessao pega a partir do seq 1 desta sessão (chave (sessão, seq) > (sessão, 0))
  if (length(todos) == 0) return(tibble::tibble())
  dplyr::bind_rows(lapply(todos, function(e) tibble::tibble(seq = e$seq, t = e$t, scene = e$scene %||% "", event = e$event,
                                                            detalhe = substr(as.character(jsonlite::toJSON(e$data, auto_unbox = TRUE)), 1, 160)))) |>
    dplyr::arrange(dplyr::desc(.data$seq))
}

# Mesmos rótulos e cores dos relatórios
cores_puzzle <- c(Musical = "#7B3F99", Espelhos = "#1F77B4", Conjuntos = "#2CA02C", Receitas = "#E07B00", `Área` = "#9E9E9E")
cores_estado <- c(confusao = "#E07B00", frustracao = "#C00000", tedio = "#7F7F7F", engajamento = "#1F77B4",
                  encantamento = "#2CA02C", neutro = "#E6E6E6")
nomes_estado <- c(confusao = "Confusão", frustracao = "Frustração", tedio = "Tédio", engajamento = "Engajamento",
                  encantamento = "Encantamento", neutro = "Neutro")
cores_sit <- c(domina = "#2CA02C", `em progresso` = "#E0A800", `ainda não domina` = "#C00000", `poucos dados` = "#BDBDBD")
nomes_indice <- c(smile = "Sorriso", brow_down = "Sobrancelha franzida", mouth_press = "Lábios pressionados")
cores_indice <- c(Sorriso = "#2CA02C", `Sobrancelha franzida` = "#1F77B4", `Lábios pressionados` = "#C00000")
puzzle_de <- function(cena) dplyr::case_when(grepl("^Musical", cena) ~ "Musical", grepl("^Espelhos", cena) ~ "Espelhos",
                                             grepl("^Conjuntos", cena) ~ "Conjuntos", grepl("^Receitas", cena) ~ "Receitas",
                                             grepl("^Area", cena) ~ "Área", TRUE ~ NA_character_)
pct <- function(x, d = 0) scales::percent(x, accuracy = 10^-d, decimal.mark = ",")
num <- function(x, d = 1) format(round(x, d), nsmall = d, decimal.mark = ",", big.mark = ".")
tema <- ggplot2::theme_minimal(base_size = 13) +
  ggplot2::theme(plot.title = ggplot2::element_text(face = "bold"), legend.position = "bottom", panel.grid.minor = ggplot2::element_blank())
tabela_dt <- function(df, n = 10, ...) DT::datatable(df, rownames = FALSE, options = list(pageLength = n, scrollX = TRUE,
  language = list(url = "https://cdn.datatables.net/plug-ins/1.13.6/i18n/pt-BR.json")), ...)
vazio <- function(msg) { ggplot2::ggplot() + ggplot2::annotate("text", 0, 0, label = msg, size = 5, color = "grey40") + ggplot2::theme_void() }

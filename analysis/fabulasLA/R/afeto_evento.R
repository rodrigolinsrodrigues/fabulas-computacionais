#' Expressão facial em torno de cada evento do jogo (integração multimodal).
#'
#' Junta eventos e amostras faciais pelo relógio comum (`t_ms`) da sessão: para cada evento,
#' resume as amostras com rosto detectado na janela `[t - antes_ms, t + depois_ms]`.
#' Também devolve os índices em **escore z em relação à linha de base do próprio participante**
#' (média e desvio de todas as amostras dele com rosto detectado), o que permite comparar
#' crianças com rostos e expressividade diferentes.
#'
#' @param coleta objeto de [ler_coleta()] ou [ler_coleta_servidor()]
#' @param eventos tipos de evento a considerar (NULL = todos, exceto os resumos `affect_*`)
#' @param antes_ms,depois_ms tamanho da janela em milissegundos
#' @param indices índices faciais a resumir (colunas do arquivo facial)
#' @param participante filtra um participante (código)
#' @return tibble com uma linha por evento: identificação, `amostras`, `rosto` (proporção de
#'   amostras com rosto), a média de cada índice e `z_<indice>` (em desvios da linha de base)
#' @examples
#' \dontrun{
#' coleta <- ler_coleta("PILOTO26")
#' erros <- afeto_por_evento(coleta, "solution_snapshot", antes_ms = 2000, depois_ms = 5000)
#' }
#' @export
afeto_por_evento <- function(coleta, eventos = NULL, antes_ms = 1000, depois_ms = 3000,
                             indices = c("smile", "brow_down", "mouth_press", "eye_wide", "jaw_open", "gaze_away"),
                             participante = NULL) {
  ev <- coleta$eventos
  af <- coleta$afeto
  if (!is.null(participante)) {
    participante <- .normalizar_codigo(participante)
    ev <- ev[.normalizar_codigo(ev$participant) %in% participante, ]
    if (nrow(af) > 0) af <- af[.normalizar_codigo(af$participant) %in% participante, ]
  }
  ev <- if (is.null(eventos)) ev[!grepl("^affect_", ev$event), ] else ev[ev$event %in% eventos, ]
  base <- tibble(session = ev$session, participant = ev$participant, seq = ev$seq, t_ms = ev$t_ms,
                 scene = ev$scene, event = ev$event)
  indices <- intersect(indices, names(af))
  if (nrow(base) == 0 || nrow(af) == 0 || length(indices) == 0) {
    vazio <- base |> mutate(amostras = 0L, rosto = NA_real_)
    for (i in indices) vazio[[i]] <- NA_real_
    for (i in indices) vazio[[paste0("z_", i)]] <- NA_real_
    return(vazio)
  }

  # Linha de base de cada participante (só amostras com rosto)
  com_rosto <- af[!is.na(af$face_present) & af$face_present == 1, c("participant", indices)]
  lb <- com_rosto |> group_by(.data$participant) |>
    summarise(across(all_of(indices), list(m = ~ mean(.x, na.rm = TRUE), s = ~ stats::sd(.x, na.rm = TRUE))), .groups = "drop")

  janelas <- base |> mutate(lo = .data$t_ms - antes_ms, hi = .data$t_ms + depois_ms, .linha = dplyr::row_number())
  amostras <- af |> select("session", t_af = "t_ms", "face_present", all_of(indices))
  junto <- janelas |> select(".linha", "session", "lo", "hi") |>
    inner_join(amostras, by = join_by(session, between(y$t_af, x$lo, x$hi)))
  resumo <- junto |> group_by(.data$.linha) |>
    summarise(amostras = dplyr::n(), rosto = mean(.data$face_present == 1, na.rm = TRUE),
              across(all_of(indices), ~ if (any(face_present == 1, na.rm = TRUE)) mean(.x[face_present == 1], na.rm = TRUE) else NA_real_),
              .groups = "drop")

  out <- janelas |> left_join(resumo, by = ".linha") |>
    mutate(amostras = tidyr::replace_na(.data$amostras, 0L)) |>
    left_join(lb, by = "participant")
  for (i in indices) {
    m <- out[[paste0(i, "_m")]]; s <- out[[paste0(i, "_s")]]
    out[[paste0("z_", i)]] <- ifelse(is.na(s) | s == 0, NA_real_, (out[[i]] - m) / s)
  }
  out |> select("session", "participant", "seq", "t_ms", "scene", "event", "amostras", "rosto",
                all_of(indices), all_of(paste0("z_", indices))) |>
    arrange(.data$session, .data$t_ms)
}

.normalizar_codigo <- function(x) toupper(gsub("[^A-Za-z0-9]", "", x %||% ""))

# Carrega os dados de um relatório: servidor (Supabase), pastas/.zip ou simulação.
.carregar_coleta_relatorio <- function(caminhos = "", servidor_url = "", chave_publica = "", estudo = "",
                                       n_alunos = 100, semente = 2026) {
  if (nzchar(servidor_url)) {
    coleta <- ler_coleta_servidor(servidor_url, chave_publica, estudo)
    return(list(coleta = coleta, fonte = paste0("servidor ", servidor_url, " (estudo ", estudo, ")"), simulado = FALSE))
  }
  if (nzchar(caminhos)) {
    cam <- trimws(strsplit(caminhos, ";")[[1]])
    return(list(coleta = ler_coleta(cam), fonte = gsub("\\", "/", paste(cam, collapse = "; "), fixed = TRUE), simulado = FALSE))
  }
  pasta <- simular_coleta(n_alunos = n_alunos, seed = semente, pasta = file.path(tempdir(), paste0("fabulas_simulado_", semente, "_", n_alunos)))
  list(coleta = ler_coleta(pasta), fonte = "dados simulados", simulado = TRUE)
}

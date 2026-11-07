#' Estados afetivos de aprendizagem por janela de tempo (regras heurísticas, versão 0.1).
#'
#' Estima, para cada janela de tempo de cada sessão, o quanto há sinais de **confusão**,
#' **frustração**, **tédio**, **engajamento** e **encantamento** — os estados centrais da
#' aprendizagem segundo D'Mello e Graesser (2012). Cada estado combina sinais do rosto (em
#' escore z em relação à linha de base do próprio participante) com sinais de comportamento
#' no jogo na mesma janela. As regras estão em [regras_estados()].
#'
#' **São indicadores, não diagnósticos.** As regras não foram validadas com crianças; use para
#' explorar os dados e gerar hipóteses, e valide com observação em sala (BROMP) antes de tirar
#' conclusões. Nada disso é gravado no jogo ou no servidor: é recalculado a cada análise.
#'
#' @param coleta objeto de [ler_coleta()] ou [ler_coleta_servidor()]
#' @param janela_s tamanho da janela em segundos
#' @param limiar escore mínimo (0–1) para atribuir um estado; abaixo dele a janela fica "neutro"
#' @param cobertura_min proporção mínima de amostras com rosto para usar os sinais faciais; abaixo
#'   dela só o comportamento é usado e a confiança é marcada como "baixa"
#' @param participante filtra um participante (código)
#' @return tibble com uma linha por janela: `session`, `participant`, `scene`, `t_ini`, `t_fim`
#'   (ms), `cobertura` facial, um escore 0–1 para cada estado (`confusao`, `frustracao`, `tedio`,
#'   `engajamento`, `encantamento`), o `estado` predominante, a `confianca` ("alta", "media",
#'   "baixa") e a `versao` das regras
#' @references D'Mello, S., & Graesser, A. (2012). Dynamics of affective states during complex
#'   learning. Learning and Instruction, 22(2), 145–157.
#'   Ocumpaugh, J. et al. (2015). Baker Rodrigo Ocumpaugh Monitoring Protocol (BROMP) 2.0.
#' @export
estados_faciais <- function(coleta, janela_s = 10, limiar = 0.6, cobertura_min = 0.5, participante = NULL) {
  ev <- coleta$eventos
  af <- coleta$afeto
  if (!is.null(participante)) {
    cod <- .normalizar_codigo(participante)
    ev <- ev[.normalizar_codigo(ev$participant) %in% cod, ]
    if (nrow(af) > 0) af <- af[.normalizar_codigo(af$participant) %in% cod, ]
  }
  if (nrow(ev) == 0) return(.estados_vazio())
  jan_ms <- janela_s * 1000
  ev <- .garantir_cols(ev, c("success", "idle_ms", "distance", "total_positions"))

  # ---- Janelas por sessão
  limites <- ev |> group_by(.data$session, .data$participant) |>
    summarise(fim = max(.data$t_ms, na.rm = TRUE), .groups = "drop")
  janelas <- limites |> mutate(t_ini = lapply(.data$fim, function(f) seq(0, max(0, f), by = jan_ms))) |>
    tidyr::unnest("t_ini") |> select(-"fim") |>
    mutate(t_fim = .data$t_ini + jan_ms, w = floor(.data$t_ini / jan_ms))

  # Cena de cada janela: a última cena em que entrou até o meio da janela
  cenas <- ev |> filter(.data$event == "scene_enter") |> select("session", t_cena = "t_ms", "scene") |> arrange(.data$session, .data$t_cena)
  janelas <- janelas |> mutate(meio = .data$t_ini + jan_ms / 2) |>
    left_join(cenas, by = join_by(session, closest(meio >= t_cena))) |> select(-"meio", -"t_cena")

  # ---- Comportamento na janela
  ev <- ev |> mutate(w = floor(.data$t_ms / jan_ms))
  tent <- ev |> filter(.data$event == "solution_snapshot")
  acoes <- c("object_pick", "object_place", "object_drop", "pillar_rotate", "pillar_push", "recipe_place",
             "recipe_remove", "lens_used", "puzzle_attempt", "door_enter")
  comp <- ev |> group_by(.data$session, .data$w) |>
    summarise(erros = sum(.data$event == "solution_snapshot" & .data$success %in% c(FALSE, "false", "FALSE")),
              acertos = sum(.data$event %in% c("puzzle_complete", "jewel_earned")) +
                sum(.data$event == "solution_snapshot" & .data$success %in% c(TRUE, "true", "TRUE")),
              acoes = sum(.data$event %in% acoes),
              reinicios = sum(.data$event %in% c("scene_restart")),
              missao = sum(.data$event == "mission_panel_toggle"),
              fora = sum(.data$event == "app_blur"), .groups = "drop")
  # Erros seguidos sem melhorar (distância não diminuiu) na janela
  sem_progresso <- tent |> arrange(.data$session, .data$scene, .data$t_ms) |>
    group_by(.data$session, .data$scene) |>
    mutate(d = suppressWarnings(as.numeric(.data$distance)), piorou = !is.na(lag(.data$d)) & !is.na(.data$d) & .data$d >= lag(.data$d)) |>
    mutate(dist_rel = .data$d / suppressWarnings(as.numeric(.data$total_positions))) |>
    ungroup() |> group_by(.data$session, .data$w) |>
    summarise(sem_progresso = sum(.data$piorou & !(.data$success %in% c(TRUE, "true", "TRUE"))),
              distancia = { v <- mean(.data$dist_rel, na.rm = TRUE); if (is.nan(v)) 0 else pmin(1, v) }, .groups = "drop")
  # Fração da janela em ociosidade
  ocio <- .intervalos_ocio(ev)
  janelas <- janelas |> left_join(comp, by = c("session", "w")) |> left_join(sem_progresso, by = c("session", "w")) |>
    mutate(across(c("erros", "acertos", "acoes", "reinicios", "missao", "fora", "sem_progresso"), ~ tidyr::replace_na(.x, 0L)),
           distancia = tidyr::replace_na(.data$distancia, 0))
  janelas$ocioso <- .fracao_ocio(janelas, ocio)

  # ---- Rosto na janela (escore z pela linha de base do participante)
  ind <- c("smile", "brow_down", "mouth_press", "eye_wide", "eye_squint", "frown", "nose_sneer", "gaze_away", "jaw_open")
  if (nrow(af) > 0 && any(af$face_present == 1, na.rm = TRUE)) {
    ind <- intersect(ind, names(af))
    rosto <- af[!is.na(af$face_present) & af$face_present == 1, ]
    lb <- rosto |> group_by(.data$participant) |>
      summarise(across(all_of(ind), list(m = ~ mean(.x, na.rm = TRUE), s = ~ stats::sd(.x, na.rm = TRUE))), .groups = "drop")
    afz <- af |> left_join(lb, by = "participant")
    for (i in ind) {
      s <- afz[[paste0(i, "_s")]]
      afz[[paste0("z_", i)]] <- ifelse(afz$face_present == 1 & !is.na(s) & s > 0, (afz[[i]] - afz[[paste0(i, "_m")]]) / s, NA_real_)
    }
    afz$virado <- afz$face_present != 1 | abs(tidyr::replace_na(afz$head_yaw, 90)) > 30
    fac <- afz |> mutate(w = floor(.data$t_ms / jan_ms)) |> group_by(.data$session, .data$w) |>
      summarise(cobertura = mean(.data$face_present == 1, na.rm = TRUE), virado = mean(.data$virado, na.rm = TRUE),
                across(all_of(paste0("z_", ind)), ~ { v <- mean(.x, na.rm = TRUE); if (is.nan(v)) NA_real_ else v }),
                .groups = "drop")
    janelas <- janelas |> left_join(fac, by = c("session", "w"))
  }
  janelas <- .garantir_cols(janelas, c("cobertura", "virado", paste0("z_", c("smile", "brow_down", "mouth_press", "eye_wide",
                                                                          "eye_squint", "frown", "nose_sneer", "gaze_away", "jaw_open"))))
  janelas$cobertura <- tidyr::replace_na(janelas$cobertura, 0)

  # ---- Escores (regras v0.1)
  r <- regras_estados()
  usar_rosto <- janelas$cobertura >= cobertura_min
  z <- function(n) { v <- janelas[[paste0("z_", n)]]; v <- ifelse(usar_rosto, tidyr::replace_na(v, 0), 0); pmax(-3, pmin(3, v)) }
  cap <- function(x, m = 2) pmin(x, m)
  sinais <- list(
    z_smile = z("smile"), z_brow_down = z("brow_down"), z_mouth_press = z("mouth_press"), z_eye_wide = z("eye_wide"),
    z_eye_squint = z("eye_squint"), z_frown = z("frown"), z_nose_sneer = z("nose_sneer"), z_gaze_away = z("gaze_away"),
    z_jaw_open = z("jaw_open"),
    virado = ifelse(usar_rosto, tidyr::replace_na(janelas$virado, 0), 0),
    erros = cap(janelas$erros), sem_progresso = cap(janelas$sem_progresso), acertos = cap(janelas$acertos, 1),
    acoes = cap(janelas$acoes, 3) / 3, reinicios = cap(janelas$reinicios, 1), missao = cap(janelas$missao, 1),
    fora = cap(janelas$fora, 1), ocioso = janelas$ocioso, distancia = janelas$distancia)
  for (estado in names(r$pesos)) {
    pesos <- r$pesos[[estado]]
    lin <- r$intercepto[[estado]] + Reduce(`+`, lapply(names(pesos), function(n) pesos[[n]] * sinais[[n]]))
    janelas[[estado]] <- stats::plogis(lin)
  }
  est <- names(r$pesos)
  m <- as.matrix(janelas[, est])
  melhor <- max.col(m, ties.method = "first")
  topo <- m[cbind(seq_len(nrow(m)), melhor)]
  janelas$estado <- ifelse(topo >= limiar, est[melhor], "neutro")
  janelas$confianca <- ifelse(!usar_rosto, "baixa", ifelse(topo >= 0.75, "alta", "media"))
  janelas$versao <- r$versao

  janelas |> select("session", "participant", "scene", "t_ini", "t_fim", "cobertura", all_of(est), "estado", "confianca",
                    "erros", "acertos", "acoes", "ocioso", "versao") |>
    arrange(.data$session, .data$t_ini)
}

#' Regras usadas por [estados_faciais()] (pesos dos sinais de cada estado).
#'
#' Cada estado é `plogis(intercepto + soma(peso × sinal))`. Sinais faciais `z_*` são escores z em
#' relação à linha de base do participante (limitados a ±3); sinais de comportamento:
#' `erros`, `sem_progresso` (erros sem diminuir a distância), `distancia` (proporção de posições
#' erradas nas tentativas da janela, 0–1), `acertos`, `acoes` (0–1),
#' `reinicios`, `missao`, `fora` (saiu da janela do jogo), `ocioso` (fração da janela parado),
#' `virado` (fração das amostras sem rosto ou com a cabeça virada > 30°).
#' Base teórica: confusão ≈ AU4 (sobrancelha abaixada) + AU7 (pálpebras tensas) com impasse
#' (longe da solução, sem progresso);
#' frustração ≈ AU23/24 (lábios pressionados) + AU15 (cantos da boca para baixo) com erros sem
#' progresso; tédio ≈ olhar desviado, pouca ação, ociosidade; engajamento ≈ olhar na tela com ação;
#' encantamento ≈ AU12 (sorriso) + AU5 (olhos arregalados) junto a acertos (D'Mello & Graesser, 2012;
#' Grafsgaard et al., 2013).
#' @return lista com `versao`, `intercepto` e `pesos` por estado
#' @export
regras_estados <- function() {
  list(
    versao = "0.1",
    intercepto = list(confusao = -2.2, frustracao = -2.6, tedio = -2.4, engajamento = -0.6, encantamento = -2.6),
    pesos = list(
      confusao = list(z_brow_down = 0.9, z_eye_squint = 0.5, z_mouth_press = 0.3, erros = 0.4, distancia = 2.0,
                      sem_progresso = 0.8, missao = 0.6, ocioso = 0.4),
      frustracao = list(z_mouth_press = 0.8, z_frown = 0.6, z_nose_sneer = 0.4, z_brow_down = 0.3, sem_progresso = 0.9,
                        erros = 0.3, reinicios = 1.2),
      tedio = list(z_gaze_away = 0.6, virado = 1.2, ocioso = 2.2, fora = 1.0, z_jaw_open = 0.3, acoes = -1.0, z_smile = -0.3),
      engajamento = list(acoes = 1.6, erros = 0.3, acertos = 0.4, virado = -1.5, ocioso = -2.0, z_gaze_away = -0.4),
      encantamento = list(z_smile = 1.1, z_eye_wide = 0.5, acertos = 1.8)
    ))
}

.estados_vazio <- function() {
  tibble(session = character(), participant = character(), scene = character(), t_ini = numeric(), t_fim = numeric(),
         cobertura = numeric(), confusao = numeric(), frustracao = numeric(), tedio = numeric(), engajamento = numeric(),
         encantamento = numeric(), estado = character(), confianca = character(), erros = integer(), acertos = integer(),
         acoes = integer(), ocioso = numeric(), versao = character())
}

.garantir_cols <- function(df, cols) { for (c in cols) if (!c %in% names(df)) df[[c]] <- NA; df }

.intervalos_ocio <- function(ev) {
  ini <- ev |> filter(.data$event == "idle_start") |> select("session", ini = "t_ms") |> arrange(.data$session, .data$ini)
  fim <- ev |> filter(.data$event %in% c("idle_end", "session_end")) |> select("session", fim = "t_ms") |> arrange(.data$session, .data$fim)
  if (nrow(ini) == 0) return(tibble(session = character(), ini = numeric(), fim = numeric()))
  ini |> left_join(fim, by = join_by(session, closest(ini <= fim))) |> mutate(fim = dplyr::coalesce(.data$fim, .data$ini))
}

.fracao_ocio <- function(janelas, ocio) {
  if (nrow(ocio) == 0) return(rep(0, nrow(janelas)))
  j <- janelas |> mutate(.l = dplyr::row_number()) |> select(".l", "session", "t_ini", "t_fim")
  sob <- j |> inner_join(ocio, by = join_by(session, x$t_ini < y$fim, x$t_fim > y$ini)) |>
    mutate(s = pmin(.data$t_fim, .data$fim) - pmax(.data$t_ini, .data$ini)) |>
    group_by(.data$.l) |> summarise(s = sum(.data$s), .groups = "drop")
  out <- rep(0, nrow(janelas))
  out[sob$.l] <- pmin(1, sob$s / (janelas$t_fim[sob$.l] - janelas$t_ini[sob$.l]))
  out
}

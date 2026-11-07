# =============================================================================
# Fábulas Computacionais — simulação de dados de Learning Analytics
# -----------------------------------------------------------------------------
# Gera dados FICTÍCIOS no mesmo formato que o jogo grava:
#   events_<sessão>.jsonl   (um evento JSON por linha)
#   affect_<sessão>.csv     (amostras faciais; aqui só os índices derivados)
#   observacoes_bromp.csv   (rótulos de observação simulados, para o detector)
#
# O modelo de simulação é propositalmente simples e só serve para mostrar como
# as análises ficariam. Nenhum número aqui representa resultados reais.
#
# Uso:
#   pasta <- fabulasLA::simular_coleta(n_alunos = 100)
# =============================================================================


# ----------------------------------------------------------------- catálogo
.niveis_fabulas <- function() {
  tibble(
    puzzle = rep(c("Musical", "Espelhos", "Conjuntos", "Receitas"), each = 3),
    fase   = rep(1:3, 4),
    b      = c(-0.6, 0.9, 1.0,  0.0, 0.6, 1.1,  -0.9, -0.2, 0.3,  -0.4, 0.2, 0.7),
    n_pos  = c(8, 8, 10,  4, 5, 6,  4, 6, 8,  4, 5, 6)
  ) |>
    mutate(cena = ifelse(puzzle == "Musical", paste("Musical", fase), paste(puzzle, "Fase", fase)))
}

.notas      <- c("Do", "Re", "Mi", "Fa", "Sol", "La", "Si")
.formas     <- c("quadrado", "retangulo", "triangulo", "losango", "circulo", "hexagono")
.parceira   <- c(quadrado = "retangulo", retangulo = "quadrado", triangulo = "losango",
                 losango = "triangulo", circulo = "hexagono", hexagono = "circulo")
.insumos    <- c("tomate", "batata", "peixe", "cenoura", "ovo", "cebola")
.acoes      <- c("cortar", "fritar")
.rotacoes   <- c(0, 90, 180, 270)

.gabarito <- function(puzzle, n, fase) {
  set.seed(1000 + match(puzzle, c("Musical", "Espelhos", "Conjuntos", "Receitas")) * 10 + fase)
  switch(puzzle,
    Musical   = sample(.notas, n, TRUE),
    Espelhos  = paste0("r=", sample(.rotacoes, n, TRUE)),
    Conjuntos = sample(.formas, n, TRUE),
    Receitas  = paste0(sample(.insumos, n), "+", sample(.acoes, n, TRUE))
  )
}

# Valor errado para a posição i, de acordo com o tipo de puzzle
.valor_errado <- function(puzzle, i, gabarito, fase, serie) {
  certo <- gabarito[i]
  switch(puzzle,
    Musical  = sample(setdiff(.notas, certo), 1),
    Espelhos = paste0("r=", sample(setdiff(.rotacoes, as.numeric(sub("r=", "", certo))), 1)),
    Conjuntos = {
      p_parceira <- ifelse(serie == 3, 0.75, 0.5)
      if (runif(1) < p_parceira) unname(.parceira[certo]) else sample(setdiff(.formas, certo), 1)
    },
    Receitas = {
      partes <- strsplit(certo, "+", fixed = TRUE)[[1]]
      outros <- setdiff(sub("\\+.*", "", gabarito), partes[1])
      probs <- switch(fase, c(.25, .10, .10, .20, .20, .15), c(.12, .06, .07, .25, .20, .30), c(.05, .03, .02, .15, .17, .58))
      tipo <- sample(c("empty_step", "empty_food", "empty_action", "wrong_food", "wrong_action", "order"), 1, prob = probs)
      if (tipo == "order" && length(outros) == 0) tipo <- "wrong_food"
      switch(tipo,
        empty_step   = "+",
        empty_food   = paste0("+", partes[2]),
        empty_action = paste0(partes[1], "+"),
        wrong_food   = paste0(sample(setdiff(.insumos, partes[1]), 1), "+", partes[2]),
        wrong_action = paste0(partes[1], "+", setdiff(.acoes, partes[2])),
        order        = paste0(sample(outros, 1), "+", partes[2]))
    })
}

.tipo_erro_receita <- function(slot, certo, gabarito) {
  f <- sub("\\+.*", "", slot); a <- sub(".*\\+", "", slot)
  cf <- sub("\\+.*", "", certo); ca <- sub(".*\\+", "", certo)
  if (f == "" && a == "") return("empty_step")
  if (f == "") return("empty_food")
  if (a == "") return("empty_action")
  if (f == cf && a == ca) return("ok")
  if (f != cf && f %in% sub("\\+.*", "", gabarito)) return(if (a == ca) "order" else "order+wrong_action")
  if (f != cf && a != ca) return("wrong_food+wrong_action")
  if (f != cf) "wrong_food" else "wrong_action"
}

# ----------------------------------------------------------------- uma fase
.simular_fase <- function(al, lv, t0, add, obs) {
  n <- lv$n_pos; cena <- lv$cena; puzzle <- lv$puzzle; fase <- lv$fase
  b <- lv$b + ifelse(al$dispositivo == "teclado" && puzzle %in% c("Espelhos", "Conjuntos"), 0.35, 0) -
    ifelse(identical(al$condicao, "B"), 0.3, 0)
  gab <- lv$gab[[1]]
  p_err <- plogis(b - al$theta - 0.4)
  w <- rep(1, n); if (puzzle == "Musical" && fase >= 2) w[n - 1] <- 5   # equívoco no fim do laço
  errado <- runif(n) < pmin(0.95, p_err * w / mean(w))
  slots <- gab
  for (i in which(errado)) slots[i] <- .valor_errado(puzzle, i, gab, fase, al$serie)
  usa_rep <- !(puzzle == "Musical" && fase >= 2) || runif(1) < plogis(-1.2 + al$theta)

  fator <- switch(al$perfil, depurador = 1.6, explorador = 1, tentativa_erro = 0.6)
  t <- t0 + runif(1, 6, 25) * 1000 * fator
  if (runif(1) < plogis(0.7 * al$theta - 0.2)) add(t - 2000, "mission_panel_toggle", cena)

  tentativa <- 0; dist_ant <- NA; sem_prog <- 0; mudou <- seq_len(n); t_ant <- t0
  repeat {
    tentativa <- tentativa + 1
    # ações intermediárias antes da tentativa
    n_acoes <- length(mudou)
    if (n_acoes > 0 && puzzle != "Musical") {
      ini_acoes <- max(t_ant + 1500, t - 15000)
      ts <- sort(runif(n_acoes, ini_acoes, max(ini_acoes + 500, t - 500)))
      for (k in seq_along(mudou)) {
        i <- mudou[k]
        if (puzzle == "Conjuntos") {
          add(ts[k] - 1500, "object_pick", cena, object = paste0("obj_", i), type = slots[i], from = "")
          if (runif(1) < switch(al$perfil, depurador = .08, explorador = .30, tentativa_erro = .20)) {
            add(ts[k] - 800, "object_drop", cena, object = paste0("obj_", i))
            add(ts[k] - 600, "object_pick", cena, object = paste0("obj_", i), type = slots[i], from = "")
          }
          add(ts[k], "object_place", cena, object = paste0("obj_", i), type = slots[i],
              space = paste0("Espaco_", i), space_type = gab[i], correct = slots[i] == gab[i])
        } else if (puzzle == "Receitas") {
          add(ts[k], "recipe_place", cena, table = paste0("Mesa_", i), is_action = FALSE,
              item = sub("\\+.*", "", slots[i]), expected = sub("\\+.*", "", gab[i]),
              correct = sub("\\+.*", "", slots[i]) == sub("\\+.*", "", gab[i]))
        } else {
          add(ts[k], "pillar_rotate", cena, pillar = paste0("Pilar_", i), rotation_before = I(c(0, 0, 0)))
        }
      }
    }

    dist <- sum(slots != gab) + (!usa_rep)
    primeiro <- which(slots != gab)[1]
    add(t, "puzzle_attempt", cena, level_type = puzzle, area = fase, attempt = tentativa, time_s = (t - t0) / 1000)

    extras <- list()
    if (puzzle == "Musical") extras <- list(part1_repeat = usa_rep, repeat_times_selected = if (usa_rep) 2L else 0L,
                                            p1_repeat_start = if (usa_rep) "Rep_ini_0" else "", p1_repeat_end = if (usa_rep) paste0("Rep_fim_", n - 2) else "")
    erradas <- which(slots != gab)
    if (puzzle == "Conjuntos") extras <- list(confusions = I(if (length(erradas)) paste0(gab[erradas], "->", slots[erradas]) else character(0)),
                                              misplaced_objects = sum(slots != gab))
    if (puzzle == "Receitas") extras <- list(step_errors = I(vapply(seq_len(n), function(i) .tipo_erro_receita(slots[i], gab[i], gab), "")))
    snap <- c(list(puzzle = puzzle, attempt = tentativa, slots = I(slots), total_positions = n,
                   filled_positions = sum(!(slots %in% c("", "+")))),
              if (puzzle != "Espelhos") list(expected = I(gab), correct_positions = n - sum(slots != gab),
                                             first_error = ifelse(is.na(primeiro), -1L, primeiro - 1L), distance = dist),
              list(changed_positions = if (tentativa == 1) NULL else I(mudou - 1L),
                   n_changed = if (tentativa == 1) -1L else length(mudou), success = dist == 0),
              extras)
    do.call(add, c(list(t + 5, "solution_snapshot", cena), snap))

    if (puzzle == "Espelhos") {
      acesos <- if (is.na(primeiro)) n + 1 else primeiro
      add(t + 4000, "beam_result", cena, segments_lit = acesos, segments_total = n + 1, success = dist == 0,
          duration_s = 4)
      if (!is.na(primeiro)) add(t + 3000, "beam_blocked", cena, segment = paste0("Luz_", primeiro), hit = "Parede")
      t <- t + 4500
    }

    progresso <- !is.na(dist_ant) && dist < dist_ant
    sem_prog <- if (progresso) 0 else sem_prog + (tentativa > 1)
    # rótulo de observação (BROMP) simulado para o intervalo que antecede a tentativa
    obs(cena, tentativa, rbinom(1, 1, plogis(-2.4 + 0.8 * sem_prog + 0.18 * dist - 0.4 * al$theta + rnorm(1, 0, 0.8))), t)

    if (dist == 0) {
      if (puzzle == "Espelhos") add(t + 5500, "solution_final", cena, puzzle = "Espelhos",
                                    pillars = I(paste0("Pilar_", seq_len(n))), slots = I(gab))
      add(t + 6000, "puzzle_complete", cena, level_type = puzzle, area = fase, attempts = tentativa, time_s = (t - t0) / 1000)
      return(list(t = t + 9000, sucesso = TRUE, abandonou = FALSE, tentativas = tentativa))
    }
    p_ab <- plogis(-4.3 + 0.9 * sem_prog + 0.22 * tentativa - 0.6 * al$theta)
    if (tentativa >= 14 || runif(1) < p_ab) {
      return(list(t = t + runif(1, 3, 12) * 1000, sucesso = FALSE, abandonou = TRUE, tentativas = tentativa))
    }

    # engajamento entre tentativas
    if (runif(1) < plogis(-2.6 + 0.8 * sem_prog - 0.4 * al$theta)) {
      dur <- runif(1, 8, 60) * 1000
      add(t + 30000, "idle_start", cena, threshold_s = 30)
      add(t + 30000 + dur, "idle_end", cena, idle_ms = 30000 + dur)
      t <- t + 30000 + dur
    }
    if (sem_prog >= 2 && runif(1) < 0.3) add(t + 2000, "mission_panel_toggle", cena)
    if (runif(1) < 0.03) { add(t + 1000, "pause_open", cena); add(t + 1000 + runif(1, 5, 40) * 1000, "pause_close", cena) }

    # próxima modificação, conforme o perfil de estratégia
    E <- which(slots != gab)
    if (al$perfil == "depurador" && length(E) > 0) {
      k <- max(1, rbinom(1, length(E), plogis(al$theta - b + 0.8)))
      mudou <- sort(E[sample.int(length(E), k)]); p_c <- plogis(al$theta - b + 1.5)
      intervalo <- runif(1, 20, 45)
    } else if (al$perfil == "explorador") {
      mudou <- sort(sample.int(n, max(2, round(n * runif(1, .4, .8))))); p_c <- plogis(al$theta - b + 0.6)
      intervalo <- runif(1, 12, 30)
    } else {
      mudou <- sort(sample.int(n, max(1, round(n * runif(1, .2, .5))))); p_c <- 0.5
      intervalo <- runif(1, 5, 14)
    }
    for (i in mudou) slots[i] <- if (runif(1) < p_c) gab[i] else .valor_errado(puzzle, i, gab, fase, al$serie)
    if (!usa_rep && runif(1) < plogis(-1.4 + 0.45 * tentativa + al$theta)) usa_rep <- TRUE
    dist_ant <- dist
    t_ant <- t
    t <- t + intervalo * 1000
  }
}

# ----------------------------------------------------------------- afeto (1 Hz)
.simular_afeto <- function(al, eventos, sessao, inicio_utc, hz) {
  fim <- max(eventos$t_ms)
  t <- seq(0, fim, by = 1000 / hz)
  cenas <- eventos |> filter(event == "scene_enter") |> arrange(t_ms)
  idx <- findInterval(t, cenas$t_ms); idx[idx == 0] <- 1
  cena <- cenas$scene[idx]
  puzzle <- sub(" .*", "", cena); puzzle[grepl("^Area", cena)] <- "Area"

  kern <- function(tempos, amp, tau) {
    out <- numeric(length(t))
    for (k in seq_along(tempos)) { d <- (t - tempos[k]) / 1000; s <- d >= 0 & d < 6 * tau; out[s] <- out[s] + amp[k] * exp(-d[s] / tau) }
    out
  }
  snaps <- eventos |> filter(event == "solution_snapshot")
  suc <- vapply(snaps$data, function(d) isTRUE(d$success), TRUE)
  dist <- vapply(snaps$data, function(d) if (is.null(d$distance)) NA_real_ else d$distance, 1)
  melhora <- c(FALSE, diff(dist) < 0) & !is.na(dist)
  melhora[is.na(melhora)] <- FALSE
  t_err <- snaps$t_ms[!suc]; amp_err <- ifelse(melhora[!suc], 0.08, 0.17)
  ociosos <- eventos |> filter(event %in% c("idle_start", "idle_end")) |> arrange(t_ms)
  em_ocio <- rep(FALSE, length(t))
  if (nrow(ociosos) > 1) for (k in which(ociosos$event == "idle_start")) {
    fim_k <- ociosos$t_ms[ociosos$event == "idle_end" & ociosos$t_ms > ociosos$t_ms[k]][1]
    if (!is.na(fim_k)) em_ocio <- em_ocio | (t >= ociosos$t_ms[k] & t <= fim_k)
  }

  e <- al$expressividade; z <- function(sd) rnorm(length(t), 0, sd)
  esforco <- c(Area = 0, Musical = 0.02, Espelhos = 0.13, Conjuntos = 0.05, Receitas = 0.07)[puzzle]
  smile  <- pmin(1, pmax(0, e * (0.10 + ifelse(puzzle == "Musical", 0.06, 0) + kern(snaps$t_ms[suc], rep(0.35, sum(suc)), 8)) + z(0.04)))
  brow   <- pmin(1, pmax(0, e * (0.12 + esforco + kern(t_err, rep(0.06, length(t_err)), 15)) + z(0.04)))
  press  <- pmin(1, pmax(0, e * (0.06 + kern(t_err, amp_err, 6)) + z(0.03)))
  p_face <- (if (al$camera_ruim) 0.55 else 0.95) - 0.10 * t / max(t) - 0.35 * em_ocio
  face   <- rbinom(length(t), 1, pmin(0.99, pmax(0.05, p_face)))
  yaw    <- rnorm(length(t), 0, 10) + ifelse(em_ocio, rnorm(length(t), 0, 30), 0)
  na_if0 <- function(x) ifelse(face == 1, round(x, 4), NA)

  tibble(
    utc = format(inicio_utc + t / 1000, "%Y-%m-%dT%H:%M:%OS3Z", tz = "UTC"),
    t_ms = t, session = sessao, participant = al$participant, scene = cena, face_present = face,
    head_yaw = na_if0(yaw), head_pitch = na_if0(rnorm(length(t), -5, 6)), head_roll = na_if0(rnorm(length(t), 0, 4)),
    smile = na_if0(smile), frown = na_if0(e * 0.05 + z(0.02)), brow_down = na_if0(brow),
    brow_inner_up = na_if0(e * 0.08 + z(0.03)), brow_outer_up = na_if0(e * 0.06 + z(0.03)),
    eye_wide = na_if0(e * 0.05 + z(0.02)), eye_squint = na_if0(e * (0.15 + esforco / 2) + z(0.04)),
    eye_blink = na_if0(abs(z(0.15))), jaw_open = na_if0(abs(z(0.06))), mouth_press = na_if0(press),
    nose_sneer = na_if0(abs(z(0.02))), gaze_away = na_if0(pmin(1, abs(yaw) / 60 + abs(z(0.05))))
  )
}

# ----------------------------------------------------------------- uma sessão
.simular_sessao <- function(al, niveis, inicio_utc, hz) {
  sessao <- paste0(format(inicio_utc, "%Y%m%dT%H%M%SZ", tz = "UTC"), "-", substr(gsub("-", "", al$participant), 2, 7))
  eventos <- list(); obs <- list()
  add <- function(t, event, scene, ...) {
    d <- list(...)
    if (length(d) == 0) d <- structure(list(), names = character(0))   # vira {} no JSON, como no jogo
    eventos[[length(eventos) + 1]] <<- tibble(t_ms = round(t, 1), event = event, scene = scene, data = list(d))
  }
  add_obs <- function(cena, tentativa, confuso, t) {
    obs[[length(obs) + 1]] <<- tibble(session = sessao, participant = al$participant, scene = cena,
                                      attempt = tentativa, t_ms = t, confuso = confuso)
  }

  add(0, "session_start", "MainMenu", schema = "1.1", study_id = al$estudo, school_id = al$escola,
      class_id = al$turma, machine_id = al$maquina, condition = al$condicao, id_mode = "code",
      consent = TRUE, camera_consent = TRUE, age = al$idade, gender = al$genero,
      school_grade = paste0(al$serie, "º ano"), game_version = "0.1", platform = "WindowsPlayer",
      os = "Windows 11", device_model = al$maquina, screen = "1920x1080",
      joysticks = ifelse(al$dispositivo == "gamepad", "Xbox Controller", ""))
  t <- 800; joias <- 0; duracao <- runif(1, 35, 55) * 60000; primeira_visita <- TRUE
  for (area in 1:3) {
    if ((area == 2 && joias < 2) || (area == 3 && joias < 5) || t > duracao) break
    cena_area <- paste0("Area", area)
    add(t, "scene_enter", cena_area, scene_name = cena_area); t_area <- t
    ordem <- sample(c("Musical", "Espelhos", "Conjuntos", "Receitas"), prob = c(.22, .13, .45, .20))
    fila <- ordem; evitadas <- character()
    while (length(fila) > 0 && t < duracao) {
      pz <- fila[1]; fila <- fila[-1]
      lv <- niveis |> filter(puzzle == pz, fase == area)
      t <- t + runif(1, 8, 25) * 1000
      add(t, "door_approach", cena_area, door = lv$cena)
      p_ev <- ifelse(pz == "Espelhos" && !(pz %in% evitadas), 0.40, 0.10) - 0.08 * al$theta
      if (!(pz %in% evitadas) && runif(1) < p_ev) {
        t <- t + runif(1, 2, 6) * 1000
        add(t, "door_leave", cena_area, door = lv$cena)
        evitadas <- c(evitadas, pz); fila <- c(fila, pz); next
      }
      add(t + 1500, "door_enter", cena_area, door = lv$cena)
      add(t + 1600, "scene_exit", cena_area, scene_name = cena_area, time_in_scene_ms = t + 1600 - t_area)
      t <- t + 3600
      add(t, "scene_enter", lv$cena, scene_name = lv$cena)
      add(t + 100, "puzzle_start", lv$cena, level_type = pz, area = area, previous_attempts = 0L,
          previous_time_s = 0, already_won = FALSE)
      r <- .simular_fase(al, lv, t + 100, add, add_obs)
      if (r$sucesso) { joias <- joias + 1; add(r$t - 2000, "jewel_earned", lv$cena, level = lv$cena, total_jewels = joias, game_time_s = r$t / 1000) }
      add(r$t, "scene_exit", lv$cena, scene_name = lv$cena, time_in_scene_ms = r$t - t)
      t <- r$t + 3000
      add(t, "scene_enter", cena_area, scene_name = cena_area); t_area <- t
    }
  }
  add(t + 2000, "session_end", "MainMenu", reason = ifelse(t > duracao, "app_quit", "back_to_menu"),
      duration_ms = t + 2000, recovered = FALSE)

  ev <- bind_rows(eventos) |> arrange(t_ms)
  if (isTRUE(al$interrompida)) {
    # queda no meio da sessão: o jogo reconstrói o fim na abertura seguinte
    corte <- runif(1, 0.3, 0.8) * max(ev$t_ms)
    ev <- ev |> filter(t_ms <= corte, event != "session_end")
    ultimo <- max(ev$t_ms)
    ev <- bind_rows(ev, tibble(t_ms = ultimo, event = "session_end", scene = "",
                               data = list(list(reason = "interrupted", duration_ms = round(ultimo), recovered = TRUE))))
  }
  afeto <- .simular_afeto(al, ev, sessao, inicio_utc, hz)

  # resumo afetivo a cada 10 s, como o AffectManager faz
  resumo <- afeto |> mutate(bloco = floor(t_ms / 10000)) |> group_by(bloco) |>
    summarise(t_ms = max(t_ms), scene = last(scene), samples = n(), face_ratio = mean(face_present),
              smile = mean(smile, na.rm = TRUE), brow_down = mean(brow_down, na.rm = TRUE),
              mouth_press = mean(mouth_press, na.rm = TRUE), .groups = "drop")
  ev_resumo <- tibble(t_ms = resumo$t_ms, event = "affect_summary", scene = resumo$scene,
                      data = lapply(seq_len(nrow(resumo)), function(i) list(
                        samples = resumo$samples[i], face_ratio = round(resumo$face_ratio[i], 3), paused = FALSE,
                        means = list(smile = round(resumo$smile[i], 4), brow_down = round(resumo$brow_down[i], 4),
                                     mouth_press = round(resumo$mouth_press[i], 4)))))
  ev_cap <- tibble(t_ms = 50, event = "affect_capture_start", scene = "MainMenu",
                   data = list(list(camera = "USB Camera", resolution = "640x480", sample_rate_hz = hz,
                                    model = "face_landmarker_v2_with_blendshapes.bytes")))
  fim_sessao <- ev |> filter(event == "session_end")
  fim_sessao$t_ms <- max(c(fim_sessao$t_ms, ev$t_ms[ev$event != "session_end"]))   # o fim é sempre o último instante
  ev <- bind_rows(ev |> filter(event != "session_end"), ev_resumo |> filter(t_ms <= max(fim_sessao$t_ms)), ev_cap) |>
    arrange(t_ms) |> bind_rows(fim_sessao) |>
    mutate(id = .hex32(n()), seq = row_number(),
           t = format(inicio_utc + t_ms / 1000, "%Y-%m-%dT%H:%M:%OS3Z", tz = "UTC"),
           session = sessao, participant = al$participant) |>
    select(id, seq, t, t_ms, session, participant, scene, event, data)
  afeto <- afeto |> filter(t_ms <= max(fim_sessao$t_ms))

  list(sessao = sessao, eventos = ev, afeto = afeto, obs = bind_rows(obs))
}

# ----------------------------------------------------------------- principal
#' Simula uma coleta completa (dados fictícios) no mesmo formato que o jogo grava.
#'
#' Útil para testar análises e o relatório antes de ter dados reais. Gera também
#' observacoes_bromp.csv (rótulos de observação simulados) e _verdade_simulacao.csv
#' (variáveis latentes usadas na geração, para validar os métodos).
#'
#' @param n_alunos número de estudantes (uma sessão cada)
#' @param pasta pasta de saída (é limpa antes)
#' @param seed semente
#' @param hz amostras faciais por segundo
#' @param prop_interrompidas proporção de sessões interrompidas (queda/travamento)
#' @param estudo identificador do estudo
#' @return o caminho da pasta, invisível
simular_coleta <- function(n_alunos = 100, pasta = file.path(tempdir(), "LearningAnalytics_simulado"),
                           seed = 2026, hz = 1, prop_interrompidas = 0.05, estudo = "SIM-2026") {
  # Gabaritos fixos por fase (gerados antes de definir a semente da simulação dos alunos)
  niveis <- .niveis_fabulas()
  niveis$gab <- lapply(seq_len(nrow(niveis)), function(i) .gabarito(niveis$puzzle[i], niveis$n_pos[i], niveis$fase[i]))
  set.seed(seed)
  dir.create(pasta, recursive = TRUE, showWarnings = FALSE)
  unlink(list.files(pasta, full.names = TRUE))

  escolas <- c("Escola A", "Escola B", "Escola C", "Escola D")
  efeito_escola <- c(0.25, 0, -0.15, -0.35)
  alunos <- tibble(
    participant = gerar_codigos(n_alunos),
    nome = sprintf("ALUNO %03d", seq_len(n_alunos)),
    escola = sample(escolas, n_alunos, TRUE),
    serie = sample(3:5, n_alunos, TRUE),
    genero = sample(c("f", "m", "outro"), n_alunos, TRUE, prob = c(.48, .48, .04)),
    dispositivo = sample(c("gamepad", "teclado"), n_alunos, TRUE, prob = c(.6, .4)),
    maquina = sprintf("PC-%02d", sample(1:8, n_alunos, TRUE))
  ) |>
    mutate(idade = serie + 5 + rbinom(n(), 1, .3),
           theta = rnorm(n()) + 0.4 * (serie - 4) + efeito_escola[match(escola, escolas)],
           perfil = ifelse(runif(n()) < plogis(0.9 * theta - 0.1), "depurador",
                           ifelse(runif(n()) < 0.6, "explorador", "tentativa_erro")),
           expressividade = rlnorm(n(), 0, 0.3),
           camera_ruim = maquina %in% c("PC-03", "PC-07"),
           estudo = estudo, turma = paste0(sub("Escola ", "", escola), serie),
           condicao = sample(c("A", "B"), n(), TRUE),
           interrompida = runif(n()) < prop_interrompidas)

  inicio <- as.POSIXct("2026-11-09 12:00:00", tz = "UTC")
  obs_todas <- list()
  for (i in seq_len(n_alunos)) {
    al <- alunos[i, ]
    inicio_utc <- inicio + (i %/% 25) * 86400 + (i %% 25) * 50 * 60
    s <- .simular_sessao(al, niveis, inicio_utc, hz)
    con <- file(file.path(pasta, paste0("events_", s$sessao, ".jsonl")), open = "w", encoding = "UTF-8")
    jsonlite::stream_out(s$eventos, con, verbose = FALSE, auto_unbox = TRUE, null = "null", na = "null", digits = NA)
    close(con)
    write_csv(s$afeto, file.path(pasta, paste0("affect_", s$sessao, ".csv")), na = "")
    obs_todas[[i]] <- s$obs
  }

  # Modo "code": como no jogo, nenhum nome é gravado (a lista nome <-> código fica em papel).
  write_csv(bind_rows(obs_todas), file.path(pasta, "observacoes_bromp.csv"))
  # Só existe na simulação: variáveis latentes usadas para gerar os dados (para validar as análises)
  write_csv(alunos |> select(participant, escola, maquina, condicao, theta, perfil, camera_ruim, interrompida),
            file.path(pasta, "_verdade_simulacao.csv"))
  invisible(pasta)
}

.hex32 <- function(n) vapply(seq_len(n), function(i) paste(sample(c(0:9, letters[1:6]), 32, TRUE), collapse = ""), "")

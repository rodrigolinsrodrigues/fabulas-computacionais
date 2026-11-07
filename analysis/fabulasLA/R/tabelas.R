#' Monta as tabelas de análise a partir de uma coleta.
#'
#' @param coleta objeto de ler_coleta()
#' @return lista de tibbles:
#'   sessoes   — uma linha por sessão (perfil, estudo, escola, turma, condição, desfecho)
#'   fases     — uma linha por participante x fase (tentativas, tempo, conclusão, abandono)
#'   tentativas — uma linha por tentativa com o estado da solução (distância, posições alteradas...)
#'   afeto_sessao — cobertura e médias faciais por sessão
tabelas_analise <- function(coleta) {
  ev <- .garantir(coleta$eventos, c("level_type", "area", "attempt", "attempts", "time_s", "door", "distance",
    "n_changed", "success", "segments_lit", "segments_total", "part1_repeat", "idle_ms", "reason", "schema",
    "study_id", "school_id", "class_id", "machine_id", "condition", "age", "gender", "school_grade",
    "school_name", "device_model", "joysticks", "total_positions", "recovered", "puzzle"))

  inicio <- ev |> filter(.data$event == "session_start")
  fim <- ev |> filter(.data$event == "session_end") |> group_by(.data$session) |> slice(1) |> ungroup() |>
    transmute(session = .data$session, motivo_fim = .data$reason, duracao_min = .data$duration_ms / 60000)
  sessoes <- inicio |>
    transmute(session = .data$session, participant = .data$participant, inicio_utc = .data$t,
              estudo = .data$study_id, escola = .data$school_id, escola_informada = .data$school_name, turma = .data$class_id,
              maquina = coalesce(na_if(.data$machine_id, ""), .data$device_model),
              condicao = .data$condition, idade = .data$age, genero = .data$gender, serie = .data$school_grade,
              dispositivo = ifelse(.data$joysticks %in% c("", NA), "teclado/mouse", "gamepad"), schema = .data$schema) |>
    left_join(fim, by = "session") |>
    mutate(escola_informada = ifelse(is.na(.data$escola_informada), "", trimws(.data$escola_informada)),
           # escola do estudo (research_config); sem ela, a escola digitada pelo estudante no cadastro
           escola = ifelse(is.na(.data$escola) | .data$escola == "",
                           ifelse(.data$escola_informada == "", "Não informada", .data$escola_informada), .data$escola),
           condicao = ifelse(is.na(.data$condicao) | .data$condicao == "", "(sem condição)", .data$condicao),
           interrompida = .data$motivo_fim %in% "interrupted")

  saidas <- ev |> filter(.data$event == "scene_exit", !is.na(.puzzle_de(.data$scene))) |>
    group_by(.data$session, .data$scene) |> summarise(t_saida = first(.data$t_ms), .groups = "drop")
  fases <- ev |> filter(.data$event == "puzzle_start") |>
    group_by(.data$session, .data$scene) |> slice(1) |> ungroup() |>
    transmute(session = .data$session, participant = .data$participant, scene = .data$scene,
              puzzle = .puzzle_de(.data$scene), fase = .fase_de(.data$scene), t_ini = .data$t_ms) |>
    left_join(ev |> filter(.data$event == "puzzle_complete") |> group_by(.data$session, .data$scene) |> slice(1) |> ungroup() |>
                transmute(session = .data$session, scene = .data$scene, t_fim = .data$t_ms, tempo_s = .data$time_s),
              by = c("session", "scene")) |>
    left_join(ev |> filter(.data$event == "puzzle_attempt") |> count(.data$session, .data$scene, name = "tentativas"),
              by = c("session", "scene")) |>
    left_join(saidas, by = c("session", "scene")) |>
    mutate(tentativas = replace_na(.data$tentativas, 0L), concluiu = !is.na(.data$t_fim), abandonou = !.data$concluiu,
           tempo_evento_s = ifelse(.data$concluiu, .data$tempo_s, (.data$t_saida - .data$t_ini) / 1000)) |>
    left_join(sessoes |> select("session", "escola", "turma", "condicao", "serie", "genero", "dispositivo", "idade"),
              by = "session")

  snap <- ev |> filter(.data$event == "solution_snapshot")
  lista <- function(nome) if (nome %in% names(snap)) snap[[nome]] else rep(list(NULL), nrow(snap))
  tentativas <- snap |>
    transmute(session = .data$session, participant = .data$participant, scene = .data$scene, t_ms = .data$t_ms,
              puzzle = .data$puzzle, fase = .fase_de(.data$scene), attempt = .data$attempt,
              total_positions = .data$total_positions, distance = .data$distance, n_changed = .data$n_changed,
              success = .data$success, part1_repeat = .data$part1_repeat)
  tentativas$slots <- lista("slots")
  tentativas$expected <- lista("expected")
  tentativas$confusions <- lista("confusions")
  tentativas$step_errors <- lista("step_errors")

  finais <- ev |> filter(.data$event == "solution_final")
  if (nrow(finais) > 0) {
    finais <- tibble(session = finais$session, scene = finais$scene, final = finais$slots)
    tentativas <- tentativas |> left_join(finais, by = c("session", "scene")) |>
      mutate(distance = ifelse(.data$puzzle == "Espelhos" & !map_lgl(.data$final, is.null),
                               map2_dbl(.data$slots, .data$final, ~ sum(unlist(.x) != unlist(.y))), .data$distance))
  }
  tentativas <- tentativas |>
    mutate(dist_rel = .data$distance / .data$total_positions) |>
    group_by(.data$session, .data$scene) |> arrange(.data$attempt, .by_group = TRUE) |>
    mutate(delta = .data$distance - lag(.data$distance), intervalo_s = (.data$t_ms - lag(.data$t_ms)) / 1000) |>
    ungroup()

  af <- coleta$afeto
  afeto_sessao <- if (nrow(af) > 0) af |> group_by(.data$session, .data$participant) |>
    summarise(amostras = n(), cobertura = mean(.data$face_present),
              across(any_of(c("smile", "brow_down", "mouth_press")), ~ mean(.x, na.rm = TRUE)), .groups = "drop") else tibble()

  list(sessoes = sessoes, fases = fases, tentativas = tentativas, afeto_sessao = afeto_sessao)
}

.garantir <- function(df, cols) {
  for (c in cols) if (!c %in% names(df)) df[[c]] <- NA
  df
}

.puzzle_de <- function(cena) case_when(str_detect(cena, "^Musical") ~ "Musical", str_detect(cena, "^Espelhos") ~ "Espelhos",
                                       str_detect(cena, "^Conjuntos") ~ "Conjuntos", str_detect(cena, "^Receitas") ~ "Receitas",
                                       TRUE ~ NA_character_)
.fase_de <- function(cena) suppressWarnings(as.integer(str_extract(cena, "\\d+$")))

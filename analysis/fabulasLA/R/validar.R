#' Confere a qualidade e a integridade de uma coleta.
#'
#' Verifica: integridade dos arquivos (SHA-256 do manifesto), eventos duplicados, lacunas na
#' sequência, sessões sem início ou sem fim, sessões interrompidas, eventos fora do catálogo,
#' tempos fora de ordem, versões do esquema, participantes em mais de uma condição e
#' cobertura da câmera.
#'
#' @param coleta objeto de ler_coleta()
#' @param cobertura_minima proporção mínima de amostras com rosto para considerar a sessão facial válida
#' @return tibble com verificacao, status ("ok", "aviso", "erro"), detalhe e n (classe fabulas_validacao)
validar_coleta <- function(coleta, cobertura_minima = 0.6) {
  ev <- coleta$eventos
  if (nrow(ev) > 0) ev <- .garantir(ev, c("id", "seq", "reason", "schema", "condition", "t_ms"))
  res <- list()
  add <- function(verificacao, status, detalhe, n = NA_integer_)
    res[[length(res) + 1]] <<- tibble(verificacao = verificacao, status = status, detalhe = detalhe, n = as.integer(n))

  if (nrow(ev) == 0) {
    add("Eventos lidos", "erro", "Nenhum evento encontrado.", 0)
    return(structure(bind_rows(res), class = c("fabulas_validacao", "tbl_df", "tbl", "data.frame")))
  }
  add("Eventos lidos", "ok", sprintf("%s eventos de %d sessões e %d participantes",
      format(nrow(ev), big.mark = ".", decimal.mark = ","), n_distinct(ev$session), n_distinct(ev$participant)), nrow(ev))

  # Integridade (manifest.json dos .zip)
  arq <- coleta$arquivos
  if (any(!is.na(arq$sha_ok))) {
    ruins <- arq$nome[!is.na(arq$sha_ok) & !arq$sha_ok]
    if (length(ruins)) add("Integridade dos arquivos", "erro", paste("Checksum diferente do manifesto:", paste(ruins, collapse = ", ")), length(ruins))
    else add("Integridade dos arquivos", "ok", sprintf("%d arquivos conferidos com o manifesto (SHA-256)", sum(!is.na(arq$sha_ok))), sum(!is.na(arq$sha_ok)))
  } else add("Integridade dos arquivos", "aviso", "Sem manifesto (pasta lida diretamente): integridade não conferida.")

  # Duplicatas
  com_id <- ev |> filter(!is.na(.data$id))
  dup <- sum(duplicated(com_id$id))
  if (dup > 0) add("Eventos duplicados", "erro", sprintf("%d eventos com id repetido", dup), dup)
  else if (nrow(com_id) > 0) add("Eventos duplicados", "ok", "Nenhum id repetido", 0)

  # Versões e legado (schema 1.0 não tem id/seq)
  versoes <- ev |> filter(.data$event == "session_start") |> pull("schema") |> unique()
  legado <- ev |> group_by(.data$session) |> summarise(sem_seq = all(is.na(.data$seq)), .groups = "drop") |> filter(.data$sem_seq)
  add("Versão do esquema", if (nrow(legado) > 0) "aviso" else "ok",
      paste0("Versões: ", paste(sort(versoes), collapse = ", "),
             if (nrow(legado) > 0) sprintf(" — %d sessões sem id/seq (esquema antigo): sequência não conferida", nrow(legado)) else ""),
      length(versoes))

  # Lacunas de sequência e tempo fora de ordem
  seqs <- ev |> filter(!is.na(.data$seq)) |> group_by(.data$session) |>
    summarise(n = n(), maximo = max(.data$seq), repetidos = sum(duplicated(.data$seq)),
              tempo_volta = sum(diff(.data$t_ms[order(.data$seq)]) < -1), .groups = "drop")
  lac <- seqs |> filter(.data$maximo != .data$n | .data$repetidos > 0)
  if (nrow(lac) > 0) add("Sequência de eventos", "erro",
      sprintf("%d sessões com lacunas/repetições na sequência (eventos perdidos?): %s", nrow(lac), paste(head(lac$session, 5), collapse = ", ")), nrow(lac))
  else if (nrow(seqs) > 0) add("Sequência de eventos", "ok", "Sequências contínuas (1, 2, 3, ...) em todas as sessões", 0)
  volta <- seqs |> filter(.data$tempo_volta > 0)
  if (nrow(volta) > 0) add("Ordem temporal", "aviso", sprintf("%d sessões com t_ms voltando no tempo", nrow(volta)), nrow(volta))

  # Início e fim de sessão
  por_sessao <- ev |> group_by(.data$session) |>
    summarise(inicio = any(.data$event == "session_start"), fim = any(.data$event == "session_end"),
              interrompida = any(.data$event == "session_end" & .data$reason %in% "interrupted"), .groups = "drop")
  sem_inicio <- sum(!por_sessao$inicio)
  if (sem_inicio > 0) add("Início das sessões", "erro", sprintf("%d sessões sem session_start (arquivo incompleto)", sem_inicio), sem_inicio)
  else add("Início das sessões", "ok", "Todas as sessões têm session_start", 0)
  sem_fim <- sum(!por_sessao$fim)
  if (sem_fim > 0) add("Fim das sessões", "aviso", sprintf("%d sessões sem session_end (em andamento ou jogo fechado antes de reabrir)", sem_fim), sem_fim)
  else add("Fim das sessões", "ok", "Todas as sessões foram encerradas", 0)
  inter <- sum(por_sessao$interrompida)
  if (inter > 0) add("Sessões interrompidas", "aviso",
      sprintf("%d sessões terminaram sem encerramento normal (queda/travamento); fim reconstruído na abertura seguinte", inter), inter)
  else add("Sessões interrompidas", "ok", "Nenhuma", 0)

  # Catálogo
  conhecidos <- vapply(catalogo_eventos()$events, function(e) e$name, "")
  fora <- setdiff(unique(ev$event), conhecidos)
  if (length(fora) > 0) add("Eventos fora do catálogo", "aviso", paste(fora, collapse = ", "), length(fora))
  else add("Eventos fora do catálogo", "ok", "Todos os tipos de evento estão documentados", 0)

  # Condições
  if ("condition" %in% names(ev)) {
    cond <- ev |> filter(.data$event == "session_start", !is.na(.data$condition), .data$condition != "") |>
      distinct(.data$participant, .data$condition) |> count(.data$participant) |> filter(.data$n > 1)
    if (nrow(cond) > 0) add("Condições experimentais", "aviso", sprintf("%d participantes aparecem em mais de uma condição", nrow(cond)), nrow(cond))
  }

  # Câmera
  af <- coleta$afeto
  if (nrow(af) > 0) {
    cob <- af |> group_by(.data$session) |> summarise(face = mean(.data$face_present), .groups = "drop")
    baixas <- sum(cob$face < cobertura_minima)
    add("Cobertura da câmera", if (baixas > 0) "aviso" else "ok",
        sprintf("%d de %d sessões com rosto detectado em menos de %d%% das amostras", baixas, nrow(cob), round(cobertura_minima * 100)), baixas)
  } else add("Cobertura da câmera", "aviso", "Sem dados faciais (câmera desligada ou indisponível)", 0)

  structure(bind_rows(res), class = c("fabulas_validacao", "tbl_df", "tbl", "data.frame"))
}

#' @export
print.fabulas_validacao <- function(x, ...) {
  icone <- c(ok = "[OK]   ", aviso = "[AVISO]", erro = "[ERRO] ")
  cat("Validação da coleta\n")
  for (i in seq_len(nrow(x))) cat(icone[[x$status[i]]], " ", x$verificacao[i], ": ", x$detalhe[i], "\n", sep = "")
  n_err <- sum(x$status == "erro"); n_av <- sum(x$status == "aviso")
  cat(sprintf("\n%d erro(s), %d aviso(s).\n", n_err, n_av))
  invisible(x)
}

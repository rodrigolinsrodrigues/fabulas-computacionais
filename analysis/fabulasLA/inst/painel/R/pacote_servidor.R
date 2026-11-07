# GERADO AUTOMATICAMENTE por analysis/sincronizar_painel.R a partir de
# analysis/fabulasLA/R/servidor.R. NÃO EDITE AQUI: edite o original e rode o script de novo.

#' Baixa a coleta de um estudo do servidor central (Supabase) e lê como ler_coleta().
#'
#' O jogo envia ao servidor todos os eventos e todas as amostras faciais (as cópias locais
#' continuam nos computadores). Esta função baixa tudo em páginas, recria os arquivos
#' `events_<sessão>.jsonl` e `affect_<sessão>.csv` numa pasta e chama [ler_coleta()].
#' O resultado é idêntico ao de ler as pastas dos computadores — inclusive é possível
#' passar a pasta baixada junto com .zip de escolas que coletaram sem internet.
#'
#' @param url URL do projeto Supabase (`https://<projeto>.supabase.co`)
#' @param chave_publica chave pública (anon/publishable) do projeto
#' @param estudo identificador do estudo (studyId)
#' @param chave_leitura chave de LEITURA do estudo (arquivo `Pesquisa/servidor_<estudo>.txt`).
#'   Por segurança, prefira guardá-la na variável de ambiente `FABULAS_CHAVE_LEITURA`
#'   (ex.: no arquivo `.Renviron`) em vez de escrevê-la no script.
#' @param pasta onde gravar os arquivos baixados (padrão: pasta temporária). Use uma pasta
#'   fixa para guardar um retrato da coleta (os arquivos são sobrescritos a cada download).
#' @param afeto baixar também as amostras faciais (podem ser muitas)
#' @param verificar_integridade repassado a [ler_coleta()]
#' @param .post função de transporte (uso interno e testes)
#' @return objeto "fabulas_coleta" (ver [ler_coleta()]); a pasta fica em `attr(x, "pasta")`
#' @export
ler_coleta_servidor <- function(url, chave_publica, estudo,
                                chave_leitura = Sys.getenv("FABULAS_CHAVE_LEITURA"),
                                pasta = NULL, afeto = TRUE, verificar_integridade = TRUE,
                                .post = .post_supabase) {
  if (!nzchar(chave_leitura)) stop("Informe a chave de leitura do estudo (ou defina FABULAS_CHAVE_LEITURA).")
  if (grepl("^sb_secret_|service_role", chave_publica))
    stop("Use a chave PÚBLICA (anon/publishable), não a secreta (service_role).")
  pasta <- pasta %||% file.path(tempfile("fabulas_servidor_"), estudo)
  dir.create(pasta, recursive = TRUE, showWarnings = FALSE)
  chamar <- function(rpc, corpo) .post(url, chave_publica, rpc, corpo)

  # ---- Eventos (paginados por sessão + seq)
  eventos <- .baixar_paginas(chamar, "fc_export_events", estudo, chave_leitura, "p_after_seq", "seq")
  sessoes_ev <- vapply(eventos, function(e) e$session %||% "", character(1))
  for (s in unique(sessoes_ev)) {
    linhas <- vapply(eventos[sessoes_ev == s], .json_linha, character(1))
    .gravar_linhas(file.path(pasta, paste0("events_", s, ".jsonl")), linhas)
  }

  # ---- Amostras faciais (paginadas por sessão + idx)
  n_afeto <- 0L
  if (afeto) {
    amostras <- .baixar_paginas(chamar, "fc_export_affect", estudo, chave_leitura, "p_after_idx", "idx")
    n_afeto <- length(amostras)
    sessoes_af <- vapply(amostras, function(a) a$session %||% "", character(1))
    for (s in unique(sessoes_af)) {
      tab <- dplyr::bind_rows(lapply(amostras[sessoes_af == s], function(a) {
        a <- lapply(a, function(v) if (length(v) == 0) NA else v)
        tibble::as_tibble(a)
      }))
      tab <- tab[order(tab$idx), setdiff(names(tab), "idx")]
      fixas <- c("utc", "t_ms", "session", "participant", "scene", "face_present", "head_yaw", "head_pitch", "head_roll")
      bs <- sort(grep("^bs_", names(tab), value = TRUE))
      outras <- setdiff(names(tab), c(fixas, bs))
      tab <- tab[, c(intersect(fixas, names(tab)), outras, bs)]
      readr::write_csv(tab, file.path(pasta, paste0("affect_", s, ".csv")), na = "")
    }
  }

  message(sprintf("Baixados do servidor: %d sessões, %d eventos, %d amostras faciais -> %s",
                  length(unique(sessoes_ev)), length(eventos), n_afeto, pasta))
  coleta <- ler_coleta(pasta, verificar_integridade = verificar_integridade)
  attr(coleta, "pasta") <- pasta
  coleta
}

#' Resumo do estudo no servidor (sessões recebidas, eventos, amostras, se terminaram).
#' @inheritParams ler_coleta_servidor
#' @return tibble com uma linha por sessão
#' @export
status_servidor <- function(url, chave_publica, estudo,
                            chave_leitura = Sys.getenv("FABULAS_CHAVE_LEITURA"), .post = .post_supabase) {
  r <- .post(url, chave_publica, "fc_study_overview",
             list(p_study = estudo, p_key = chave_leitura))
  if (length(r) == 0) return(tibble())
  dplyr::bind_rows(lapply(r, function(s) tibble::as_tibble(lapply(s, function(v) if (length(v) == 0) NA else v))))
}

.baixar_paginas <- function(chamar, rpc, estudo, chave, campo_pos, campo_item) {
  tudo <- list()
  depois_sessao <- ""
  depois_pos <- 0
  repeat {
    corpo <- list(p_study = estudo, p_key = chave, p_after_session = depois_sessao, p_limit = 1000)
    corpo[[campo_pos]] <- depois_pos
    pagina <- chamar(rpc, corpo)
    if (length(pagina) == 0) break
    tudo <- c(tudo, pagina)
    ultimo <- pagina[[length(pagina)]]
    depois_sessao <- ultimo$session
    depois_pos <- ultimo[[campo_item]]
    if (length(pagina) < 1000) break
  }
  tudo
}

.json_linha <- function(e) {
  if (is.list(e$data) && length(e$data) == 0) e$data <- structure(list(), names = character(0))
  as.character(jsonlite::toJSON(e, auto_unbox = TRUE, null = "null", digits = NA))
}

.gravar_linhas <- function(caminho, linhas) {
  con <- file(caminho, open = "wb")
  on.exit(close(con))
  writeBin(charToRaw(enc2utf8(paste0(paste(linhas, collapse = "\n"), "\n"))), con)
}

# Chamada RPC ao PostgREST do Supabase; devolve a resposta já convertida (listas).
.post_supabase <- function(url, chave_publica, rpc, corpo) {
  if (!requireNamespace("curl", quietly = TRUE)) stop("Instale o pacote 'curl': install.packages('curl')")
  h <- curl::new_handle()
  cab <- list(apikey = chave_publica, `Content-Type` = "application/json", Accept = "application/json")
  if (startsWith(chave_publica, "eyJ")) cab$Authorization <- paste("Bearer", chave_publica)
  do.call(curl::handle_setheaders, c(list(h), cab))
  curl::handle_setopt(h, postfields = as.character(jsonlite::toJSON(corpo, auto_unbox = TRUE)))
  r <- curl::curl_fetch_memory(paste0(sub("/+$", "", url), "/rest/v1/rpc/", rpc), handle = h)
  txt <- rawToChar(r$content)
  Encoding(txt) <- "UTF-8"
  if (r$status_code >= 300) {
    msg <- tryCatch(jsonlite::fromJSON(txt)$message, error = function(e) txt)
    if (grepl("chave", msg %||% "")) msg <- paste(msg, "(confira o estudo e a chave de LEITURA)")
    stop(sprintf("Servidor respondeu %d em %s: %s", r$status_code, rpc, msg))
  }
  jsonlite::fromJSON(txt, simplifyVector = FALSE)
}

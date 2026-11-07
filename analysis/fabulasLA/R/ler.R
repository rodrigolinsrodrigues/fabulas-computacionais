#' Lê uma coleta do Fábulas Computacionais.
#'
#' Aceita pastas LearningAnalytics (como o jogo grava), arquivos .zip exportados pelo
#' Painel do Pesquisador, ou uma mistura de vários — por exemplo, um .zip por escola.
#' Arquivos repetidos em exportações diferentes (a mesma sessão exportada duas vezes)
#' são lidos uma única vez (fica a versão mais completa).
#'
#' @param caminhos vetor de pastas e/ou arquivos .zip
#' @param verificar_integridade confere o SHA-256 de cada arquivo contra o manifest.json dos .zip
#' @return objeto "fabulas_coleta": lista com eventos, afeto, arquivos, manifestos e extras
ler_coleta <- function(caminhos, verificar_integridade = TRUE) {
  caminhos <- normalizePath(caminhos, mustWork = TRUE)
  tmp <- tempfile("fabulas_")
  dir.create(tmp)
  arquivos <- list()
  manifestos <- list()

  for (c in caminhos) {
    if (dir.exists(c)) {
      soltos <- list.files(c, "^(events_.*\\.jsonl|affect_.*\\.csv)$", full.names = TRUE, recursive = TRUE)
      soltos <- soltos[!grepl("[/\\\\]Exportacoes[/\\\\]", soltos)]
      arquivos[[length(arquivos) + 1]] <- tibble(caminho = soltos, fonte = c, sha_manifesto = NA_character_)
      zips <- list.files(c, "\\.zip$", full.names = TRUE, recursive = TRUE)
    } else if (grepl("\\.zip$", c, ignore.case = TRUE)) {
      zips <- c
    } else {
      stop("Não reconheço ", c, " (use uma pasta ou um .zip exportado pelo jogo).")
    }
    for (z in zips) {
      destino <- file.path(tmp, sprintf("z%03d", length(manifestos) + length(arquivos) + 1))
      dir.create(destino)
      unzip(z, exdir = destino)
      man <- NULL
      if (file.exists(file.path(destino, "manifest.json"))) {
        man <- jsonlite::fromJSON(file.path(destino, "manifest.json"))
        manifestos[[length(manifestos) + 1]] <- tibble(
          zip = basename(z), study_id = man$study_id %||% NA, school_id = man$school_id %||% NA,
          class_id = man$class_id %||% NA, machine_id = man$machine_id %||% NA,
          exported_utc = man$exported_utc %||% NA, game_version = man$game_version %||% NA,
          n_arquivos = NROW(man$files))
      }
      dentro <- list.files(destino, "^(events_.*\\.jsonl|affect_.*\\.csv)$", full.names = TRUE)
      sha <- if (!is.null(man) && NROW(man$files) > 0) man$files$sha256[match(basename(dentro), man$files$name)] else NA_character_
      arquivos[[length(arquivos) + 1]] <- tibble(caminho = dentro, fonte = z, sha_manifesto = sha)
    }
  }

  arquivos <- bind_rows(arquivos)
  if (nrow(arquivos) == 0) stop("Nenhum arquivo events_*.jsonl ou affect_*.csv encontrado em: ", paste(caminhos, collapse = ", "))
  arquivos <- arquivos |>
    mutate(nome = basename(.data$caminho), bytes = file.size(.data$caminho),
           sha_ok = if (verificar_integridade && requireNamespace("openssl", quietly = TRUE))
             ifelse(is.na(.data$sha_manifesto), NA, vapply(.data$caminho, .sha256, "") == .data$sha_manifesto) else NA)
  # mesma sessão em mais de uma exportação: fica o arquivo maior (mais completo)
  usados <- arquivos |> group_by(.data$nome) |> slice_max(.data$bytes, n = 1, with_ties = FALSE) |> ungroup()

  ev_arqs <- usados$caminho[grepl("^events_", usados$nome)]
  af_arqs <- usados$caminho[grepl("^affect_", usados$nome)]

  extras <- list()
  for (c in caminhos[dir.exists(caminhos)]) {
    for (f in c("observacoes_bromp.csv", "_verdade_simulacao.csv"))
      if (file.exists(file.path(c, f))) extras[[sub("\\.csv$", "", f)]] <- read_csv(file.path(c, f), show_col_types = FALSE)
  }

  estrutura <- list(
    eventos = .ler_eventos(ev_arqs),
    afeto = .ler_afeto(af_arqs),
    arquivos = arquivos,
    manifestos = bind_rows(manifestos),
    extras = extras
  )
  class(estrutura) <- "fabulas_coleta"
  estrutura
}

.sha256 <- function(caminho) {
  con <- file(caminho, "rb")
  on.exit(close(con))
  as.character(openssl::sha256(con))
}

.ler_eventos <- function(arqs) {
  if (length(arqs) == 0) return(tibble())
  linhas <- unlist(lapply(arqs, readLines, encoding = "UTF-8", warn = FALSE))
  linhas <- linhas[nzchar(trimws(linhas))]
  linhas <- sub('"data":\\[\\]', '"data":{}', linhas)
  ev <- jsonlite::fromJSON(paste0("[", paste(linhas, collapse = ","), "]"), simplifyVector = TRUE, flatten = TRUE)
  ev <- as_tibble(ev)
  names(ev) <- sub("^data\\.", "", names(ev))
  if (!"seq" %in% names(ev)) ev$seq <- NA_real_
  if (!"id" %in% names(ev)) ev$id <- NA_character_
  ev
}

.ler_afeto <- function(arqs) {
  if (length(arqs) == 0) return(tibble())
  map_dfr(arqs, read_csv, show_col_types = FALSE, col_types = cols(utc = col_character(), .default = "?"))
}

#' @export
print.fabulas_coleta <- function(x, ...) {
  ev <- x$eventos
  cat("<Coleta Fábulas Computacionais>\n")
  cat("  Eventos:   ", format(nrow(ev), big.mark = ".", decimal.mark = ","), "\n")
  if (nrow(ev) > 0) {
    cat("  Sessões:   ", n_distinct(ev$session), "\n")
    cat("  Participantes:", n_distinct(ev$participant), "\n")
  }
  cat("  Amostras faciais:", format(nrow(x$afeto), big.mark = ".", decimal.mark = ","), "\n")
  cat("  Arquivos lidos:", sum(!duplicated(x$arquivos$nome)), "de", nrow(x$arquivos), "encontrados\n")
  if (nrow(x$manifestos) > 0) cat("  Exportações (.zip):", nrow(x$manifestos), "\n")
  invisible(x)
}

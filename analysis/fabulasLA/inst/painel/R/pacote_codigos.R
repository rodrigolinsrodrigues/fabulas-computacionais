# GERADO AUTOMATICAMENTE por analysis/sincronizar_painel.R a partir de
# analysis/fabulasLA/R/codigos.R. NÃO EDITE AQUI: edite o original e rode o script de novo.

# Códigos de participante — mesmo algoritmo do jogo (ParticipantCodes.cs).
# Formato: 4 caracteres + 1 verificador; válido quando sum(posição * valor) %% 31 == 0.

.alfabeto <- strsplit("23456789ABCDEFGHJKMNPQRSTUVWXYZ", "")[[1]]

.normalizar_codigo <- function(x) toupper(gsub("[ ._-]", "", x %||% ""))

#' Verifica se códigos de participante são válidos (dígito verificador).
#' @param codigo vetor de códigos (maiúsculas/minúsculas, espaços e hífens são ignorados)
#' @return vetor lógico
codigo_valido <- function(codigo) {
  vapply(as.character(codigo), function(cod) {
    if (is.na(cod)) return(FALSE)
    ch <- strsplit(.normalizar_codigo(cod), "")[[1]]
    if (length(ch) != 5) return(FALSE)
    v <- match(ch, .alfabeto) - 1L
    if (anyNA(v)) return(FALSE)
    sum(seq_along(v) * v) %% 31L == 0L
  }, logical(1), USE.NAMES = FALSE)
}

#' Gera códigos de participante válidos e distintos.
#' @param n quantidade
#' @param seed semente opcional
gerar_codigos <- function(n, seed = NULL) {
  if (!is.null(seed)) set.seed(seed)
  inv5 <- 25L  # inverso de 5 módulo 31
  out <- character(0)
  while (length(out) < n) {
    v <- sample.int(31L, 4L, replace = TRUE) - 1L
    s <- sum(seq_along(v) * v)
    c <- (((31L - s %% 31L) %% 31L) * inv5) %% 31L
    out <- unique(c(out, paste0(c(.alfabeto[v + 1L], .alfabeto[c + 1L]), collapse = "")))
  }
  out[seq_len(n)]
}

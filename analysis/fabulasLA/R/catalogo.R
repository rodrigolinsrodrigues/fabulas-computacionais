#' Catálogo de eventos do jogo (mesmo arquivo usado pelo Unity).
#' @return lista com schema_version, common_fields, events e affect_columns
catalogo_eventos <- function() {
  arq <- system.file("extdata", "catalogo_eventos.json", package = "fabulasLA")
  if (arq == "") stop("catalogo_eventos.json não encontrado no pacote.")
  jsonlite::fromJSON(arq, simplifyVector = FALSE)
}

#' Livro de códigos (codebook): todas as variáveis dos dados, com tipo e descrição.
#' @return tibble com tabela, evento, categoria, quando, campo, tipo e descricao
codebook <- function() {
  cat <- catalogo_eventos()
  campo <- function(x) tibble(campo = x$name, tipo = x$type, descricao = x$description)
  comuns <- map_dfr(cat$common_fields, campo) |>
    mutate(tabela = "eventos (campos comuns)", evento = "(todos)", categoria = "", quando = "")
  eventos <- map_dfr(cat$events, function(e) {
    f <- if (length(e$fields) == 0) tibble(campo = "(sem campos)", tipo = "", descricao = "") else map_dfr(e$fields, campo)
    f |> mutate(tabela = "eventos (data)", evento = e$name, categoria = e$category, quando = e$when)
  })
  afeto <- map_dfr(cat$affect_columns, campo) |>
    mutate(tabela = "afeto (affect_*.csv)", evento = "", categoria = "afeto", quando = "a cada amostra da webcam")
  bind_rows(comuns, eventos, afeto) |>
    select("tabela", "evento", "categoria", "quando", "campo", "tipo", "descricao")
}

#' Salva o codebook em CSV (abre no Excel com acentos corretos).
#' @param arquivo caminho do .csv
exportar_codebook <- function(arquivo = "codebook_fabulas.csv") {
  write_excel_csv(codebook(), arquivo)
  invisible(normalizePath(arquivo))
}

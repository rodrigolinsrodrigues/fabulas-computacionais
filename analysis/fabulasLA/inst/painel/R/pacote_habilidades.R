# GERADO AUTOMATICAMENTE por analysis/sincronizar_painel.R a partir de
# analysis/fabulasLA/R/habilidades.R. NÃO EDITE AQUI: edite o original e rode o script de novo.

#' Mapa de habilidades: que conceito de pensamento computacional cada fase exercita.
#'
#' Lê `inst/extdata/mapa_habilidades.csv` (ou outro arquivo no mesmo formato). Colunas:
#' `habilidade` (identificador), `rotulo`, `descricao`, `cenas` (expressão regular sobre o nome
#' da cena) e `evidencia`, que define o que conta como acerto em cada tentativa:
#' \describe{
#'   \item{primeira_tentativa}{crédito parcial na primeira tentativa de cada fase: proporção de
#'     posições certas antes de qualquer correção (o que a criança já sabia ao chegar)}
#'   \item{sucesso}{a tentativa resolveu a fase (0/1)}
#'   \item{repeticao}{no Musical, a repetição foi usada corretamente (campo `part1_repeat`)}
#'   \item{melhora}{depois de um erro, a nova tentativa ficou mais perto da solução (ou acertou)}
#' }
#' Edite uma cópia do CSV para mudar o mapeamento (ex.: incluir novas fases) e passe o caminho.
#' @param arquivo CSV com o mapa (padrão: o do pacote)
#' @return tibble com o mapa
#' @export
mapa_habilidades <- function(arquivo = file.path(pasta_extdata(), "mapa_habilidades.csv")) {
  readr::read_csv(arquivo, show_col_types = FALSE, col_types = readr::cols(.default = "c"))
}

#' Domínio de cada conceito por estudante (Bayesian Knowledge Tracing).
#'
#' Cada fase jogada (e, para repetição e depuração, cada tentativa) é uma **oportunidade** de
#' praticar as habilidades da fase (ver [mapa_habilidades()]). A evidência pode ser binária ou
#' crédito parcial (0–1, proporção de posições certas), tratado como evidência ponderada no
#' filtro do BKT (Wang & Heffernan, 2013). Para cada habilidade, um modelo BKT
#' (Corbett & Anderson, 1995) com quatro parâmetros é ajustado por máxima verossimilhança com
#' todos os estudantes:
#' \describe{
#'   \item{L0}{probabilidade de já dominar antes de jogar}
#'   \item{T}{probabilidade de aprender a cada oportunidade}
#'   \item{G}{chute: acertar sem dominar}
#'   \item{S}{deslize: errar mesmo dominando}
#' }
#' Com os parâmetros, o modelo atualiza, tentativa a tentativa, a probabilidade de cada
#' estudante dominar a habilidade. Por convenção, considera-se **dominada** quando ela passa de
#' 0,95 (Corbett & Anderson, 1995).
#'
#' @param coleta objeto de [ler_coleta()] ou [ler_coleta_servidor()]
#' @param mapa mapa de habilidades ([mapa_habilidades()])
#' @param limiar probabilidade a partir da qual a habilidade é considerada dominada
#' @param parametros tibble com `habilidade, L0, T, G, S` para usar parâmetros fixos (ex.: os
#'   estimados num estudo maior) em vez de ajustar nesta coleta
#' @param validacao_cruzada calcula a AUC de previsão com validação cruzada por estudante (5 grupos)
#' @param min_oportunidades com menos oportunidades que isto, a situação fica "poucos dados"
#'   (a probabilidade é calculada, mas ainda depende muito do conhecimento inicial do grupo)
#' @return objeto "fabulas_dominio": lista com `parametros` (por habilidade, com o ajuste),
#'   `trajetoria` (uma linha por oportunidade: probabilidade de acertar prevista antes e de
#'   dominar depois), `final` (estudante × habilidade: oportunidades, acertos, probabilidade de
#'   domínio e situação) e `mapa`
#' @references Corbett, A. T., & Anderson, J. R. (1995). Knowledge tracing: Modeling the
#'   acquisition of procedural knowledge. User Modeling and User-Adapted Interaction, 4, 253–278.
#'   Wang, Y., & Heffernan, N. (2013). Extending knowledge tracing to allow partial credit.
#'   In Artificial Intelligence in Education (pp. 181–188). Springer.
#' @examples
#' \dontrun{
#' dom <- dominio_habilidades(ler_coleta("PILOTO26"))
#' dom$parametros   # o que o jogo ensina e quão difícil é cada conceito
#' dom$final        # domínio de cada estudante em cada conceito
#' }
#' @export
dominio_habilidades <- function(coleta, mapa = mapa_habilidades(), limiar = 0.95, parametros = NULL,
                                validacao_cruzada = TRUE, min_oportunidades = 3) {
  obs <- observacoes_habilidades(coleta, mapa)
  habs <- mapa$habilidade
  pars <- list(); traj <- list()
  for (h in habs) {
    o <- obs[obs$habilidade == h, ]
    if (nrow(o) == 0) next
    p <- if (!is.null(parametros) && h %in% parametros$habilidade) {
      unlist(parametros[parametros$habilidade == h, c("L0", "T", "G", "S")])
    } else .bkt_ajustar(o)
    tr <- .bkt_filtrar(o, p)
    auc_in <- .auc(tr$correto, tr$p_acerto)
    auc_cv <- if (validacao_cruzada && is.null(parametros)) .bkt_auc_cv(o) else NA_real_
    pars[[h]] <- tibble(habilidade = h, L0 = p[[1]], T = p[[2]], G = p[[3]], S = p[[4]],
                        estudantes = n_distinct(o$participant), oportunidades = nrow(o),
                        media_acerto = mean(o$correto), auc = auc_in, auc_cv = auc_cv)
    traj[[h]] <- tr
  }
  parametros <- if (length(pars) == 0) {
    # ainda não há tentativas suficientes (ex.: coleta recém-iniciada)
    tibble(habilidade = character(), rotulo = character(), L0 = numeric(), T = numeric(), G = numeric(), S = numeric(),
           estudantes = integer(), oportunidades = integer(), media_acerto = numeric(), auc = numeric(), auc_cv = numeric())
  } else dplyr::bind_rows(pars) |> left_join(mapa |> select("habilidade", "rotulo"), by = "habilidade") |>
    select("habilidade", "rotulo", everything())
  trajetoria <- if (length(traj) == 0) {
    tibble(participant = character(), habilidade = character(), ordem = integer(), session = character(), t_ms = numeric(),
           scene = character(), correto = numeric(), p_acerto = numeric(), p_dominio = numeric())
  } else dplyr::bind_rows(traj)
  final <- if (nrow(trajetoria) == 0) {
    tibble(participant = character(), habilidade = character(), oportunidades = integer(), acertos = numeric(),
           media_acerto = numeric(), p_dominio = numeric(), situacao = character(), rotulo = character())
  } else trajetoria |>
    group_by(.data$participant, .data$habilidade) |>
    summarise(oportunidades = n(), acertos = sum(.data$correto), media_acerto = mean(.data$correto),
              p_dominio = dplyr::last(.data$p_dominio), .groups = "drop") |>
    mutate(situacao = dplyr::case_when(.data$oportunidades < min_oportunidades ~ "poucos dados",
                                       .data$p_dominio >= limiar ~ "domina",
                                       .data$p_dominio >= 0.6 ~ "em progresso",
                                       TRUE ~ "ainda não domina")) |>
    left_join(mapa |> select("habilidade", "rotulo"), by = "habilidade")
  structure(list(parametros = parametros, trajetoria = trajetoria, final = final, mapa = mapa, limiar = limiar),
            class = "fabulas_dominio")
}

#' Oportunidades de prática de cada habilidade (uma linha por tentativa e habilidade).
#' @inheritParams dominio_habilidades
#' @return tibble `participant, habilidade, ordem, session, t_ms, scene, correto`
#' @export
observacoes_habilidades <- function(coleta, mapa = mapa_habilidades()) {
  tent <- tabelas_analise(coleta)$tentativas
  if (nrow(tent) == 0) return(tibble(participant = character(), habilidade = character(), ordem = integer(),
                                     session = character(), t_ms = numeric(), scene = character(), correto = integer()))
  inicio <- coleta$eventos |> filter(.data$event == "session_start") |> select("session", inicio = "t")
  tent <- tent |> left_join(inicio, by = "session") |> arrange(.data$participant, .data$inicio, .data$session, .data$t_ms)
  tent$sucesso <- tent$success %in% TRUE
  tent <- tent |> group_by(.data$session, .data$scene) |> arrange(.data$t_ms, .by_group = TRUE) |>
    mutate(anterior_errou = dplyr::lag(!.data$sucesso, default = FALSE),
           d = suppressWarnings(as.numeric(.data$distance)), melhorou = !is.na(dplyr::lag(.data$d)) & !is.na(.data$d) & .data$d < dplyr::lag(.data$d)) |>
    ungroup()

  saida <- list()
  for (i in seq_len(nrow(mapa))) {
    m <- mapa[i, ]
    t <- tent[grepl(m$cenas, tent$scene), ]
    t <- switch(m$evidencia,
      primeira_tentativa = t |> group_by(.data$session, .data$scene) |> slice_min(.data$t_ms, n = 1, with_ties = FALSE) |> ungroup() |>
        mutate(correto = ifelse(.data$sucesso, 1, 1 - suppressWarnings(as.numeric(.data$dist_rel)))) |> filter(!is.na(.data$correto)),
      sucesso = t |> mutate(correto = as.integer(.data$sucesso)),
      repeticao = t |> filter(!is.na(.data$part1_repeat)) |> mutate(correto = as.integer(.data$part1_repeat %in% TRUE)),
      melhora = t |> filter(.data$anterior_errou) |> mutate(correto = as.integer(.data$sucesso | .data$melhorou)),
      stop("evidencia desconhecida no mapa de habilidades: ", m$evidencia))
    if (nrow(t) == 0) next
    saida[[i]] <- t |> transmute(participant = .data$participant, habilidade = m$habilidade, session = .data$session,
                                 t_ms = .data$t_ms, scene = .data$scene, inicio = .data$inicio, correto = .data$correto)
  }
  dplyr::bind_rows(saida) |> arrange(.data$participant, .data$habilidade, .data$inicio, .data$session, .data$t_ms) |>
    group_by(.data$participant, .data$habilidade) |> mutate(ordem = dplyr::row_number()) |> ungroup() |>
    select("participant", "habilidade", "ordem", "session", "t_ms", "scene", "correto")
}

#' @export
print.fabulas_dominio <- function(x, ...) {
  cat("<Domínio de habilidades (BKT)>\n")
  cat("  Estudantes:", n_distinct(x$final$participant), " | Habilidades:", nrow(x$parametros), " | limiar de domínio:", x$limiar, "\n\n")
  p <- x$parametros
  if (nrow(p)) print(as.data.frame(p |> transmute(rotulo, L0 = round(L0, 2), T = round(T, 2), G = round(G, 2), S = round(S, 2),
                                                  estudantes, oportunidades, auc_cv = round(auc_cv, 2))), row.names = FALSE)
  if (nrow(x$final)) {
    cat("\nSituação por habilidade:\n")
    print(as.data.frame(x$final |> count(rotulo, situacao) |> tidyr::pivot_wider(names_from = "situacao", values_from = "n", values_fill = 0)), row.names = FALSE)
  }
  invisible(x)
}

# ---------------------------------------------------------------- BKT

# Filtro para frente, vetorizado por estudante. o: participant, ordem, correto (ordenado).
.bkt_filtrar <- function(o, p) {
  L0 <- p[[1]]; Tr <- p[[2]]; G <- p[[3]]; S <- p[[4]]
  o <- o[order(o$participant, o$ordem), ]
  estud <- unique(o$participant)
  idx <- match(o$participant, estud)
  pL <- rep(L0, length(estud))
  p_acerto <- numeric(nrow(o)); p_dom <- numeric(nrow(o))
  for (k in sort(unique(o$ordem))) {
    linhas <- which(o$ordem == k)
    e <- idx[linhas]; c <- o$correto[linhas]
    pc <- pL[e] * (1 - S) + (1 - pL[e]) * G
    post <- c * (pL[e] * (1 - S) / pc) + (1 - c) * (pL[e] * S / (1 - pc))   # c em [0, 1]: evidência parcial
    pL[e] <- post + (1 - post) * Tr
    p_acerto[linhas] <- pc; p_dom[linhas] <- pL[e]
  }
  o$p_acerto <- p_acerto
  o$p_dominio <- p_dom
  o
}

.bkt_loglik <- function(par, o) {
  tr <- .bkt_filtrar(o, par)
  pc <- pmin(pmax(tr$p_acerto, 1e-9), 1 - 1e-9)
  -sum(log(tr$correto * pc + (1 - tr$correto) * (1 - pc)))
}

# Máxima verossimilhança com limites usuais: 0,05 <= G, S <= 0,3 evitam soluções degeneradas
# (chute ou deslize perto de zero tornam o modelo implausível e instável com crédito parcial).
.bkt_ajustar <- function(o, inferior = c(0.01, 0.01, 0.05, 0.05), superior = c(0.99, 0.5, 0.3, 0.3)) {
  inicios <- list(c(0.2, 0.1, 0.15, 0.1), c(0.5, 0.2, 0.1, 0.05), c(0.05, 0.05, 0.25, 0.2))
  melhor <- NULL
  for (ini in inicios) {
    r <- tryCatch(stats::optim(ini, .bkt_loglik, o = o, method = "L-BFGS-B", lower = inferior, upper = superior),
                  error = function(e) NULL)
    if (!is.null(r) && (is.null(melhor) || r$value < melhor$value)) melhor <- r
  }
  stats::setNames(melhor$par, c("L0", "T", "G", "S"))
}

# AUC da previsão do próximo acerto, com validação cruzada por estudante.
.bkt_auc_cv <- function(o, k = 5, semente = 1) {
  estud <- unique(o$participant)
  if (length(estud) < k * 2) return(NA_real_)
  set.seed(semente)
  grupo <- stats::setNames(sample(rep(seq_len(k), length.out = length(estud))), estud)
  prev <- rep(NA_real_, nrow(o)); o <- o[order(o$participant, o$ordem), ]
  for (g in seq_len(k)) {
    teste <- grupo[o$participant] == g
    p <- .bkt_ajustar(o[!teste, ])
    prev[teste] <- .bkt_filtrar(o[teste, ], p)$p_acerto
  }
  .auc(o$correto, prev)
}

.auc <- function(y, p) {
  ok <- !is.na(y) & !is.na(p); y <- as.integer(y[ok] >= 0.5); p <- p[ok]   # crédito parcial: acerto = metade ou mais
  n1 <- sum(y == 1); n0 <- sum(y == 0)
  if (n1 == 0 || n0 == 0) return(NA_real_)
  (sum(rank(p)[y == 1]) - n1 * (n1 + 1) / 2) / (n1 * n0)
}

# Testes do pacote fabulasLA.
# Uma coleta simulada pequena é gerada uma vez e reutilizada.

pasta_sim <- local({
  p <- file.path(tempdir(), "teste_fabulasLA_sim")
  simular_coleta(n_alunos = 12, pasta = p, seed = 11, prop_interrompidas = 0.25)
  p
})
coleta <- ler_coleta(pasta_sim)

test_that("códigos de participante: mesmo algoritmo do jogo", {
  cods <- gerar_codigos(500, seed = 1)
  expect_length(unique(cods), 500)
  expect_true(all(codigo_valido(cods)))
  expect_true(all(codigo_valido(tolower(cods[1:20]))))
  expect_true(codigo_valido(paste0(substr(cods[1], 1, 2), "-", substr(cods[1], 3, 5))))
  # toda troca de um caractere é detectada
  alfabeto <- strsplit("23456789ABCDEFGHJKMNPQRSTUVWXYZ", "")[[1]]
  for (cod in cods[1:20]) for (pos in 1:5) for (ch in setdiff(alfabeto, substr(cod, pos, pos))) {
    errado <- cod; substr(errado, pos, pos) <- ch
    expect_false(codigo_valido(errado))
  }
  expect_false(any(codigo_valido(c("", NA, "MARIA", "ABCDEF"))))
  # código gerado pelo jogo (C#) com a mesma regra: soma ponderada mod 31
  v <- match(strsplit(cods[1], "")[[1]], alfabeto) - 1
  expect_equal(sum(seq_along(v) * v) %% 31, 0)
})

test_that("leitura da coleta simulada", {
  expect_s3_class(coleta, "fabulas_coleta")
  expect_equal(dplyr::n_distinct(coleta$eventos$session), 12)
  expect_true(all(c("id", "seq", "t_ms", "session", "participant", "event") %in% names(coleta$eventos)))
  expect_gt(nrow(coleta$afeto), 1000)
  expect_true(!is.null(coleta$extras$observacoes_bromp))
  expect_output(print(coleta), "Sessões")
})

test_that("validação: coleta íntegra não tem erros, mas aponta interrompidas", {
  v <- validar_coleta(coleta)
  expect_s3_class(v, "fabulas_validacao")
  expect_equal(sum(v$status == "erro"), 0)
  expect_equal(v$status[v$verificacao == "Sequência de eventos"], "ok")
  expect_equal(v$status[v$verificacao == "Eventos fora do catálogo"], "ok")
  expect_equal(v$status[v$verificacao == "Sessões interrompidas"], "aviso")
  expect_output(print(v), "Validação da coleta")
})

test_that("validação detecta duplicatas, lacunas, sessão sem início e evento desconhecido", {
  ruim <- coleta
  ev <- ruim$eventos
  s1 <- unique(ev$session)[1]
  ev <- dplyr::bind_rows(ev, ev[1, ])                                   # duplicata
  ev <- ev[!(ev$session == s1 & ev$seq == 5), ]                          # lacuna
  s2 <- unique(ev$session)[2]
  ev <- ev[!(ev$session == s2 & ev$event == "session_start"), ]          # sem início
  ev$event[ev$session == s1 & ev$seq == 6] <- "evento_misterioso"        # fora do catálogo
  ruim$eventos <- ev
  v <- validar_coleta(ruim)
  expect_equal(v$status[v$verificacao == "Eventos duplicados"], "erro")
  expect_equal(v$status[v$verificacao == "Sequência de eventos"], "erro")
  expect_equal(v$status[v$verificacao == "Início das sessões"], "erro")
  expect_equal(v$status[v$verificacao == "Eventos fora do catálogo"], "aviso")
  expect_match(v$detalhe[v$verificacao == "Eventos fora do catálogo"], "evento_misterioso")
})

test_that("tabelas de análise", {
  tb <- tabelas_analise(coleta)
  expect_named(tb, c("sessoes", "fases", "tentativas", "afeto_sessao"))
  expect_equal(nrow(tb$sessoes), 12)
  expect_true(all(c("escola", "turma", "condicao", "interrompida") %in% names(tb$sessoes)))
  expect_true(all(tb$sessoes$condicao %in% c("A", "B")))
  expect_true(any(tb$sessoes$interrompida))
  expect_true(all(c("tentativas", "concluiu", "abandonou", "tempo_evento_s") %in% names(tb$fases)))
  expect_true(all(c("distance", "delta", "n_changed", "dist_rel") %in% names(tb$tentativas)))
  expect_true(all(tb$tentativas$dist_rel[!is.na(tb$tentativas$dist_rel)] >= 0))
  # tentativas e eventos batem
  n_tent <- sum(coleta$eventos$event == "puzzle_attempt")
  expect_equal(sum(tb$fases$tentativas), n_tent)
})

test_that("codebook cobre todos os eventos do catálogo", {
  cb <- codebook()
  cat <- catalogo_eventos()
  nomes <- vapply(cat$events, function(e) e$name, "")
  expect_true(all(nomes %in% cb$evento))
  expect_true(all(c("id", "seq") %in% cb$campo))
  arq <- exportar_codebook(file.path(tempdir(), "cb.csv"))
  expect_true(file.exists(arq))
  expect_gt(length(readLines(arq, encoding = "UTF-8")), 100)
})

test_that("lê .zip com manifesto e confere integridade", {
  skip_if_not(nzchar(Sys.getenv("FABULAS_ZIP_TESTE")), "defina FABULAS_ZIP_TESTE com um .zip exportado pelo jogo")
  z <- Sys.getenv("FABULAS_ZIP_TESTE")
  c2 <- ler_coleta(z)
  expect_gt(nrow(c2$manifestos), 0)
  expect_true(all(c2$arquivos$sha_ok[!is.na(c2$arquivos$sha_ok)]))
  expect_equal(sum(validar_coleta(c2)$status == "erro"), 0)
})

test_that("ler_coleta_servidor reconstrói a coleta igual à dos arquivos locais", {
  # Servidor falso com as mesmas regras do schema.sql: páginas de até 1000 por (sessão, seq/idx)
  ev_arqs <- list.files(pasta_sim, "^events_.*[.]jsonl$", full.names = TRUE)
  eventos <- unlist(lapply(ev_arqs, function(f) lapply(readLines(f, encoding = "UTF-8"), jsonlite::fromJSON, simplifyVector = FALSE)), recursive = FALSE)
  af_arqs <- list.files(pasta_sim, "^affect_.*[.]csv$", full.names = TRUE)
  amostras <- unlist(lapply(af_arqs, function(f) {
    t <- readr::read_csv(f, col_types = readr::cols(.default = "c"), na = "")
    lapply(seq_len(nrow(t)), function(i) {
      a <- as.list(t[i, ])
      a <- lapply(a, function(v) if (is.na(v)) NULL else v)
      num <- setdiff(names(a), c("utc", "session", "participant", "scene"))
      a[num] <- lapply(a[num], function(v) if (is.null(v)) NULL else as.numeric(v))
      c(list(idx = i), a)
    })
  }), recursive = FALSE)
  chamadas <- character()
  falso <- function(url, chave_publica, rpc, corpo) {
    chamadas <<- c(chamadas, rpc)
    if (corpo$p_key != "lei_ok") stop("Servidor respondeu 403: chave de leitura invalida")
    pagina <- function(itens, campo, depois) {
      s <- vapply(itens, function(x) x$session, ""); p <- vapply(itens, function(x) as.numeric(x[[campo]]), 0)
      o <- order(s, p); s <- s[o]; p <- p[o]; itens <- itens[o]
      ok <- s > corpo$p_after_session | (s == corpo$p_after_session & p > depois)
      head(itens[ok], 1000)
    }
    switch(rpc,
      fc_export_events = pagina(eventos, "seq", corpo$p_after_seq),
      fc_export_affect = pagina(amostras, "idx", corpo$p_after_idx),
      list())
  }
  srv <- suppressMessages(ler_coleta_servidor("https://falso.supabase.co", "sb_publishable_x", "SIM-2026", "lei_ok",
                                              pasta = file.path(tempdir(), "teste_servidor"), .post = falso))
  expect_gt(sum(chamadas == "fc_export_events"), 1)   # houve paginação
  expect_equal(nrow(srv$eventos), nrow(coleta$eventos))
  expect_equal(nrow(srv$afeto), nrow(coleta$afeto))
  k <- c("session", "seq", "event", "t_ms", "participant", "scene")
  expect_equal(as.data.frame(arrange(srv$eventos[, k], session, seq)), as.data.frame(arrange(coleta$eventos[, k], session, seq)))
  num <- intersect(names(coleta$afeto)[sapply(coleta$afeto, is.numeric)], names(srv$afeto))
  expect_equal(as.data.frame(arrange(srv$afeto, session, t_ms)[, num]), as.data.frame(arrange(coleta$afeto, session, t_ms)[, num]),
               tolerance = 1e-9)
  expect_equal(nrow(tabelas_analise(srv)$tentativas), nrow(tabelas_analise(coleta)$tentativas))

  expect_error(ler_coleta_servidor("https://x", "sb_publishable_x", "SIM-2026", "errada", .post = falso), "chave")
  expect_error(ler_coleta_servidor("https://x", "sb_secret_abc", "SIM-2026", "lei_ok", .post = falso), "PÚBLICA")
  expect_error(ler_coleta_servidor("https://x", "sb_publishable_x", "SIM-2026", ""), "chave de leitura")
})

test_that("CSV facial só com cabeçalho (câmera que falhou) não quebra a leitura", {
  p <- file.path(tempdir(), "teste_afeto_vazio")
  dir.create(p, showWarnings = FALSE)
  file.copy(list.files(pasta_sim, full.names = TRUE), p, overwrite = TRUE)
  writeLines("utc,t_ms,session,participant,scene,face_present,head_yaw,smile,bs_jawOpen", file.path(p, "affect_VAZIA.csv"))
  c2 <- ler_coleta(p)
  expect_equal(nrow(c2$afeto), nrow(coleta$afeto))
  expect_true(is.numeric(c2$afeto$t_ms))
})

test_that("afeto_por_evento junta eventos e rosto pelo relógio da sessão", {
  a <- afeto_por_evento(coleta, eventos = "solution_snapshot", antes_ms = 1000, depois_ms = 3000)
  expect_equal(nrow(a), sum(coleta$eventos$event == "solution_snapshot"))
  expect_true(all(c("amostras", "rosto", "smile", "z_smile", "z_brow_down") %in% names(a)))
  com <- a[a$amostras > 0, ]
  expect_gt(nrow(com), 0)
  # conferência manual de uma janela
  e <- com[1, ]
  manual <- coleta$afeto |> dplyr::filter(session == e$session, t_ms >= e$t_ms - 1000, t_ms <= e$t_ms + 3000)
  expect_equal(e$amostras, nrow(manual))
  expect_equal(e$smile, mean(manual$smile[manual$face_present == 1], na.rm = TRUE))
  # escore z: em todas as amostras do participante, média ~0
  p <- e$participant
  todos <- afeto_por_evento(coleta, participante = tolower(p))
  expect_true(all(todos$participant == p))
  # sem dados faciais: devolve as colunas com NA, sem erro
  vazio <- afeto_por_evento(list(eventos = coleta$eventos, afeto = tibble::tibble()), "puzzle_start")
  expect_true(all(is.na(vazio$rosto)))
})

test_that("estados_faciais estima os cinco estados por janela", {
  e <- estados_faciais(coleta, janela_s = 10)
  est <- c("confusao", "frustracao", "tedio", "engajamento", "encantamento")
  expect_true(all(c("session", "participant", "scene", "t_ini", "t_fim", est, "estado", "confianca", "versao") %in% names(e)))
  expect_true(all(e$estado %in% c(est, "neutro")))
  for (s in est) expect_true(all(e[[s]] >= 0 & e[[s]] <= 1))
  expect_true(all(e$t_fim - e$t_ini == 10000))
  expect_equal(unique(e$versao), regras_estados()$versao)
  # janelas com acerto têm mais encantamento que janelas sem acerto
  expect_gt(mean(e$encantamento[e$acertos > 0]), mean(e$encantamento[e$acertos == 0]))
  # janelas inteiramente ociosas têm mais tédio e menos engajamento
  expect_gt(mean(e$tedio[e$ocioso > .9]), mean(e$tedio[e$ocioso == 0]))
  expect_lt(mean(e$engajamento[e$ocioso > .9]), mean(e$engajamento[e$ocioso == 0]))
  # filtro por participante e funcionamento sem dados faciais (só comportamento, confiança baixa)
  p <- e$participant[1]
  expect_true(all(estados_faciais(coleta, participante = tolower(p))$participant == p))
  sem_rosto <- estados_faciais(list(eventos = coleta$eventos, afeto = tibble::tibble()))
  expect_true(all(sem_rosto$confianca == "baixa"))
  expect_equal(nrow(estados_faciais(list(eventos = coleta$eventos[0, ], afeto = tibble::tibble()))), 0)
})

test_that("BKT: filtro bate com a conta à mão e o ajuste recupera parâmetros conhecidos", {
  # Conta à mão para uma sequência curta (L0 = .3, T = .2, G = .1, S = .1)
  p <- c(L0 = .3, T = .2, G = .1, S = .1)
  o <- tibble::tibble(participant = "A", ordem = 1:2, correto = c(1, 0))
  tr <- fabulasLA:::.bkt_filtrar(o, p)
  pc1 <- .3 * .9 + .7 * .1
  post1 <- .3 * .9 / pc1; L1 <- post1 + (1 - post1) * .2
  pc2 <- L1 * .9 + (1 - L1) * .1
  post2 <- L1 * .1 / (1 - pc2); L2 <- post2 + (1 - post2) * .2
  expect_equal(tr$p_acerto, c(pc1, pc2))
  expect_equal(tr$p_dominio, c(L1, L2))
  # Crédito parcial 0,5 fica entre acerto e erro
  meio <- fabulasLA:::.bkt_filtrar(tibble::tibble(participant = "A", ordem = 1, correto = .5), p)$p_dominio
  acerto <- fabulasLA:::.bkt_filtrar(tibble::tibble(participant = "A", ordem = 1, correto = 1), p)$p_dominio
  erro <- fabulasLA:::.bkt_filtrar(tibble::tibble(participant = "A", ordem = 1, correto = 0), p)$p_dominio
  expect_true(erro < meio && meio < acerto)

  # Recuperação: 400 estudantes x 12 oportunidades gerados com parâmetros conhecidos
  set.seed(7)
  verd <- c(L0 = .25, T = .15, G = .2, S = .1)
  gera <- function() {
    dom <- runif(1) < verd[["L0"]]; out <- integer(12)
    for (k in 1:12) {
      out[k] <- as.integer(runif(1) < ifelse(dom, 1 - verd[["S"]], verd[["G"]]))
      if (!dom) dom <- runif(1) < verd[["T"]]
    }
    out
  }
  sim <- purrr::map_dfr(1:400, function(i) tibble::tibble(participant = sprintf("P%03d", i), ordem = 1:12, correto = gera()))
  est <- fabulasLA:::.bkt_ajustar(sim)
  expect_equal(unname(est), unname(verd), tolerance = 0.06)
  expect_gt(fabulasLA:::.bkt_auc_cv(sim), 0.7)
})

test_that("dominio_habilidades estima o domínio de cada conceito por estudante", {
  mapa <- mapa_habilidades()
  expect_true(all(c("habilidade", "rotulo", "descricao", "cenas", "evidencia") %in% names(mapa)))
  expect_true(all(mapa$evidencia %in% c("primeira_tentativa", "sucesso", "repeticao", "melhora")))
  ob <- observacoes_habilidades(coleta, mapa)
  expect_true(all(ob$correto >= 0 & ob$correto <= 1))
  expect_true(all(ob$habilidade %in% mapa$habilidade))
  d <- dominio_habilidades(coleta, validacao_cruzada = FALSE)
  expect_s3_class(d, "fabulas_dominio")
  expect_true(all(d$final$p_dominio >= 0 & d$final$p_dominio <= 1))
  expect_true(all(d$final$situacao %in% c("domina", "em progresso", "ainda não domina", "poucos dados")))
  expect_true(all(d$final$situacao[d$final$oportunidades < 3] == "poucos dados"))
  with(d$parametros, {
    expect_true(all(L0 >= .01 & L0 <= .99)); expect_true(all(G >= .05 & G <= .3)); expect_true(all(S >= .05 & S <= .3))
  })
  # a trajetória termina no domínio final
  ult <- d$trajetoria |> dplyr::group_by(participant, habilidade) |> dplyr::slice_max(ordem, n = 1) |> dplyr::ungroup()
  comb <- dplyr::inner_join(ult, d$final, by = c("participant", "habilidade"))
  expect_equal(comb$p_dominio.x, comb$p_dominio.y)
  # parâmetros fixos são respeitados
  fixos <- d$parametros |> dplyr::select(habilidade, L0, T, G, S) |> dplyr::mutate(T = 0.5)
  d2 <- dominio_habilidades(coleta, parametros = fixos, validacao_cruzada = FALSE)
  expect_equal(d2$parametros$T, rep(0.5, nrow(fixos)))
  # o domínio estimado acompanha a habilidade "verdadeira" da simulação
  v <- coleta$extras[["_verdade_simulacao"]]
  m <- d$final |> dplyr::group_by(participant) |> dplyr::summarise(dom = mean(p_dominio)) |> dplyr::inner_join(v, by = "participant")
  expect_gt(cor(m$dom, m$theta), 0.4)
  expect_output(print(d), "Domínio de habilidades")
})

test_that("painel Shiny: arquivos carregam e a camada de dados funciona", {
  skip_if_not_installed("shiny"); skip_if_not_installed("bslib"); skip_if_not_installed("DT"); skip_if_not_installed("bsicons")
  pasta_app <- system.file("painel", package = "fabulasLA")
  expect_true(file.exists(file.path(pasta_app, "app.R")))
  amb <- new.env()
  for (f in list.files(file.path(pasta_app, "R"), full.names = TRUE)) sys.source(f, envir = amb)
  expect_true(all(c("monitorUI", "monitorServer", "grupoUI", "grupoServer", "individualUI", "individualServer") %in% ls(amb)))
  fonte <- list(modo = "pasta", caminhos = pasta_sim, n_alunos = 12)
  vs <- amb$visao_sessoes(fonte)
  expect_equal(nrow(vs), dplyr::n_distinct(coleta$eventos$session))
  expect_equal(sum(vs$eventos), nrow(coleta$eventos))
  expect_true(all(c("session", "participant", "encerrada", "ultimo_seq", "eventos", "amostras", "ultimo") %in% names(vs)))
  a <- amb$preparar_analise(coleta)
  expect_true(all(c("tb", "dom", "est", "validacao") %in% names(a)))
  ev <- amb$eventos_da_sessao(fonte, vs$session[1], coleta)
  expect_true(all(ev$session == vs$session[1]))
  # interface monta sem erro
  expect_s3_class(amb$monitorUI("m"), "shiny.tag")
  expect_s3_class(amb$grupoUI("g"), "shiny.tag")
  expect_s3_class(amb$individualUI("i"), "shiny.tag")
})

test_that("painel: cópia das funções do pacote está em dia (rode analysis/sincronizar_painel.R)", {
  pacote_r <- file.path("..", "..", "R"); painel_r <- file.path("..", "..", "inst", "painel", "R")
  skip_if_not(dir.exists(pacote_r) && dir.exists(painel_r), "fontes do pacote não disponíveis")
  origem <- setdiff(list.files(pacote_r, pattern = "[.]R$"), c("relatorio.R", "painel.R"))
  for (f in origem) {
    copia <- file.path(painel_r, paste0("pacote_", f))
    expect_true(file.exists(copia), info = paste("falta a cópia de", f))
    esperado <- readLines(file.path(pacote_r, f), encoding = "UTF-8", warn = FALSE)
    esperado <- gsub('system.file\\("extdata", "([^"]+)", package = "fabulasLA"\\)', 'file.path(pasta_extdata(), "\\1")', esperado)
    esperado <- gsub("fabulasLA:::?", "", esperado)
    atual <- readLines(copia, encoding = "UTF-8", warn = FALSE)
    expect_identical(atual[-(1:3)], esperado, info = paste(f, "desatualizado: rode analysis/sincronizar_painel.R"))
  }
  for (f in list.files(file.path("..", "..", "inst", "extdata"))) {
    expect_identical(unname(tools::md5sum(file.path("..", "..", "inst", "extdata", f))),
                     unname(tools::md5sum(file.path("..", "..", "inst", "painel", "extdata", f))), info = f)
  }
  # nenhuma referência ao pacote que faça o shinyapps.io tentar instalá-lo
  codigo <- unlist(lapply(list.files(file.path("..", "..", "inst", "painel"), pattern = "[.]R$", recursive = TRUE, full.names = TRUE),
                          readLines, encoding = "UTF-8", warn = FALSE))
  codigo <- codigo[!grepl("^\\s*#", codigo)]
  expect_false(any(grepl("fabulasLA::|library\\(fabulasLA|require\\(fabulasLA", codigo)))
})

test_that("coletas sem tentativas ou só com navegação não quebram as análises", {
  # Ex.: logo depois de limpar o banco — crianças entraram, andaram pelo mundo, mas não tentaram nenhum puzzle
  sem_tent <- coleta
  sem_tent$eventos <- coleta$eventos[!coleta$eventos$event %in% c("solution_snapshot", "puzzle_attempt", "puzzle_complete", "solution_final"), ]
  sem_tent$eventos <- sem_tent$eventos[, setdiff(names(sem_tent$eventos), c("puzzle", "slots", "expected", "distance", "total_positions",
                                                                              "n_changed", "success", "part1_repeat", "attempt", "attempts"))]
  sem_tent$afeto <- tibble::tibble()
  tb <- tabelas_analise(sem_tent)
  expect_equal(nrow(tb$tentativas), 0)
  expect_gt(nrow(tb$sessoes), 0)
  expect_s3_class(dominio_habilidades(sem_tent, validacao_cruzada = FALSE), "fabulas_dominio")
  expect_true(is.data.frame(estados_faciais(sem_tent)))
  expect_s3_class(validar_coleta(sem_tent), "fabulas_validacao")
  expect_equal(nrow(afeto_por_evento(sem_tent, "scene_enter")), sum(sem_tent$eventos$event == "scene_enter"))
})

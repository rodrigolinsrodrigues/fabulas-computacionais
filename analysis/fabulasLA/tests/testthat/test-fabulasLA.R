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

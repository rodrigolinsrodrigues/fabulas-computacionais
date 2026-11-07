# Servidor central (Supabase)

Banco de dados que recebe, de todos os computadores das escolas, **todos os eventos** e **todas as amostras
faciais** do jogo. Os arquivos locais continuam sendo gravados em cada computador e servem de backup; sem internet,
o jogo guarda e envia depois.

| Arquivo | Função |
|---|---|
| `schema.sql` | Tabelas, funções e permissões. Rode **uma vez** no SQL Editor do projeto. Pode ser rodado de novo (atualiza sem apagar dados). |
| `teste_schema.sql` | Testes do banco (recebimento, duplicatas, chaves, permissões, leitura paginada). |

## Preparar (uma vez por projeto)

1. Em [supabase.com](https://supabase.com), crie um projeto na região **South America (São Paulo)**. Guarde a senha
   do banco num gerenciador de senhas.
2. **SQL Editor → New query**: cole todo o `schema.sql` e clique em **Run**.
3. **Project Settings → API Keys**: copie a **URL do projeto** e a chave **publishable** (ou `anon`).
   A chave **secret / service_role** dá acesso total ao banco: **nunca** coloque no jogo, no R ou no Git.

## Cadastrar um estudo

1. No Unity, **Fabulas → Learning Analytics → Painel do Estudo**, seção *Servidor central*: marque *Enviar ao
   servidor*, cole a URL e a chave pública e clique em **Gerar chaves do estudo**.
2. Abra `Pesquisa/servidor_<estudo>.txt` (fora do Git) e rode no SQL Editor o comando indicado:
   ```sql
   select public.fc_create_study('PILOTO26', '<chave de envio>', '<chave de leitura>');
   ```
3. **Salvar configuração** e **Testar conexão** no Painel do Estudo. Gere a build: `server_sync.json` vai junto em
   `StreamingAssets` e no repositório.

| Chave | Onde fica | Permite |
|---|---|---|
| Pública (publishable/anon) | jogo e R | só chamar as funções abaixo |
| Envio (`env_…`) | jogo (`server_sync.json`, no repositório) | **gravar** dados novos do estudo; não lê nem apaga |
| Leitura (`lei_…`) | equipe (`Pesquisa/`, `.Renviron`) | baixar os dados do estudo |

No banco só ficam os hashes SHA-256 das chaves. Para trocar as chaves (ex.: computador perdido), gere novas no
Painel do Estudo e rode o novo comando `fc_create_study`.

O repositório já traz o `server_sync.json` do estudo `FABULAS` neste projeto: quem rodar o jogo, mesmo em testes,
grava nele. Para a coleta real, use um estudo próprio (ex.: `PILOTO26`), para separar os dados dos testes.

## Baixar os dados (R)

```r
Sys.setenv(FABULAS_CHAVE_LEITURA = "lei_...")   # melhor: no arquivo .Renviron
coleta <- fabulasLA::ler_coleta_servidor("https://<projeto>.supabase.co", "sb_publishable_...", "PILOTO26")
fabulasLA::validar_coleta(coleta)
```

## Como o banco funciona

- Tabelas `fc_studies`, `fc_sessions` (resumo por sessão), `fc_events` (um evento por linha, com a linha original em
  `raw`) e `fc_affect` (uma amostra por linha: colunas principais + todos os blendshapes em `raw`).
- **RLS ligado sem políticas**: a chave pública não lê nem grava as tabelas diretamente. Tudo passa pelas funções
  `fc_ingest_events`, `fc_ingest_affect`, `fc_upload_status` (chave de envio) e `fc_export_events`,
  `fc_export_affect`, `fc_study_overview` (chave de leitura). `fc_create_study` só roda no SQL Editor.
- Reenvios são ignorados (`id` do evento; sessão + número da amostra). As funções respondem até onde receberam
  cada sessão, e o jogo continua dali.
- Plano gratuito: 500 MB de banco. Com câmera a 2 amostras/s, cada hora de jogo gera ~7.200 amostras (alguns MB).

## Testar localmente (Docker)

```bash
docker run -d --name fc-pg-teste -e POSTGRES_PASSWORD=teste -p 54329:5432 postgres:15-alpine
printf 'create schema if not exists extensions; create extension if not exists pgcrypto schema extensions;\ncreate role anon nologin; create role authenticated nologin; grant usage on schema public to anon, authenticated;\n' > preparo.sql
docker cp preparo.sql fc-pg-teste:/preparo.sql && docker cp schema.sql fc-pg-teste:/schema.sql && docker cp teste_schema.sql fc-pg-teste:/teste.sql
MSYS_NO_PATHCONV=1 docker exec fc-pg-teste psql -U postgres -v ON_ERROR_STOP=1 -q -f /preparo.sql -f /schema.sql -f /teste.sql
```

Para testar o jogo e o R com HTTP de verdade, suba o [PostgREST](https://postgrest.org) apontando para esse banco
(`PGRST_DB_ANON_ROLE=anon`) atrás de um proxy que mapeie `/rest/v1/` para ele (como no Supabase), aponte o
`serverUrl` de um `server_sync.json` de teste para o proxy e rode o autoteste do jogo com `-serverConfig <arquivo>`.

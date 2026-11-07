-- Teste do schema.sql num PostgreSQL comum (simula o Supabase: schema extensions e papéis anon/authenticated).
-- Usado pelos testes automatizados:  psql -v ON_ERROR_STOP=1 -f preparo -f schema.sql -f teste_schema.sql
\set ON_ERROR_STOP on

select public.fc_create_study('TESTE', 'envio-123', 'leitura-456');

-- 1) Envio de eventos com a chave certa, incluindo uma duplicata
set role anon;
select public.fc_ingest_events('TESTE', 'envio-123', '[
  {"id":"a1","seq":1,"t":"2026-11-07T15:26:04.1234567Z","t_ms":0,"session":"S1","participant":"K7M2X","scene":"MainMenu","event":"session_start","data":{"school_id":"ESC","class_id":"T1","machine_id":"PC1","condition":"A"}},
  {"id":"a2","seq":2,"t":"2026-11-07T15:26:05Z","t_ms":1000.5,"session":"S1","participant":"K7M2X","scene":"Area1","event":"scene_enter","data":{"scene_name":"Area1"}},
  {"id":"a2","seq":2,"t":"2026-11-07T15:26:05Z","t_ms":1000.5,"session":"S1","participant":"K7M2X","scene":"Area1","event":"scene_enter","data":{"scene_name":"Area1"}}
]'::jsonb) as ack1;
-- reenvio do mesmo lote + continuação
select public.fc_ingest_events('TESTE', 'envio-123', '[
  {"id":"a2","seq":2,"t":"2026-11-07T15:26:05Z","t_ms":1000.5,"session":"S1","participant":"K7M2X","scene":"Area1","event":"scene_enter","data":{}},
  {"id":"a3","seq":3,"t":"2026-11-07T15:27:00Z","t_ms":56000,"session":"S1","participant":"K7M2X","scene":"","event":"session_end","data":{"reason":"interrupted","recovered":true}}
]'::jsonb) as ack2;

-- 2) Amostras faciais (com valores vazios e blendshapes)
select public.fc_ingest_affect('TESTE', 'envio-123', '[
  {"idx":1,"utc":"2026-11-07T15:26:06.5Z","t_ms":2000,"session":"S1","participant":"K7M2X","scene":"Area1","face_present":1,"head_yaw":-3.5,"smile":0.12,"brow_down":0.3,"bs_mouthSmileLeft":0.1,"bs_jawOpen":0.05},
  {"idx":2,"utc":"2026-11-07T15:26:07Z","t_ms":2500,"session":"S1","participant":"K7M2X","scene":"Area1","face_present":0,"head_yaw":null,"smile":null},
  {"idx":2,"utc":"2026-11-07T15:26:07Z","t_ms":2500,"session":"S1","participant":"K7M2X","scene":"Area1","face_present":0}
]'::jsonb) as ack_afeto;

select public.fc_upload_status('TESTE', 'envio-123', array['S1', 'S-NOVA']) as status;

-- 3) Chave errada é recusada
do $$ begin
  perform public.fc_ingest_events('TESTE', 'errada', '[]'::jsonb);
  raise exception 'FALHA: chave errada foi aceita';
exception when sqlstate '28000' then raise notice 'OK: chave de envio errada recusada';
end $$;

-- 4) A chave pública (anon) não lê as tabelas diretamente nem cadastra estudos
do $$ begin
  perform 1 from public.fc_events;
  raise exception 'FALHA: anon leu fc_events';
exception when insufficient_privilege then raise notice 'OK: anon não lê fc_events';
end $$;
do $$ begin
  perform public.fc_create_study('X', 'a', 'b');
  raise exception 'FALHA: anon cadastrou estudo';
exception when insufficient_privilege then raise notice 'OK: anon não cadastra estudo';
end $$;

-- 5) Leitura com a chave de leitura; a chave de envio NÃO serve para ler
select count(*) as eventos_exportados from public.fc_export_events('TESTE', 'leitura-456');
select count(*) as pagina_apos_seq1 from public.fc_export_events('TESTE', 'leitura-456', 'S1', 1, 1000);
select count(*) as afeto_exportado from public.fc_export_affect('TESTE', 'leitura-456');
do $$ begin
  perform public.fc_export_events('TESTE', 'envio-123');
  raise exception 'FALHA: chave de envio leu dados';
exception when sqlstate '28000' then raise notice 'OK: chave de envio não lê dados';
end $$;
select s->>'session' as sessao, s->>'school_id' as escola, s->>'condition' as condicao, s->>'n_events' as eventos,
       s->>'n_affect' as afeto, s->>'ended' as encerrada, s->>'end_reason' as motivo
from public.fc_study_overview('TESTE', 'leitura-456') s;
reset role;

-- 6) Conferências finais
do $$ declare n int; m int; b jsonb; begin
  select count(*) into n from public.fc_events;  if n <> 3 then raise exception 'FALHA: % eventos (esperado 3)', n; end if;
  select count(*) into m from public.fc_affect;  if m <> 2 then raise exception 'FALHA: % amostras (esperado 2)', m; end if;
  select raw into b from public.fc_affect where idx = 1;
  if (b->>'bs_jawOpen')::float <> 0.05 then raise exception 'FALHA: blendshape perdido'; end if;
  raise notice 'OK: 3 eventos sem duplicatas, 2 amostras faciais, blendshapes preservados';
end $$;

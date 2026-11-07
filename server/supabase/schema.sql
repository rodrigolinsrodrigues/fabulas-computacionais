-- =============================================================================
-- Fábulas Computacionais — banco central de learning analytics (Supabase)
-- -----------------------------------------------------------------------------
-- Como usar: Supabase > SQL Editor > cole este arquivo inteiro > Run.
-- Pode ser executado de novo sem perder dados (cria só o que não existe e
-- atualiza as funções).
--
-- Modelo de segurança:
--  * As tabelas têm RLS ligado e NENHUMA política: a chave pública (anon) não
--    lê nem grava nada diretamente.
--  * O jogo só consegue ENVIAR dados, pelas funções fc_ingest_*, e só com a
--    "chave de envio" do estudo.
--  * A leitura (pacote R) usa as funções fc_export_*, com a "chave de leitura"
--    do estudo, que fica só com os pesquisadores.
--  * As chaves são guardadas apenas como hash SHA-256.
--  * Cadastrar um estudo (fc_create_study) só é possível aqui no SQL Editor.
-- =============================================================================

create extension if not exists pgcrypto with schema extensions;

-- ---------------------------------------------------------------- tabelas

create table if not exists public.fc_studies (
  study_id        text primary key,
  upload_key_hash text not null,
  read_key_hash   text not null,
  active          boolean not null default true,
  created_at      timestamptz not null default now()
);

create table if not exists public.fc_sessions (
  session         text primary key,
  study_id        text not null references public.fc_studies(study_id),
  participant     text,
  school_id       text,
  class_id        text,
  machine_id      text,
  condition       text,
  started_at      timestamptz,
  ended           boolean not null default false,
  end_reason      text,
  last_seq        integer not null default 0,
  n_events        integer not null default 0,
  n_affect        integer not null default 0,
  first_received  timestamptz not null default now(),
  last_received   timestamptz not null default now()
);

create table if not exists public.fc_events (
  id          text primary key,
  study_id    text not null references public.fc_studies(study_id),
  session     text not null,
  seq         integer not null,
  participant text,
  t           timestamptz,
  t_ms        double precision,
  scene       text,
  event       text not null,
  data        jsonb,
  raw         jsonb not null,          -- a linha exatamente como o jogo gravou
  received_at timestamptz not null default now()
);
create index if not exists fc_events_session_seq on public.fc_events (session, seq);
create index if not exists fc_events_study_session on public.fc_events (study_id, session, seq);
create index if not exists fc_events_event on public.fc_events (event);

create table if not exists public.fc_affect (
  session       text not null,
  idx           integer not null,      -- número da amostra (linha do CSV)
  study_id      text not null references public.fc_studies(study_id),
  participant   text,
  utc           timestamptz,
  t_ms          double precision,
  scene         text,
  face_present  smallint,
  head_yaw      real, head_pitch real, head_roll real,
  smile         real, frown real, brow_down real, brow_inner_up real, brow_outer_up real,
  eye_wide      real, eye_squint real, eye_blink real, jaw_open real, mouth_press real,
  nose_sneer    real, gaze_away real,
  raw           jsonb not null,        -- a linha completa (inclui os 52 blendshapes bs_*)
  received_at   timestamptz not null default now(),
  primary key (session, idx)
);
create index if not exists fc_affect_study_session on public.fc_affect (study_id, session, idx);

alter table public.fc_studies  enable row level security;
alter table public.fc_sessions enable row level security;
alter table public.fc_events   enable row level security;
alter table public.fc_affect   enable row level security;

-- Segunda barreira: no Supabase, as chaves públicas recebem acesso às tabelas por padrão.
-- Retiramos esse acesso; os dados só entram e saem pelas funções com chave do estudo.
revoke all on table public.fc_studies, public.fc_sessions, public.fc_events, public.fc_affect from public, anon, authenticated;

-- ---------------------------------------------------------------- funções internas

create or replace function public.fc_check_key(p_study text, p_key text, p_kind text)
returns boolean
language sql stable security definer set search_path = public, extensions as $$
  select exists (
    select 1 from public.fc_studies s
    where s.study_id = p_study and s.active
      and (case when p_kind = 'upload' then s.upload_key_hash else s.read_key_hash end)
          = encode(extensions.digest(coalesce(p_key, ''), 'sha256'), 'hex'));
$$;

create or replace function public.fc_num(v text)
returns double precision
language sql immutable as $$
  select case when v is null or v = '' or v = 'null' then null else v::double precision end;
$$;

create or replace function public.fc_refresh_sessions(p_study text, p_sessions text[])
returns void
language sql security definer set search_path = public, extensions as $$
  insert into public.fc_sessions as s (session, study_id, participant, school_id, class_id, machine_id, condition,
                                       started_at, ended, end_reason, last_seq, n_events, n_affect, last_received)
  select x.session, p_study,
         (select max(e.participant) from public.fc_events e where e.session = x.session),
         (select max(e.data->>'school_id')  from public.fc_events e where e.session = x.session and e.event = 'session_start'),
         (select max(e.data->>'class_id')   from public.fc_events e where e.session = x.session and e.event = 'session_start'),
         (select max(e.data->>'machine_id') from public.fc_events e where e.session = x.session and e.event = 'session_start'),
         (select max(e.data->>'condition')  from public.fc_events e where e.session = x.session and e.event = 'session_start'),
         (select min(e.t) from public.fc_events e where e.session = x.session and e.event = 'session_start'),
         coalesce((select bool_or(e.event = 'session_end') from public.fc_events e where e.session = x.session), false),
         (select max(e.data->>'reason') from public.fc_events e where e.session = x.session and e.event = 'session_end'),
         coalesce((select max(e.seq) from public.fc_events e where e.session = x.session), 0),
         (select count(*) from public.fc_events e where e.session = x.session),
         (select count(*) from public.fc_affect a where a.session = x.session),
         now()
  from unnest(p_sessions) as x(session)
  on conflict (session) do update set
    participant = coalesce(excluded.participant, s.participant),
    school_id = coalesce(excluded.school_id, s.school_id),
    class_id = coalesce(excluded.class_id, s.class_id),
    machine_id = coalesce(excluded.machine_id, s.machine_id),
    condition = coalesce(excluded.condition, s.condition),
    started_at = coalesce(excluded.started_at, s.started_at),
    ended = excluded.ended, end_reason = excluded.end_reason,
    last_seq = excluded.last_seq, n_events = excluded.n_events, n_affect = excluded.n_affect,
    last_received = now();
$$;

-- ---------------------------------------------------------------- envio (jogo)

-- Recebe um lote de eventos (linhas do events_*.jsonl). Ignora duplicatas pelo id.
-- Retorna {"<sessão>": maior seq recebido}.
create or replace function public.fc_ingest_events(p_study text, p_key text, p_events jsonb)
returns jsonb
language plpgsql security definer set search_path = public, extensions as $$
declare
  v_sessions text[];
  v_result jsonb;
begin
  if not public.fc_check_key(p_study, p_key, 'upload') then
    raise exception 'chave de envio invalida para o estudo %', p_study using errcode = '28000';
  end if;
  if jsonb_typeof(p_events) <> 'array' or jsonb_array_length(p_events) > 2000 then
    raise exception 'lote invalido (esperado array com ate 2000 eventos)';
  end if;

  insert into public.fc_events (id, study_id, session, seq, participant, t, t_ms, scene, event, data, raw)
  select e->>'id', p_study, e->>'session', (e->>'seq')::integer, e->>'participant',
         (e->>'t')::timestamptz, public.fc_num(e->>'t_ms'), e->>'scene', e->>'event', e->'data', e
  from jsonb_array_elements(p_events) e
  where e ? 'id' and e ? 'session' and e ? 'seq' and e ? 'event'
  on conflict (id) do nothing;

  select array_agg(distinct e->>'session') into v_sessions from jsonb_array_elements(p_events) e;
  if v_sessions is null then return '{}'::jsonb; end if;
  perform public.fc_refresh_sessions(p_study, v_sessions);

  select coalesce(jsonb_object_agg(s.session, s.last_seq), '{}'::jsonb) into v_result
  from public.fc_sessions s where s.session = any (v_sessions);
  return v_result;
end;
$$;

-- Recebe um lote de amostras faciais (linhas do affect_*.csv já convertidas em objetos,
-- com o campo "idx" = número da linha). Retorna {"<sessão>": maior idx recebido}.
create or replace function public.fc_ingest_affect(p_study text, p_key text, p_samples jsonb)
returns jsonb
language plpgsql security definer set search_path = public, extensions as $$
declare
  v_sessions text[];
  v_result jsonb;
begin
  if not public.fc_check_key(p_study, p_key, 'upload') then
    raise exception 'chave de envio invalida para o estudo %', p_study using errcode = '28000';
  end if;
  if jsonb_typeof(p_samples) <> 'array' or jsonb_array_length(p_samples) > 2000 then
    raise exception 'lote invalido (esperado array com ate 2000 amostras)';
  end if;

  insert into public.fc_affect (session, idx, study_id, participant, utc, t_ms, scene, face_present,
                                head_yaw, head_pitch, head_roll, smile, frown, brow_down, brow_inner_up, brow_outer_up,
                                eye_wide, eye_squint, eye_blink, jaw_open, mouth_press, nose_sneer, gaze_away, raw)
  select s->>'session', (s->>'idx')::integer, p_study, s->>'participant', nullif(s->>'utc', '')::timestamptz,
         public.fc_num(s->>'t_ms'), s->>'scene', public.fc_num(s->>'face_present')::smallint,
         public.fc_num(s->>'head_yaw'), public.fc_num(s->>'head_pitch'), public.fc_num(s->>'head_roll'),
         public.fc_num(s->>'smile'), public.fc_num(s->>'frown'), public.fc_num(s->>'brow_down'),
         public.fc_num(s->>'brow_inner_up'), public.fc_num(s->>'brow_outer_up'), public.fc_num(s->>'eye_wide'),
         public.fc_num(s->>'eye_squint'), public.fc_num(s->>'eye_blink'), public.fc_num(s->>'jaw_open'),
         public.fc_num(s->>'mouth_press'), public.fc_num(s->>'nose_sneer'), public.fc_num(s->>'gaze_away'), s
  from jsonb_array_elements(p_samples) s
  where s ? 'session' and s ? 'idx'
  on conflict (session, idx) do nothing;

  select array_agg(distinct s->>'session') into v_sessions from jsonb_array_elements(p_samples) s;
  if v_sessions is null then return '{}'::jsonb; end if;
  perform public.fc_refresh_sessions(p_study, v_sessions);

  select coalesce(jsonb_object_agg(x.session, x.max_idx), '{}'::jsonb) into v_result
  from (select a.session, max(a.idx) as max_idx from public.fc_affect a
        where a.session = any (v_sessions) group by a.session) x;
  return v_result;
end;
$$;

-- Diz até onde cada sessão já foi recebida (o jogo usa para retomar após reinstalar).
-- Também serve de "teste de conexão": com a chave errada, gera erro.
create or replace function public.fc_upload_status(p_study text, p_key text, p_sessions text[])
returns jsonb
language plpgsql stable security definer set search_path = public, extensions as $$
declare
  v_result jsonb;
begin
  if not public.fc_check_key(p_study, p_key, 'upload') then
    raise exception 'chave de envio invalida para o estudo %', p_study using errcode = '28000';
  end if;
  select coalesce(jsonb_object_agg(x.session, jsonb_build_object(
           'events', coalesce((select max(e.seq) from public.fc_events e where e.session = x.session), 0),
           'affect', coalesce((select max(a.idx) from public.fc_affect a where a.session = x.session), 0))), '{}'::jsonb)
    into v_result
  from unnest(coalesce(p_sessions, '{}'::text[])) as x(session);
  return v_result || jsonb_build_object('_study', jsonb_build_object(
    'sessions', (select count(*) from public.fc_sessions s where s.study_id = p_study),
    'events', (select count(*) from public.fc_events e where e.study_id = p_study),
    'affect', (select count(*) from public.fc_affect a where a.study_id = p_study)));
end;
$$;

-- ---------------------------------------------------------------- leitura (pesquisadores)

-- Eventos do estudo, paginados por (sessão, seq). Retorna as linhas originais.
create or replace function public.fc_export_events(p_study text, p_key text, p_after_session text default '',
                                                   p_after_seq integer default 0, p_limit integer default 1000)
returns setof jsonb
language plpgsql stable security definer set search_path = public, extensions as $$
begin
  if not public.fc_check_key(p_study, p_key, 'read') then
    raise exception 'chave de leitura invalida para o estudo %', p_study using errcode = '28000';
  end if;
  return query
    select e.raw from public.fc_events e
    where e.study_id = p_study and (e.session, e.seq) > (coalesce(p_after_session, ''), coalesce(p_after_seq, 0))
    order by e.session, e.seq
    limit least(greatest(coalesce(p_limit, 1000), 1), 1000);
end;
$$;

-- Amostras faciais do estudo, paginadas por (sessão, idx). Retorna as linhas originais.
create or replace function public.fc_export_affect(p_study text, p_key text, p_after_session text default '',
                                                   p_after_idx integer default 0, p_limit integer default 1000)
returns setof jsonb
language plpgsql stable security definer set search_path = public, extensions as $$
begin
  if not public.fc_check_key(p_study, p_key, 'read') then
    raise exception 'chave de leitura invalida para o estudo %', p_study using errcode = '28000';
  end if;
  return query
    select a.raw from public.fc_affect a
    where a.study_id = p_study and (a.session, a.idx) > (coalesce(p_after_session, ''), coalesce(p_after_idx, 0))
    order by a.session, a.idx
    limit least(greatest(coalesce(p_limit, 1000), 1), 1000);
end;
$$;

-- Visão geral das sessões do estudo (andamento da coleta).
create or replace function public.fc_study_overview(p_study text, p_key text)
returns setof jsonb
language plpgsql stable security definer set search_path = public, extensions as $$
begin
  if not public.fc_check_key(p_study, p_key, 'read') then
    raise exception 'chave de leitura invalida para o estudo %', p_study using errcode = '28000';
  end if;
  return query select to_jsonb(s) from public.fc_sessions s where s.study_id = p_study order by s.first_received;
end;
$$;

-- ---------------------------------------------------------------- administração

-- Cadastra (ou troca as chaves de) um estudo. Só pelo SQL Editor:
--   select public.fc_create_study('PILOTO26', '<chave de envio>', '<chave de leitura>');
-- O Painel do Estudo (Unity) gera as chaves e o comando pronto.
create or replace function public.fc_create_study(p_study text, p_upload_key text, p_read_key text)
returns text
language sql security definer set search_path = public, extensions as $$
  insert into public.fc_studies (study_id, upload_key_hash, read_key_hash)
  values (p_study, encode(extensions.digest(p_upload_key, 'sha256'), 'hex'), encode(extensions.digest(p_read_key, 'sha256'), 'hex'))
  on conflict (study_id) do update set upload_key_hash = excluded.upload_key_hash,
                                       read_key_hash = excluded.read_key_hash, active = true
  returning 'estudo ' || study_id || ' cadastrado';
$$;

-- ---------------------------------------------------------------- permissões

revoke all on function public.fc_check_key(text, text, text) from public, anon, authenticated;
revoke all on function public.fc_refresh_sessions(text, text[]) from public, anon, authenticated;
revoke all on function public.fc_create_study(text, text, text) from public, anon, authenticated;

grant execute on function public.fc_ingest_events(text, text, jsonb) to anon, authenticated;
grant execute on function public.fc_ingest_affect(text, text, jsonb) to anon, authenticated;
grant execute on function public.fc_upload_status(text, text, text[]) to anon, authenticated;
grant execute on function public.fc_export_events(text, text, text, integer, integer) to anon, authenticated;
grant execute on function public.fc_export_affect(text, text, text, integer, integer) to anon, authenticated;
grant execute on function public.fc_study_overview(text, text) to anon, authenticated;

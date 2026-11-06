# Learning Analytics: Fábulas Computacionais

Coleta de dados para pesquisa: **log de eventos do jogo** e **expressões faciais pela webcam**
(MediaPipe FaceLandmarker). Tudo é processado e gravado **localmente**. Nenhuma imagem ou vídeo é salvo.

## Arquivos

| Arquivo | Função |
|---|---|
| `TelemetryManager.cs` | Sessão, código do participante, log de eventos em JSONL, detecção de ociosidade. Criado automaticamente ao abrir o jogo (não precisa estar em nenhuma cena). |
| `AffectManager.cs` | Webcam → MediaPipe FaceLandmarker → 52 blendshapes + índices derivados + pose da cabeça → CSV. |
| `FaceAffectSample.cs` | Estrutura de uma amostra facial e cálculo dos índices derivados. |
| `ResearchConfig.cs` | Lê `Assets/StreamingAssets/research_config.json`. |
| `Editor/MediaPipeSetup.cs` | Ativa o símbolo `FABULAS_MEDIAPIPE` quando o plugin está instalado e copia o modelo para StreamingAssets. |

Os ganchos no jogo ficam em `SignupManager`, `LevelDataManager`, `GameManager` e `LoadNewArea`.

## 1. Instalar o MediaPipe Unity Plugin

O projeto usa **Unity 2021.3**. A versão mais recente do plugin (v0.16.x) exige Unity 2022.3, então use a **v0.15.0**,
a última compatível com 2021.3.

1. Baixe `com.github.homuler.mediapipe-0.15.0.tgz` (~345 MB) em
   https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.15.0
2. Coloque o arquivo em `Packages/` (na raiz do projeto, ao lado de `manifest.json`).
3. No Unity: **Window → Package Manager → + → Add package from tarball…** e selecione o `.tgz`.
4. Ao terminar de compilar, rode **Fabulas → Learning Analytics → Configurar MediaPipe**. O script:
   - adiciona o símbolo `FABULAS_MEDIAPIPE` (Player Settings → Scripting Define Symbols);
   - copia `face_landmarker_v2_with_blendshapes.bytes` para `Assets/StreamingAssets/MediaPipe/`.

   Se o modelo não estiver no pacote, baixe
   `https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task`,
   renomeie para `face_landmarker_v2_with_blendshapes.bytes` e coloque nessa pasta.

> **Git:** o `.tgz` passa do limite de 100 MB do GitHub. Adicione `Packages/*.tgz` ao `.gitignore`
> e distribua o arquivo à equipe por outro meio. Sem o plugin, o jogo compila e roda normalmente,
> só sem câmera.

No Windows, o plugin roda apenas em **CPU** e o suporte é marcado como experimental pelo autor.
Teste nas máquinas da escola antes da coleta.

## 2. Configurar a coleta (`Assets/StreamingAssets/research_config.json`)

Na build, o arquivo fica em `<Jogo>_Data/StreamingAssets/research_config.json` e pode ser editado em cada máquina.

| Campo | Padrão | Descrição |
|---|---|---|
| `telemetryEnabled` | `true` | Liga o log de eventos. |
| `idleThresholdSeconds` | `30` | Segundos sem input para registrar `idle_start`. |
| `affectCaptureEnabled` | `false` | **Liga a câmera.** Só ative com TCLE/TALE assinados. |
| `affectSampleRateHz` | `2` | Amostras por segundo (0.2–15). 2–4 é suficiente e leve. |
| `affectSummaryIntervalSeconds` | `10` | Intervalo do evento `affect_summary` no log de eventos. |
| `cameraDeviceName` | `""` | Nome da webcam; vazio = a primeira encontrada. |
| `cameraWidth/Height/Fps` | `640/480/15` | Resolução pedida à webcam. |
| `saveRawBlendshapes` | `true` | Grava os 52 blendshapes brutos no CSV. |
| `showCameraIndicator` | `true` | Mostra "● Câmera ativa (pesquisa)" no canto da tela. |

**Consentimento no jogo (opcional):** no painel de cadastro, crie um `Toggle` "Autorizo o uso da câmera"
e ligue o evento *On Value Changed* a `SignupManager.SetCameraConsent`. Sem o toggle, vale só
`affectCaptureEnabled`. Durante o jogo, **F9** pausa ou retoma a câmera, e isso fica registrado.

## 3. Dados gerados

Pasta: `Application.persistentDataPath/LearningAnalytics/`
(no Windows: `%USERPROFILE%\AppData\LocalLow\<Empresa>\<Produto>\LearningAnalytics`;
no editor, use **Fabulas → Learning Analytics → Abrir pasta de dados coletados**).

| Arquivo | Conteúdo |
|---|---|
| `participants_map.csv` | `participant_id;name;age;gender;school_grade;created_utc`. **É o único arquivo com nome.** Guarde separado dos dados de análise. |
| `events_<sessão>.jsonl` | Um evento JSON por linha. |
| `affect_<sessão>.csv` | Uma linha por amostra da webcam. |

### Formato de um evento

```json
{"t":"2026-11-05T14:03:22.4810000Z","t_ms":81234.5,"session":"20261105T140201Z-a1b2c3",
 "participant":"P-3F9A0C12","scene":"Musical 2","event":"puzzle_attempt",
 "data":{"level_type":"Musical","area":2,"attempt":3,"time_s":74.2}}
```

`t_ms` é o tempo desde o início da sessão (relógio monotônico). O CSV facial usa o mesmo relógio,
então dá para alinhar eventos e expressões.

### Eventos registrados

| Evento | Quando | Dados |
|---|---|---|
| `session_start` / `session_end` | cadastro concluído / volta ao menu ou saída | idade, gênero, série, versão, SO, controles / motivo, duração |
| `scene_enter` / `scene_exit` | troca de cena | nome, tempo na cena |
| `door_approach` / `door_leave` / `door_enter` | chega perto, se afasta ou entra numa porta | porta |
| `puzzle_start` | início de uma fase | tipo, área, tentativas e tempo anteriores |
| `puzzle_attempt` | cada teste da solução (`GetAttempt`) | tentativa nº, tempo |
| `puzzle_complete` | vitória (`WinLevel`) | tentativas, tempo |
| `jewel_earned` | joia nova | fase, total de joias, tempo de jogo |
| `pause_open` / `pause_close`, `mission_panel_toggle`, `scene_restart` | menus e reinício | |
| `idle_start` / `idle_end` | sem input por N s | duração |
| `app_focus` / `app_blur` | janela ganha ou perde foco | |
| `affect_capture_start` / `_stop` / `_paused` / `_resumed` | estado da câmera | câmera, resolução, motivo |
| `affect_unavailable` | falha (sem plugin, sem câmera, sem modelo…) | motivo |
| `affect_summary` | a cada N s | proporção de rosto presente, médias dos índices |

### Colunas de `affect_<sessão>.csv`

`utc, t_ms, session, participant, scene, face_present, head_yaw, head_pitch, head_roll`,
os índices derivados e `bs_*` (52 blendshapes, de 0 a 1; vazio quando não há rosto).

| Índice derivado | Blendshapes | Leitura possível (heurística) |
|---|---|---|
| `smile` | mouthSmile L/R | alegria/diversão |
| `frown` | mouthFrown L/R | desagrado |
| `brow_down` | browDown L/R | esforço, concentração ou confusão |
| `brow_inner_up` | browInnerUp | preocupação/surpresa |
| `brow_outer_up` | browOuterUp L/R | surpresa |
| `eye_wide` / `eye_squint` / `eye_blink` | eyeWide / eyeSquint / eyeBlink | surpresa / esforço / olhos fechados |
| `jaw_open` | jawOpen | surpresa, fala, bocejo |
| `mouth_press` | mouthPress L/R | tensão/frustração |
| `nose_sneer` | noseSneer L/R | desconforto |
| `gaze_away` | eyeLook* | olhar fora do centro |

`face_present = 0` por muito tempo, ou `|head_yaw|` alto, indica que o aluno não está olhando para a tela.

## 4. Limitações e cuidados

- **Os índices não são emoções.** Expressão facial não equivale a estado emocional (Barrett et al., 2019).
  Trate os blendshapes como variáveis comportamentais e **triangule** com autorrelato e com os eventos do jogo.
  Para inferir confusão ou frustração, treine modelos com rótulos de observação (ex.: BROMP) ou autorrelato.
- O modelo foi treinado majoritariamente com adultos. Valide com crianças e com a iluminação das escolas.
- **LGPD:** dados faciais derivados de crianças são dados pessoais sensíveis (arts. 11 e 14). É necessário
  CEP, TCLE dos responsáveis, TALE da criança, finalidade explícita, armazenamento seguro e prazo de descarte.
  O jogo nunca grava imagem, mas os blendshapes continuam sendo dados biométricos derivados.
- A inferência roda no thread principal, no ritmo de `affectSampleRateHz`. Em PCs fracos, reduza para 1–2 Hz.

## 5. Registrar novos eventos

```csharp
TelemetryManager.Log("note_selected", ("column", 3), ("note", "Do"), ("instrument", "Violao"));
```

Para reagir à expressão facial (ex.: abrir uma pergunta de autorrelato):

```csharp
AffectManager.Instance.SampleCaptured += s => { if (s.facePresent && s.derived[2] > 0.6f) { /* ... */ } };
```

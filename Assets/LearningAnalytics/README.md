# Learning Analytics: Fábulas Computacionais

Coleta de dados para pesquisa: **log de eventos do jogo**, **estado da solução a cada tentativa** e
**expressões faciais pela webcam** (MediaPipe FaceLandmarker). Tudo é processado e gravado **localmente**.
Nenhuma imagem ou vídeo é salvo.

> **Pesquisador:** comece pelo [Guia do Pesquisador](Guia_do_Pesquisador.docx) (protocolo antes, durante e
> depois da coleta). O que é coletado e como analisar está em
> [Learning_Analytics_Fabulas_Computacionais.docx](Learning_Analytics_Fabulas_Computacionais.docx).

## Arquivos

| Arquivo | Função |
|---|---|
| `TelemetryManager.cs` | Sessão, código do participante, log de eventos (JSONL) com `id` e `seq`, recuperação de sessões interrompidas, ociosidade. Criado automaticamente ao abrir o jogo. |
| `SolutionTracker.cs` / `MirrorsTracker.cs` | Estado da solução a cada tentativa nos quatro puzzles. |
| `AffectManager.cs` / `FaceAffectSample.cs` | Webcam → MediaPipe → 52 blendshapes, índices derivados e pose da cabeça → CSV. Modo de teste (prévia) sem gravar. |
| `ResearchConfig.cs` | Lê `StreamingAssets/research_config.json` (ou `-researchConfig <arquivo>`). |
| `ParticipantCodes.cs` | Códigos de participante com dígito verificador (soma ponderada módulo 31). |
| `ResearchSessionFlow.cs` | Valida o código, mostra a tela de consentimento e só então inicia a sessão. |
| `ResearcherPanel.cs` | Painel do Pesquisador no jogo (**Ctrl+Shift+R** + PIN). |
| `DataExporter.cs` | Exporta a coleta num `.zip` anonimizado com `manifest.json` (SHA-256). |
| `StudyConfigValidator.cs` | Confere a configuração e aponta riscos. |
| `catalogo_eventos.json` | **Catálogo de eventos** — fonte única para o codebook, o documento e os testes. |
| `Ferramentas/Editor/StudyPanel.cs` | **Painel do Estudo** no Unity (Fabulas → Learning Analytics → Painel do Estudo). |
| `Editor/MediaPipeSetup.cs` | Ativa o símbolo `FABULAS_MEDIAPIPE` quando o plugin está instalado e copia o modelo. |
| `Tests/Editor/` | 35 testes automatizados (Unity Test Framework). |
| `SelfTest/AnalyticsSelfTest.cs` | Autoteste de ponta a ponta no jogo compilado (só com argumento de linha de comando). |
| `Documentacao~/` | Geradores dos dois documentos `.docx` (a pasta termina em `~` para o Unity ignorá-la). |

Os ganchos no jogo ficam em `SignupManager`, `LevelDataManager`, `GameManager`, `LoadNewArea` e nos scripts dos puzzles.
A análise em R fica no pacote [`analysis/fabulasLA`](../../analysis/fabulasLA).

## 1. Instalar o MediaPipe Unity Plugin

O projeto usa **Unity 2021.3**. A versão mais recente do plugin (v0.16.x) exige Unity 2022.3, então use a **v0.15.0**.

1. Baixe `com.github.homuler.mediapipe-0.15.0.tgz` (~345 MB) em
   https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.15.0 e coloque em `Packages/`.
2. Abra o projeto; ao terminar de compilar, rode **Fabulas → Learning Analytics → Configurar MediaPipe**
   (ativa `FABULAS_MEDIAPIPE` e copia o modelo para `Assets/StreamingAssets/MediaPipe/`).

O `.tgz` passa do limite do GitHub e já está no `.gitignore`. Sem o plugin, o jogo funciona normalmente, só sem câmera.
No Windows o plugin roda em CPU e o suporte é experimental: teste nas máquinas da escola.

## 2. Configurar a coleta

Use o **Painel do Estudo** (Unity → Fabulas → Learning Analytics → Painel do Estudo): ele edita o
`research_config.json`, valida os campos e gera os códigos. Na build, o arquivo fica em
`<Jogo>_Data/StreamingAssets/research_config.json` e pode ser ajustado em cada máquina.

| Campo | Padrão | Descrição |
|---|---|---|
| `studyId` / `schoolId` / `classId` | `""` | Estudo, escola e turma (vão para cada sessão e para o nome da exportação). |
| `machineId` | `""` | Computador; vazio = nome do computador. |
| `conditions` / `conditionAssignment` / `condition` | `[]` / `fixed` / `""` | Condições A/B: `fixed` (por máquina), `random` (sorteio por sessão) ou `by_participant` (estável). |
| `participantIdMode` | `name` | `name`: nome no cadastro (mapa separado). `code`: **código de estudo**, sem nomes no computador. |
| `requireConsent` / `consentTextFile` | `false` / `consent_text.txt` | Tela de consentimento antes da coleta. |
| `researcherPin` | `1234` | PIN do Painel do Pesquisador (troque!). Vazio = sem senha. |
| `exportFolder` / `exportIncludesNameMap` | `""` / `false` | Pasta da exportação e se o mapa de nomes vai junto. |
| `telemetryEnabled` | `true` | Liga o log de eventos. |
| `idleThresholdSeconds` | `30` | Segundos sem input para registrar `idle_start`. |
| `affectCaptureEnabled` | `false` | **Liga a câmera.** Só com TCLE/TALE assinados. |
| `affectSampleRateHz` / `affectSummaryIntervalSeconds` | `2` / `10` | Amostras faciais por segundo e intervalo do resumo. |
| `cameraDeviceName` / `cameraWidth/Height/Fps` | `""` / `640/480/15` | Webcam e resolução. |
| `saveRawBlendshapes` / `showCameraIndicator` | `true` / `true` | 52 blendshapes brutos e aviso de câmera ativa. |

Linha de comando: `-researchConfig <arquivo>` usa outra configuração e `-analyticsDataFolder <pasta>` grava os dados em outra pasta.

## 3. Ferramentas do pesquisador

- **Códigos de participante:** o Painel do Estudo gera códigos (ex.: `K7M2X`) e uma página de **etiquetas para imprimir**
  em `Pesquisa/`. O último caractere é verificador: qualquer erro de um caractere ou troca de vizinhos é recusada no jogo.
- **Tela de consentimento** (`requireConsent`): texto em `StreamingAssets/consent_text.txt`, com autorização separada para a câmera.
  Quem não concorda joga normalmente, sem registro.
- **Painel do Pesquisador** (no jogo, **Ctrl+Shift+R** + PIN): estudo, sessão, eventos gravados, pasta, espaço em disco,
  **teste da câmera** com prévia e "rosto detectado" (sem gravar) e **Exportar coleta (.zip)**.
- **Exportação:** `coleta_<estudo>_<escola>_<máquina>_<data>.zip` com eventos, dados faciais e `manifest.json` (SHA-256 de cada arquivo).
  Por padrão **sem** o mapa de nomes.

## 4. Dados gerados

Pasta: `Application.persistentDataPath/LearningAnalytics/` (Windows:
`%USERPROFILE%\AppData\LocalLow\DefaultCompany\Fabulas Computacionais\LearningAnalytics`).

| Arquivo | Conteúdo |
|---|---|
| `events_<sessão>.jsonl` | Um evento JSON por linha. |
| `affect_<sessão>.csv` | Uma linha por amostra da webcam. |
| `participants_map.csv` | Só no modo `name`: nome ↔ código. **Único arquivo com nome.** |
| `Exportacoes/coleta_*.zip` | Exportações feitas pelo Painel do Pesquisador. |

```json
{"id":"9c1f0d2e…","seq":42,"t":"2026-11-05T14:03:22.4810000Z","t_ms":81234.5,"session":"20261105T140201Z-a1b2c3",
 "participant":"K7M2X","scene":"Musical 2","event":"puzzle_attempt",
 "data":{"level_type":"Musical","area":2,"attempt":3,"time_s":74.2}}
```

`t_ms` é o tempo desde o início da sessão (relógio monotônico), o mesmo do CSV facial. A lista completa de eventos
e campos está no [catálogo](catalogo_eventos.json) e no codebook (`fabulasLA::exportar_codebook()`).

### Estado da solução (`solution_snapshot`)

| Puzzle | Momento | `slots[i]` | Campos extras |
|---|---|---|---|
| **Musical** | fim da reprodução | nota na posição *i* | veredito por partitura (`part*_repeat`), repetição escolhida, `blocked_slots_used` |
| **Conjuntos** | todos os espaços preenchidos | tipos dos objetos no espaço *i* | `objects`, `misplaced_objects`, `confusions` (`esperado->colocado`) |
| **Receitas** | ao cozinhar | `insumo+ação` do passo *i* | `step_errors` (`ok`, `empty_*`, `wrong_*`, `order`) |
| **Espelhos** | ao disparar a luz | rotação/posição do pilar *i* | `pillars`; depois `beam_result` e, na vitória, `solution_final` |

Campos comuns: `slots`, `expected`, `correct_positions`, `distance`, `first_error`, `changed_positions`, `n_changed`, `success`.

## 5. Qualidade e integridade

- Todo evento tem **`id`** (único) e **`seq`** (1, 2, 3… por sessão): duplicatas e perdas são detectáveis.
- **Sessões interrompidas:** se o jogo fechar sem encerrar a sessão, a próxima abertura acrescenta
  `session_end` com `reason = "interrupted"` e `recovered = true`.
- Os eventos são gravados no disco imediatamente (sem perda em caso de queda).
- `fabulasLA::validar_coleta()` confere integridade (manifesto), duplicatas, lacunas, início/fim, eventos fora do catálogo e cobertura da câmera.

## 6. Análise em R (`fabulasLA`)

```r
install.packages("analysis/fabulasLA", repos = NULL, type = "source")   # uma vez
library(fabulasLA)
coleta <- ler_coleta(c("escola_A.zip", "escola_B.zip"))   # pastas e/ou .zip
validar_coleta(coleta)
tb <- tabelas_analise(coleta)                             # sessoes, fases, tentativas, afeto_sessao
gerar_relatorio(c("escola_A.zip", "escola_B.zip"), "relatorio.html")
exportar_codebook("codebook.csv")
```

## 7. Testes automatizados

| Testes | Como rodar | O que cobrem |
|---|---|---|
| **Unity (35)** | Window → General → Test Runner → EditMode, ou `Unity.exe -batchmode -runTests -testPlatform EditMode -projectPath .` | JSON dos eventos, sequência e ids, recuperação de sessão, códigos (todo erro de 1 caractere e troca de vizinhos), condições, validador, exportação e checksums, estado da solução, índices faciais, catálogo ↔ código ↔ pacote R |
| **Pacote R** | `testthat::test_dir("analysis/fabulasLA/tests/testthat", package = "fabulasLA", load_package = "installed")` | leitura, validação (incluindo dados corrompidos), tabelas, codebook, códigos; com `FABULAS_ZIP_TESTE=<zip>`, lê um .zip do jogo |
| **Ponta a ponta** | `FabulasComputacionais.exe -batchmode -nographics -analyticsSelfTest completo -selfTestOut <pasta> -researchConfig <config> -analyticsDataFolder <pasta>` (modos: `interromper`, `completo`, `codigo_invalido`) | jogo real: cadastro com código, consentimento, fase com tentativas e vitória, painel, exportação, queda simulada e código inválido; o .zip é validado com o pacote R |

## 8. Limitações e cuidados

- **Os índices faciais não são emoções** (Barrett et al., 2019): triangule com autorrelato, observação e eventos do jogo.
- O modelo foi treinado majoritariamente com adultos. Valide com crianças e com a iluminação das escolas.
- **LGPD:** dados faciais derivados de crianças são dados pessoais sensíveis (arts. 11 e 14): CEP, TCLE, TALE,
  finalidade explícita, armazenamento seguro e prazo de descarte.
- A inferência facial roda no thread principal, no ritmo de `affectSampleRateHz`. Em PCs fracos, use 1–2 Hz.
- As condições A/B são registradas, mas as variações do jogo ainda precisam ser implementadas (consultando `TelemetryManager.Instance.Condition`).

## 9. Documentação

`Learning_Analytics_Fabulas_Computacionais.docx` (funcionalidades e exemplos de análise) e `Guia_do_Pesquisador.docx`
(protocolo de coleta) **devem ser atualizados sempre que a coleta mudar**. Eles são gerados por scripts:

```bash
cd Assets/LearningAnalytics/Documentacao~ && npm install && node gerar_documento.js && node gerar_guia.js
```

Depois, abra no Word e atualize o sumário (botão direito → Atualizar campo → Atualizar o índice inteiro).
Os apêndices do documento de funcionalidades vêm do `catalogo_eventos.json`.

## 10. Registrar novos eventos

```csharp
TelemetryManager.Log("note_selected", ("column", 3), ("note", "Do"), ("instrument", "Violao"));
```

1. Inclua o evento em `catalogo_eventos.json` (o teste `TodoEventoUsadoNoCodigo_EstaNoCatalogo` falha se esquecer).
2. Copie o catálogo para `analysis/fabulasLA/inst/extdata/` (outro teste confere a sincronia).
3. Regere os documentos.

Para reagir à expressão facial (ex.: abrir uma pergunta de autorrelato):

```csharp
AffectManager.Instance.SampleCaptured += s => { if (s.facePresent && s.derived[2] > 0.6f) { /* ... */ } };
```

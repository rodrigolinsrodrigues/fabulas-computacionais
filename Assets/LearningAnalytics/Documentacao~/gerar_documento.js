const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, HeadingLevel, Table, TableRow, TableCell,
  WidthType, ShadingType, BorderStyle, AlignmentType, LevelFormat, Footer, Header,
  PageNumber, TableOfContents, PageBreak,
} = require("docx");

// ---------------------------------------------------------------- estilo
const FONT = "Arial";
const MONO = "Consolas";
const BLUE = "1F4E79";
const LIGHT = "DEEAF6";
const GREY = "F2F2F2";
const AMBER = "FFF4CE";
const GREEN = "E2F0D9";
const CONTENT = 9026; // A4 com margens de 1"

// Texto com **negrito** e `código`
function runs(text, base = {}) {
  const out = [];
  const re = /(\*\*[^*]+\*\*|`[^`]+`)/g;
  let last = 0, m;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) out.push(new TextRun({ text: text.slice(last, m.index), ...base }));
    const tok = m[0];
    if (tok.startsWith("**")) out.push(new TextRun({ text: tok.slice(2, -2), bold: true, ...base }));
    else out.push(new TextRun({ text: tok.slice(1, -1), font: MONO, size: (base.size || 22) - 2, color: "7B2C2C", ...base.codeOverride }));
    last = m.index + tok.length;
  }
  if (last < text.length) out.push(new TextRun({ text: text.slice(last), ...base }));
  return out;
}

const P = (text, opts = {}) => new Paragraph({ children: runs(text, opts.run), spacing: { after: 120, line: 276 }, alignment: opts.align, ...opts.para });
const H1 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(t)] });
const H2 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(t)] });
const H3 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_3, children: [new TextRun(t)] });
const B = (text, level = 0) => new Paragraph({ numbering: { reference: "bullets", level }, children: runs(text), spacing: { after: 60, line: 264 } });
const N = (text, ref = "numbers") => new Paragraph({ numbering: { reference: ref, level: 0 }, children: runs(text), spacing: { after: 60, line: 264 } });
const BR = () => new Paragraph({ children: [new PageBreak()] });

function code(lines) {
  return lines.map((l, i) => new Paragraph({
    children: [new TextRun({ text: l.length ? l : " ", font: MONO, size: 17 })],
    shading: { type: ShadingType.CLEAR, fill: GREY, color: "auto" },
    spacing: { before: i === 0 ? 60 : 0, after: i === lines.length - 1 ? 160 : 0, line: 240 },
    indent: { left: 120, right: 120 },
    keepLines: true,
    keepNext: i !== lines.length - 1,
  }));
}

// Caixa destacada (uma célula), ex.: exemplo de análise
const border = { style: BorderStyle.SINGLE, size: 4, color: "BFBFBF" };
const borders = { top: border, bottom: border, left: border, right: border };
function box(title, paragraphs, fill = LIGHT) {
  return new Table({
    width: { size: CONTENT, type: WidthType.DXA },
    columnWidths: [CONTENT],
    rows: [new TableRow({
      cantSplit: true,
      children: [new TableCell({
        width: { size: CONTENT, type: WidthType.DXA },
        borders,
        shading: { type: ShadingType.CLEAR, fill, color: "auto" },
        margins: { top: 100, bottom: 100, left: 160, right: 160 },
        children: [
          new Paragraph({ children: [new TextRun({ text: title, bold: true, color: BLUE })], spacing: { after: 80 } }),
          ...paragraphs,
        ],
      })],
    })],
  });
}
const gap = () => new Paragraph({ children: [], spacing: { after: 120 } });

// Exemplo de análise padronizado
function analysis(n, title, { pergunta, dados, metodo, resultado }) {
  const items = [
    new Paragraph({ children: runs(`**Pergunta de pesquisa:** ${pergunta}`), spacing: { after: 60 } }),
    new Paragraph({ children: runs(`**Dados usados:** ${dados}`), spacing: { after: 60 } }),
    new Paragraph({ children: runs(`**Método:** ${metodo}`), spacing: { after: 60 } }),
    new Paragraph({ children: runs(`**Exemplo de resultado (ilustrativo, dados fictícios):** ${resultado}`, { italics: false }), spacing: { after: 0 } }),
  ];
  return [box(`Exemplo de análise ${n} — ${title}`, items), gap()];
}

function table(headers, rows, widths, opts = {}) {
  const total = widths.reduce((a, b) => a + b, 0);
  const mk = (text, isHeader, w) => new TableCell({
    width: { size: w, type: WidthType.DXA },
    borders,
    shading: { type: ShadingType.CLEAR, fill: isHeader ? BLUE : "FFFFFF", color: "auto" },
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    children: String(text).split("\n").map((line) => new Paragraph({
      children: isHeader ? [new TextRun({ text: line, bold: true, color: "FFFFFF", size: 19 })] : runs(line, { size: 19 }),
      spacing: { after: 0, line: 252 },
    })),
  });
  return [new Table({
    width: { size: total, type: WidthType.DXA },
    columnWidths: widths,
    rows: [
      new TableRow({ tableHeader: true, children: headers.map((h, i) => mk(h, true, widths[i])) }),
      ...rows.map((r) => new TableRow({ cantSplit: true, children: r.map((c, i) => mk(c, false, widths[i])) })),
    ],
  }), gap()];
}

// ---------------------------------------------------------------- conteúdo
const children = [];
const add = (...xs) => xs.flat().forEach((x) => children.push(x));

// Capa
add(
  new Paragraph({ children: [], spacing: { before: 2400 } }),
  new Paragraph({ children: [new TextRun({ text: "Fábulas Computacionais", font: FONT, size: 52, bold: true, color: BLUE })], alignment: AlignmentType.CENTER, spacing: { after: 200 } }),
  new Paragraph({ children: [new TextRun({ text: "Funcionalidades de Learning Analytics implementadas", font: FONT, size: 32, color: "404040" })], alignment: AlignmentType.CENTER, spacing: { after: 120 } }),
  new Paragraph({ children: [new TextRun({ text: "e exemplos de análises de pesquisa", font: FONT, size: 32, color: "404040" })], alignment: AlignmentType.CENTER, spacing: { after: 1200 } }),
  new Paragraph({ children: [new TextRun({ text: "Documento técnico-científico", size: 22, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 80 } }),
  new Paragraph({ children: [new TextRun({ text: "Responsável: Rodrigo Lins Rodrigues", size: 22, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 80 } }),
  new Paragraph({ children: [new TextRun({ text: "Novembro de 2026", size: 22, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 80 } }),
  new Paragraph({ children: runs("Versão do código: branch `feature/learning-analytics-mediapipe` · esquema de dados 1.1", { size: 20, color: "595959" }), alignment: AlignmentType.CENTER }),
  BR(),
);

// Sumário
add(
  new Paragraph({ children: [new TextRun({ text: "Sumário", font: FONT, size: 32, bold: true, color: BLUE })], spacing: { after: 200 } }),
  new TableOfContents("Sumário", { hyperlink: true, headingStyleRange: "1-2" }),
  BR(),
);

// 1. Introdução
add(
  H1("1. Introdução"),
  P("Este documento descreve todas as funcionalidades de **learning analytics** atualmente implementadas no jogo Fábulas Computacionais (Unity 2021.3) e, para cada uma delas, apresenta **exemplos de análises de pesquisa** que podem ser realizadas com os dados coletados."),
  P("O jogo trabalha conceitos de pensamento computacional por meio de quatro tipos de puzzle — **Musical** (sequência e repetição), **Espelhos** (raciocínio espacial e depuração), **Conjuntos** (classificação e agrupamento) e **Receitas** (algoritmos passo a passo) —, distribuídos em três áreas externas que funcionam como mundo central (hub). A camada de learning analytics registra, sem alterar a jogabilidade, o que a criança faz, como resolve cada desafio e, opcionalmente, suas expressões faciais pela webcam."),
  box("Como ler os exemplos de análise", [
    P("Cada exemplo segue a estrutura **pergunta de pesquisa → dados usados → método → exemplo de resultado**. Os resultados apresentados são **ilustrativos, com números fictícios**, e servem apenas para mostrar o tipo de conclusão que a análise permite. Nenhum dado real de participantes foi utilizado neste documento.", { para: { spacing: { after: 0 } } }),
  ], AMBER),
  gap(),
);

// 2. Arquitetura
add(
  H1("2. Visão geral da arquitetura"),
  P("A coleta é feita localmente, no computador onde o jogo roda, sem necessidade de internet. Opcionalmente, uma **cópia** de todos os eventos e de todas as amostras faciais é enviada a um **servidor central (Supabase)** sempre que houver conexão; os arquivos locais continuam sendo gravados e servem de backup. Os componentes são criados automaticamente quando o jogo abre — não é preciso alterar nenhuma cena."),
  H2("2.1 Componentes"),
  ...table(["Componente", "Função"], [
    ["`TelemetryManager`", "Gerencia a sessão, gera o código pseudônimo do participante e grava todos os eventos do jogo (JSON Lines). Também detecta ociosidade e perda de foco da janela."],
    ["`SolutionTracker`", "Registra o estado da solução a cada tentativa nos quatro puzzles, comparando com o gabarito e com a tentativa anterior."],
    ["`MirrorsTracker`", "Lógica específica do puzzle Espelhos: configuração dos pilares, alcance do feixe de luz e configuração vencedora."],
    ["`AffectManager`", "Captura de expressões faciais pela webcam com MediaPipe FaceLandmarker (52 blendshapes, índices derivados e orientação da cabeça)."],
    ["`ResearchConfig`", "Lê o arquivo `research_config.json`, que liga ou desliga a coleta em cada máquina sem recompilar o jogo."],
    ["`ParticipantCodes`", "Códigos de participante com dígito verificador (detecta qualquer erro de digitação de um caractere e trocas de caracteres vizinhos)."],
    ["`ResearchSessionFlow`", "Fluxo de início da sessão: valida o código, mostra a tela de consentimento e só então inicia a coleta."],
    ["`ResearcherPanel`", "Painel do Pesquisador no jogo (Ctrl+Shift+R + PIN): estado da coleta, teste da câmera com prévia e exportação."],
    ["`DataExporter`", "Exporta a coleta num .zip anonimizado com manifesto e checksums (SHA-256)."],
    ["`StudyConfigValidator` / Painel do Estudo", "Configuração do estudo no Unity sem editar JSON, com avisos de riscos, geração de etiquetas de códigos e das chaves do servidor."],
    ["`DataUploader` / `SyncEngine`", "Envio em segundo plano ao servidor central: lotes, nova tentativa sem internet, retomada de onde parou e nenhum dado local apagado."],
    ["Servidor central (`server/supabase/schema.sql`)", "Banco PostgreSQL no Supabase com funções de recebimento e de leitura protegidas por chaves por estudo; ignora envios repetidos."],
    ["`catalogo_eventos.json`", "Catálogo de eventos: fonte única para o codebook, este documento e um teste que garante que o jogo só registra eventos documentados."],
    ["Pacote R `fabulasLA`", "Leitura de pastas e .zip de várias escolas, validação, tabelas de análise, codebook e relatório completo (pasta `analysis/fabulasLA`)."],
    ["Ganchos nos scripts do jogo", "Linhas de registro adicionadas em `SignupManager`, `LevelDataManager`, `GameManager`, `LoadNewArea`, `checarPartitura`, `LeituraEspacos`, `SnapNovo`, `PickUp`, `EmitLight`, `StopScale`, `showCanvas`, `RotateObj`, `Launch`, `Lente`, `LerMesa` e `Snap`."],
  ], [2400, 6626]),
  H2("2.2 Arquivos gerados"),
  P("Pasta: `%USERPROFILE%\\AppData\\LocalLow\\DefaultCompany\\Fabulas Computacionais\\LearningAnalytics\\`. Cada sessão (do cadastro até a volta ao menu ou fechamento do jogo) gera um conjunto de arquivos:"),
  ...table(["Arquivo", "Formato", "Conteúdo"], [
    ["`events_<sessão>.jsonl`", "JSON Lines", "Um evento por linha: navegação, fases, tentativas, estado da solução, ações, engajamento e resumos afetivos."],
    ["`affect_<sessão>.csv`", "CSV (vírgula)", "Uma linha por amostra da webcam (padrão: 2 por segundo), com 52 blendshapes, índices derivados e pose da cabeça."],
    ["`participants_map.csv`", "CSV (ponto e vírgula)", "Tabela que liga o nome da criança ao código pseudônimo. É o único arquivo com nome e deve ser guardado separadamente. **Nunca** é enviado ao servidor."],
    ["`.sync_state.tsv`", "Texto (tabulação)", "Só com o envio ao servidor ligado: até que evento e amostra o servidor confirmou o recebimento, por sessão."],
  ], [2700, 1700, 4626]),
  H2("2.3 Estrutura de um evento"),
  P("Todos os eventos compartilham os mesmos campos de identificação e de tempo, o que permite cruzá-los entre si e com os dados faciais:"),
  ...code([
    '{"id":"9c1f…e07a", "seq":42, "t":"2026-11-05T14:03:22.481Z", "t_ms":81234.5,',
    ' "session":"20261105T140201Z-a1b2c3", "participant":"K7M2X",',
    ' "scene":"Musical 2", "event":"puzzle_attempt",',
    ' "data":{"level_type":"Musical","area":2,"attempt":3,"time_s":74.2}}',
  ]),
  ...table(["Campo", "Significado"], [
    ["`id`", "Identificador único do evento: permite detectar duplicatas ao juntar exportações."],
    ["`seq`", "Número de sequência na sessão (1, 2, 3…): lacunas indicam eventos perdidos."],
    ["`t`", "Data e hora UTC (ISO 8601)."],
    ["`t_ms`", "Milissegundos desde o início da sessão (relógio monotônico). É o mesmo relógio do arquivo facial, usado para sincronização."],
    ["`session` / `participant`", "Identificador da sessão e código do participante (pseudônimo gerado ou código de estudo)."],
    ["`scene`", "Cena (fase ou área) em que o evento ocorreu."],
    ["`event` / `data`", "Tipo do evento e campos específicos do evento."],
  ], [2400, 6626]),
);

// 3. Funcionalidades
add(BR(), H1("3. Funcionalidades e exemplos de análise"));

// 3.1
add(
  H2("3.1 Sessão e perfil pseudonimizado do participante"),
  P("**O que é coletado.** Ao concluir o cadastro, o jogo inicia uma sessão e registra o evento `session_start` com idade, gênero, série escolar, versão do jogo, sistema operacional, modelo do computador, resolução da tela e controles conectados. O nome **não** vai para o log: o participante recebe um código estável (ex.: `P-3F9A0C12`), e a ligação nome↔código fica apenas em `participants_map.csv`. A sessão termina com `session_end` (motivo e duração), seja ao voltar ao menu, seja ao fechar o jogo."),
  ...code(['{"event":"session_start","data":{"age":9,"gender":"f","school_grade":"4º ano",', ' "game_version":"0.1","os":"Windows 11","screen":"1920x1080","joysticks":"Xbox Controller"}}']),
);
add(...analysis(1, "Caracterização da amostra e equidade", {
  pergunta: "O desempenho no jogo difere por série escolar ou por gênero?",
  dados: "`session_start` (série, gênero, idade) unido a `puzzle_complete` (tentativas e tempo por fase).",
  metodo: "Estatística descritiva da amostra; modelo linear misto com tentativas até a conclusão como variável dependente, série e gênero como efeitos fixos e participante e escola como efeitos aleatórios (alunos aninhados em escolas).",
  resultado: "Alunos do 5º ano precisaram em média de 2,1 tentativas por fase contra 3,4 do 3º ano; não houve diferença significativa por gênero após controlar a série — indício de que o jogo não favorece um grupo específico.",
}));
add(...analysis(2, "Efeito do dispositivo de entrada", {
  pergunta: "Jogar com controle (gamepad) ou com teclado e mouse altera o tempo de conclusão?",
  dados: "Campo `joysticks` de `session_start` e `time_s` de `puzzle_complete`.",
  metodo: "Comparação de medianas (Mann-Whitney) por tipo de puzzle, já que tempos costumam ter distribuição assimétrica.",
  resultado: "Nos puzzles com manipulação 3D (Espelhos e Conjuntos), quem usou teclado levou 18% mais tempo; no Musical, baseado em menus, não houve diferença. Recomenda-se padronizar o controle nos estudos.",
}));

// 3.2
add(
  H2("3.2 Navegação no mundo do jogo (cenas e portas)"),
  P("**O que é coletado.** Cada troca de cena gera `scene_enter` e `scene_exit` (com o tempo passado na cena). Nas áreas externas, o jogo registra quando a criança se aproxima de uma porta (`door_approach`), quando se afasta sem entrar (`door_leave`) e quando entra (`door_enter`)."),
  ...code(['{"event":"door_approach","scene":"Area1","data":{"door":"Musical 1"}}', '{"event":"door_leave","scene":"Area1","data":{"door":"Musical 1"}}', '{"event":"door_enter","scene":"Area1","data":{"door":"Conjuntos Fase 1"}}']),
);
add(...analysis(3, "Ordem de escolha das fases", {
  pergunta: "Em que ordem as crianças escolhem os puzzles e isso influencia o desempenho?",
  dados: "Sequência de `door_enter` por participante; `puzzle_complete`.",
  metodo: "Mineração de processos (process mining, ex.: pm4py ou bupaR) para obter o mapa de fluxo mais frequente; análise de sequências (TraMineR) e agrupamento de trajetórias.",
  resultado: "62% das crianças começaram pelo Conjuntos; quem iniciou pelo Musical concluiu a Área 1 em menos tempo total, sugerindo que a ordem das fases pode ser usada como variável de design.",
}));
add(...analysis(4, "Evitação de desafios", {
  pergunta: "Há puzzles que as crianças evitam?",
  dados: "Pares `door_approach` → `door_leave` sem `door_enter` na mesma porta.",
  metodo: "Taxa de evitação por porta = aproximações sem entrada ÷ total de aproximações; comparação entre tipos de puzzle e relação com autoeficácia (questionário).",
  resultado: "A porta do Espelhos teve 41% de evitação na primeira visita, contra 12% das demais — sinal de que a apresentação inicial desse puzzle pode estar intimidando.",
}));

// 3.3
add(
  H2("3.3 Desempenho por fase: início, tentativas, conclusão e joias"),
  P("**O que é coletado.** Ao entrar numa fase, `puzzle_start` registra o tipo, a área e o histórico anterior. Cada vez que a criança testa a solução, `puzzle_attempt` registra o número da tentativa e o tempo acumulado. Na vitória, `puzzle_complete` registra tentativas e tempo totais, e `jewel_earned` registra a joia obtida e o total acumulado."),
  ...table(["Puzzle", "O que conta como tentativa"], [
    ["Musical", "Apertar Play para tocar a partitura montada."],
    ["Conjuntos", "Preencher todos os espaços (o jogo então verifica a classificação)."],
    ["Espelhos", "Disparar a luz no emissor."],
    ["Receitas", "Iniciar o preparo (o personagem executa os passos montados)."],
  ], [2000, 7026]),
);
add(...analysis(5, "Curva de aprendizagem entre fases", {
  pergunta: "As crianças melhoram ao longo das três fases de um mesmo tipo de puzzle?",
  dados: "`puzzle_complete` (tentativas e tempo) nas Fases 1, 2 e 3 de cada tipo.",
  metodo: "Modelo linear misto (tentativas ~ fase × tipo + (1 | participante)); visualização das curvas de aprendizagem por tipo.",
  resultado: "No Conjuntos, as tentativas caíram de 3,2 (Fase 1) para 1,8 (Fase 3), apesar do aumento de dificuldade; no Musical, subiram na Fase 2, quando a repetição é introduzida — o conceito de laço é o ponto crítico.",
}));
add(...analysis(6, "Tempo até concluir e abandono", {
  pergunta: "Quanto tempo as crianças levam para concluir cada fase e quantas desistem?",
  dados: "`puzzle_start`, `puzzle_complete` e `scene_exit` (saída sem conclusão).",
  metodo: "Análise de sobrevivência (curvas de Kaplan-Meier), com a saída sem vitória tratada como censura; regressão de Cox com tipo de puzzle e série como covariáveis.",
  resultado: "Metade das crianças concluiu o Espelhos Fase 2 em até 6 min; 15% saíram antes de concluir, quase todas após 3 tentativas sem progresso — um limiar útil para oferecer ajuda.",
}));

// 3.4
add(
  H2("3.4 Engajamento e comportamento de jogo"),
  P("**O que é coletado.** Abertura e fechamento da pausa (`pause_open`/`pause_close`), consulta ao painel de missão (`mission_panel_toggle`), reinício de cena (`scene_restart`), períodos sem nenhum input acima do limite configurado (`idle_start`/`idle_end`, padrão 30 s) e perda ou retorno de foco da janela do jogo (`app_blur`/`app_focus`)."),
);
add(...analysis(7, "Ociosidade como sinal de dificuldade ou desengajamento", {
  pergunta: "Períodos de ociosidade antecedem o abandono de uma fase?",
  dados: "`idle_start`/`idle_end` (duração), `puzzle_attempt` e `scene_exit` sem conclusão.",
  metodo: "Regressão logística multinível: abandono ~ número e duração de períodos ociosos + tentativas sem progresso. Validação cruzada por participante.",
  resultado: "Cada período ocioso acima de 30 s aumentou em 1,6 vez a chance de abandono; combinada às tentativas sem progresso, a ociosidade prevê 74% dos abandonos (AUC = 0,81).",
}));
add(...analysis(8, "Busca de ajuda e autorregulação", {
  pergunta: "Quem consulta a missão com mais frequência resolve as fases em menos tentativas?",
  dados: "Contagem de `mission_panel_toggle` por fase; `puzzle_complete`.",
  metodo: "Correlação de Spearman e análise de sequência: em que momento a consulta acontece (antes da 1ª tentativa ou após erros).",
  resultado: "Consultar a missão antes da primeira tentativa associou-se a menos tentativas (ρ = −0,34); consultas apenas após vários erros não mostraram relação, indicando diferenças de planejamento.",
}));

// 3.5
add(
  H2("3.5 Estado da solução a cada tentativa"),
  P("**O que é coletado.** Esta é a funcionalidade de maior valor analítico. Cada vez que a criança testa a solução, o evento `solution_snapshot` registra **o que ela montou** em cada posição (`slots`), **o gabarito** (`expected`), quantas posições estão certas, a distância até a solução, a primeira posição errada e **quais posições mudaram desde a tentativa anterior** na mesma fase. Assim, o registro deixa de dizer apenas se a criança acertou e passa a mostrar **como ela raciocina e corrige**."),
  ...table(["Campo comum", "Significado"], [
    ["`slots` / `expected`", "O que está em cada posição e a resposta esperada (o Espelhos não tem gabarito fixo)."],
    ["`correct_positions` / `distance`", "Posições certas e quantas faltam para a solução."],
    ["`first_error`", "Índice da primeira posição errada (−1 = nenhuma)."],
    ["`changed_positions` / `n_changed`", "Posições alteradas desde a tentativa anterior (nulo na primeira)."],
    ["`success`", "Veredito do próprio jogo para a tentativa."],
  ], [3000, 6026]),
  ...code([
    '{"event":"solution_snapshot","scene":"Musical 2","data":{"puzzle":"Musical","attempt":3,',
    ' "slots":["Do","Mi","Sol","Do","Mi","Sol","La",""],',
    ' "expected":["Do","Mi","Sol","Do","Mi","Sol","Fa","Sol"],',
    ' "correct_positions":6,"distance":2,"first_error":6,',
    ' "changed_positions":[6,7],"n_changed":2,"success":false, ...}}',
  ]),
);
add(...analysis(9, "Depuração incremental versus tentativa e erro", {
  pergunta: "As crianças corrigem a solução de forma localizada (depuração) ou mudam tudo a cada tentativa?",
  dados: "Sequência de `solution_snapshot` por participante e fase: `distance` e `n_changed`.",
  metodo: "Classificação de cada transição entre tentativas: **localizada** (poucas posições alteradas, perto do erro, distância diminui), **reinício** (maioria das posições alteradas) ou **repetição** (nada alterado). Agrupamento dos participantes pelas proporções de cada tipo (k-means ou análise de classes latentes).",
  resultado: "Emergiram três perfis: 'depuradores' (48%, distância cai a cada tentativa), 'exploradores' (31%, muitas mudanças, mas convergem) e 'tentativa e erro' (21%, distância oscila). O perfil 'depurador' na Área 1 previu melhor desempenho na Área 3.",
}));
add(...analysis(10, "Mapa de equívocos", {
  pergunta: "Quais erros são mais comuns em cada fase?",
  dados: "`first_error` e a comparação posição a posição entre `slots` e `expected`.",
  metodo: "Frequência de erro por posição e por fase; mapa de calor posição × fase.",
  resultado: "Na Fase 2 do Musical, 70% das tentativas erradas falham na penúltima nota, exatamente onde a repetição termina — indício de dificuldade com o limite do laço, e não com as notas.",
}));

add(H3("3.5.1 Musical — sequência e repetição"));
add(P("Registrado ao fim da reprodução da partitura. Além das notas por posição, inclui o início, o fim e o contador de repetição escolhidos, as posições esperadas da repetição, quantas posições bloqueadas foram usadas, a quantidade de repetições selecionada e o veredito separado de cada partitura (`part1_notes`, `part1_repeat`, `part2_notes`, `part2_repeat`)."));
add(...analysis(11, "Compreensão do conceito de laço", {
  pergunta: "As crianças usam a repetição para evitar copiar notas ou escrevem a sequência inteira à mão?",
  dados: "Campos de repetição (`p1_repeat_start`, `p1_repeat_end`, `p1_repeat_counter`, `p1_blocked_slots_used`) e `success`.",
  metodo: "Proporção de tentativas com repetição configurada; probabilidade de usar a repetição correta em função da fase e do número de tentativas (regressão logística); comparação com pré e pós-teste de pensamento computacional.",
  resultado: "Na primeira tentativa da Fase 2, só 35% configuraram a repetição; na quarta, 78%. Quem passou a usá-la cedo teve maior ganho no item sobre laços do pós-teste.",
}));

add(H3("3.5.2 Conjuntos — classificação e agrupamento"));
add(P("Registrado quando todos os espaços estão preenchidos. Para cada espaço, grava os tipos dos objetos colocados e o tipo esperado, a lista de objetos com sua localização, o número de objetos mal posicionados e as **confusões** no formato `esperado->colocado`."));
add(...analysis(12, "Matriz de confusão entre categorias", {
  pergunta: "Quais categorias as crianças mais confundem?",
  dados: "Campo `confusions` de `solution_snapshot` e eventos `object_place` com o campo `correct`.",
  metodo: "Matriz de confusão categoria esperada × categoria colocada, agregada por fase e por série; teste qui-quadrado para comparar séries.",
  resultado: "A confusão mais frequente foi 'quadrado→retângulo' (27% dos erros), concentrada no 3º ano — sugere uma dificuldade conceitual de geometria, não de classificação em si.",
}));

add(H3("3.5.3 Receitas — algoritmos passo a passo"));
add(P("Registrado ao iniciar o preparo. Para cada passo, grava o insumo e a ação colocados e os esperados, e classifica o erro do passo: `ok`, `empty_step`, `empty_food`, `empty_action`, `wrong_food`, `wrong_action` ou `order` (insumo correto de outro passo, isto é, ordem trocada)."));
add(...analysis(13, "Tipologia de erros algorítmicos", {
  pergunta: "Os erros das crianças são de sequência (ordem) ou de operação (ação errada)?",
  dados: "Campo `step_errors` de cada `solution_snapshot` de Receitas.",
  metodo: "Distribuição dos tipos de erro por fase; transições entre tipos de erro de uma tentativa para a seguinte (cadeia de Markov).",
  resultado: "Na Fase 1 predominam passos vazios (planejamento incompleto); na Fase 3, erros de ordem (58%). Quem comete erro de ordem tende a corrigi-lo na tentativa seguinte (0,71 de probabilidade), mas erros de ação persistem.",
}));

add(H3("3.5.4 Espelhos — raciocínio espacial e depuração"));
add(P("O Espelhos não tem gabarito fixo: a solução é física. Ao disparar a luz, o jogo grava a rotação (arredondada a 15°) e a posição de cada pilar. Quando a luz apaga, `beam_result` informa quantos trechos do feixe acenderam (alcance), quais foram e onde o feixe foi bloqueado (`beam_blocked`). Na vitória, `solution_final` registra a configuração vencedora."));
add(...analysis(14, "Alcance do feixe como medida de progresso", {
  pergunta: "As tentativas se aproximam da solução ao longo do tempo?",
  dados: "`segments_lit` de cada `beam_result`; distância entre a configuração de cada tentativa e a `solution_final` do mesmo participante (pilares diferentes).",
  metodo: "Curva de progresso individual (alcance por tentativa); classificação de trajetórias monotônicas versus oscilantes; relação com a quantidade de rotações entre disparos.",
  resultado: "Quem girava no máximo dois pilares entre disparos aumentou o alcance de forma contínua e concluiu em média com 4 disparos; quem girava muitos pilares de uma vez oscilou e precisou de 9.",
}));

// 3.6
add(
  H2("3.6 Ações intermediárias (processo de resolução)"),
  P("**O que é coletado.** Além das tentativas, o jogo registra as ações que levam até elas: no Conjuntos, pegar, soltar e encaixar objetos (`object_pick`, `object_drop`, `object_place`, com indicação de encaixe correto); no Espelhos, girar e empurrar pilares e usar a lente (`pillar_rotate`, `pillar_push`, `lens_used`); nas Receitas, colocar e retirar itens das mesas (`recipe_place`, `recipe_remove`, com indicação de item correto). No Musical, as mudanças entre tentativas são obtidas pela comparação dos `solution_snapshot`."),
);
add(...analysis(15, "Eficiência e estratégias de exploração", {
  pergunta: "As crianças planejam antes de agir ou exploram por manipulação?",
  dados: "Ações intermediárias e tempo entre a entrada na fase e a primeira ação.",
  metodo: "Indicadores: tempo até a primeira ação (planejamento), número de ações por tentativa, proporção de ações desfeitas (pegar e devolver o mesmo objeto) e de encaixes corretos. Análise de sequências de ações com modelos de Markov oculto (HMM) para identificar fases de exploração e de execução.",
  resultado: "O HMM identificou dois estados: 'exploração' (muitas ações desfeitas) e 'execução' (encaixes corretos em sequência). Crianças que passaram mais cedo para o estado de execução concluíram com menos tentativas.",
}));

// 3.7
add(
  H2("3.7 Captura de expressões faciais (MediaPipe)"),
  P("**O que é coletado.** Quando habilitada, a webcam é processada localmente pelo MediaPipe FaceLandmarker (plugin homuler v0.15.0) a uma taxa configurável (padrão: 2 amostras por segundo). **Nenhuma imagem ou vídeo é gravado**, apenas números: os **52 blendshapes** faciais (0 a 1), índices derivados, a orientação da cabeça e se havia rosto na imagem. Os dados vão para `affect_<sessão>.csv`, e um resumo é enviado ao log de eventos a cada 10 segundos (`affect_summary`)."),
  ...table(["Índice derivado", "Blendshapes", "Leitura possível (heurística)"], [
    ["`smile`", "mouthSmile E/D", "alegria, diversão"],
    ["`frown`", "mouthFrown E/D", "desagrado"],
    ["`brow_down`", "browDown E/D", "esforço, concentração ou confusão"],
    ["`brow_inner_up` / `brow_outer_up`", "browInnerUp / browOuterUp", "preocupação / surpresa"],
    ["`eye_wide` / `eye_squint` / `eye_blink`", "eyeWide / eyeSquint / eyeBlink", "surpresa / esforço / olhos fechados"],
    ["`jaw_open`", "jawOpen", "surpresa, fala, bocejo"],
    ["`mouth_press`", "mouthPress E/D", "tensão, frustração"],
    ["`nose_sneer`", "noseSneer E/D", "desconforto"],
    ["`gaze_away`", "eyeLook*", "olhar fora do centro"],
    ["`face_present`, `head_yaw`, `head_pitch`", "—", "presença diante da tela e direção da cabeça"],
  ], [3100, 2700, 3226]),
  box("Atenção: índices faciais não são emoções", [
    P("Expressão facial não equivale diretamente a estado emocional. Os índices devem ser tratados como **variáveis comportamentais** e **triangulados** com o contexto do jogo, com autorrelato e, idealmente, com observação de pesquisadores (ex.: protocolo BROMP). O modelo do MediaPipe foi treinado principalmente com adultos, o que exige validação com crianças.", { para: { spacing: { after: 0 } } }),
  ], AMBER),
  gap(),
);
add(...analysis(16, "Perfil afetivo por tipo de puzzle", {
  pergunta: "Os puzzles provocam padrões expressivos diferentes?",
  dados: "Médias de `smile`, `brow_down` e `mouth_press` por cena (`affect_*.csv` ou `affect_summary`).",
  metodo: "Modelo linear misto (índice ~ tipo de puzzle + (1 | participante)), usando apenas amostras com `face_present = 1`.",
  resultado: "O Espelhos apresentou `brow_down` médio 40% maior que o Musical, e o Musical teve mais `smile` — compatível com maior esforço cognitivo no primeiro e maior diversão no segundo.",
}));
add(...analysis(17, "Atenção à tela", {
  pergunta: "Quanto tempo as crianças ficam de fato olhando para o jogo?",
  dados: "`face_present`, `head_yaw`, `head_pitch` e `gaze_away`.",
  metodo: "Proporção de amostras com rosto presente e cabeça voltada para a tela (|yaw| < 30°) por fase; relação com ociosidade e abandono.",
  resultado: "A atenção caiu de 92% na Área 1 para 78% na Área 3; nas fases abandonadas, a queda começou em média 90 s antes da saída.",
}));

// 3.8
add(
  H2("3.8 Controle da captura e consentimento"),
  P("**O que é coletado.** Toda mudança de estado da câmera é registrada: início (`affect_capture_start`, com câmera, resolução e taxa), pausa e retomada pela tecla F9 (`affect_capture_paused`/`affect_capture_resumed`), desativação por configuração ou falta de consentimento (`affect_capture_disabled`), falhas técnicas (`affect_unavailable`, ex.: sem câmera) e alterações de consentimento (`affect_consent`). Durante a captura, um indicador visível ('● Câmera ativa (pesquisa)') é exibido no canto da tela. Antes disso, na tela de cadastro, uma **telinha da câmera** no canto superior direito mostra a imagem e se o rosto está sendo detectado, para o pesquisador ajustar a webcam; ela some quando o estudante clica em Iniciar e nada é gravado enquanto está aberta."),
);
add(...analysis(18, "Qualidade e cobertura dos dados faciais", {
  pergunta: "Os dados faciais são suficientes e confiáveis para análise?",
  dados: "Eventos de controle da câmera e `face_ratio` de `affect_summary`.",
  metodo: "Relatório de qualidade por sessão: tempo de captura efetiva, proporção de rosto detectado, pausas pela criança e falhas técnicas por máquina. Definição de critérios de exclusão (ex.: menos de 60% de rosto presente).",
  resultado: "Em 12% das sessões a cobertura ficou abaixo de 60%, concentradas em duas máquinas com webcam mal posicionada — informação usada para ajustar a montagem do laboratório antes da coleta principal.",
}));

// 3.9
add(
  H2("3.9 Integração multimodal pela linha do tempo"),
  P("Como eventos e dados faciais usam o mesmo relógio (`t_ms`), é possível analisar **o que acontecia na expressão facial antes, durante e depois** de qualquer evento do jogo: um erro, uma vitória, uma pausa ou um período ocioso."),
);
add(...analysis(19, "Resposta afetiva ao erro", {
  pergunta: "Como a expressão facial muda depois de uma tentativa errada, em comparação com uma certa?",
  dados: "`solution_snapshot` (`success`, `distance`) e amostras de `affect_*.csv` numa janela de −10 s a +10 s em torno de cada tentativa.",
  metodo: "Análise de janela de eventos (event-related): média de cada índice por segundo relativo à tentativa, separada por sucesso e erro; modelo misto para testar a diferença.",
  resultado: "Após tentativas erradas, `mouth_press` subiu 25% nos 5 s seguintes; quando o erro diminuía a distância até a solução, o aumento foi metade — o progresso parcial parece amortecer a frustração.",
}));
add(...analysis(20, "Detector de confusão sem câmera", {
  pergunta: "É possível detectar confusão apenas pelos logs, para usar em escolas sem webcam?",
  dados: "Rótulos de confusão obtidos por `brow_down` alto + autorrelato ou observação, e indicadores dos logs (tempo entre tentativas, `n_changed`, ociosidade, consulta à missão).",
  metodo: "Treinar um classificador (ex.: random forest) que prevê o rótulo a partir apenas dos logs; validação cruzada por participante.",
  resultado: "O detector baseado só em logs atingiu AUC = 0,72 — suficiente para disparar uma dica ou uma pergunta de autorrelato quando a criança parece confusa.",
}));

// 3.10 a 3.13 — recursos para o pesquisador e qualidade dos dados
add(
  H2("3.10 Identificação do estudo, códigos de participante e condições A/B"),
  P("**O que é coletado.** O evento `session_start` registra o estudo (`study_id`), a escola (`school_id`), a turma (`class_id`), o computador (`machine_id`), a forma de identificação (`id_mode`) e a **condição experimental** (`condition`). No modo **código**, a criança digita um código impresso numa etiqueta (ex.: `K7M2X`) em vez do nome; o código tem dígito verificador, então erros de digitação são recusados na hora, e nenhum nome é gravado no computador. As condições podem ser fixas por máquina, sorteadas a cada sessão ou atribuídas de forma estável por participante."),
);
add(...analysis(21, "Comparação entre condições (A/B)", {
  pergunta: "Uma variação do jogo (ex.: com dica × sem dica) muda o desempenho?",
  dados: "`condition` de `session_start`, `puzzle_complete` e `solution_snapshot`.",
  metodo: "Modelo misto (tentativas ~ condição + puzzle + (1 | participante) + (1 | escola/turma)); a atribuição estável por participante evita que a mesma criança passe pelas duas condições.",
  resultado: "Na condição com dica, as crianças concluíram 9% mais fases e fizeram 14% menos tentativas no Musical Fase 2; nas demais fases não houve diferença — a dica ajuda justamente no conceito de laço.",
}));
add(...analysis(22, "Efeitos de escola e turma", {
  pergunta: "Quanto da variação no desempenho se deve à escola e à turma?",
  dados: "`school_id`, `class_id` e `puzzle_complete`.",
  metodo: "Modelo multinível com efeitos aleatórios de escola e turma; coeficiente de correlação intraclasse (ICC).",
  resultado: "A escola explicou 11% da variação nas tentativas e a turma mais 6% (ICC) — valores que justificam incluir esses níveis em qualquer modelo de efeito do jogo.",
}));
add(
  H2("3.11 Consentimento na tela do jogo"),
  P("**O que é coletado.** Com `requireConsent` ligado, a tela de consentimento (texto editável em `StreamingAssets/consent_text.txt`) aparece após o cadastro, com autorização separada para a câmera. Só com o \"concordo\" a sessão é iniciada; quem não concorda joga normalmente, sem nenhum registro. O evento `session_start` guarda `consent` e `camera_consent`."),
);
add(...analysis(23, "Viés de seleção da câmera", {
  pergunta: "As crianças que autorizam a câmera diferem das que não autorizam?",
  dados: "`camera_consent` de `session_start` e indicadores de desempenho dos logs.",
  metodo: "Comparação de série, gênero e desempenho entre os dois grupos (testes qui-quadrado e Mann-Whitney); análises faciais reportadas com essa ressalva.",
  resultado: "82% autorizaram a câmera; os dois grupos tiveram desempenho semelhante, o que reduz a preocupação de que as análises faciais representem só um subgrupo.",
}));
add(
  H2("3.12 Qualidade e integridade dos dados"),
  P("**O que é coletado.** Todo evento tem `id` único e `seq` (sequência na sessão). Se o jogo fechar sem encerrar a sessão (queda de energia, travamento), a próxima abertura acrescenta um `session_end` com `reason = \"interrupted\"` e `recovered = true`. A exportação gera um `manifest.json` com o SHA-256 de cada arquivo. A função `validar_coleta()` do pacote R confere tudo isso automaticamente: duplicatas, lacunas, sessões sem início ou fim, eventos fora do catálogo, integridade e cobertura da câmera."),
);
add(...analysis(24, "Análise de sensibilidade às sessões interrompidas", {
  pergunta: "Os resultados mudam quando se excluem as sessões interrompidas?",
  dados: "`session_end` (`reason`, `recovered`) e os desfechos de interesse.",
  metodo: "Rodar o modelo principal com e sem as sessões interrompidas e comparar as estimativas; reportar a taxa de interrupção por escola e máquina.",
  resultado: "5% das sessões foram interrompidas, todas numa mesma escola com quedas de energia; os efeitos estimados mudaram menos de 2%, indicando resultados robustos.",
}));
add(
  H2("3.13 Ferramentas do pesquisador"),
  B("**Painel do Estudo (Unity → Fabulas → Learning Analytics → Painel do Estudo):** configura o estudo, as condições, a câmera e o PIN; aponta riscos (câmera sem consentimento, PIN padrão, mapa de nomes na exportação); gera códigos de participante com **etiquetas para imprimir**."),
  B("**Painel do Pesquisador (no jogo, Ctrl+Shift+R + PIN):** mostra estudo, sessão, eventos gravados, pasta e espaço em disco; **testa a câmera** com prévia e indicação de rosto detectado, sem gravar nada; **exporta a coleta** com um clique."),
  B("**Exportação:** um .zip por máquina, anonimizado por padrão (sem o mapa de nomes), com manifesto e checksums."),
  B("**Pacote R `fabulasLA`:** `ler_coleta()`, `ler_coleta_servidor()`, `status_servidor()`, `validar_coleta()`, `tabelas_analise()`, `afeto_por_evento()`, `estados_faciais()`, `codebook()`, `gerar_relatorio()` (estudo em grupo), `gerar_relatorio_individual()` (um estudante), `copiar_modelos_relatorio()`, `simular_coleta()`, `gerar_codigos()` e `codigo_valido()`."),
  B("**Testes automatizados:** 43 testes no Unity, testes do pacote R, testes do banco de dados (`server/supabase/teste_schema.sql`) e um autoteste de ponta a ponta que joga uma sessão completa, simula uma queda sem internet e um código inválido, envia tudo a um servidor e confere que o download é idêntico aos arquivos locais."),
);
add(
  H2("3.14 Envio ao servidor central (Supabase)"),
  P("**O que é feito.** Com o envio ligado, o jogo manda ao servidor, em segundo plano, **todos os eventos** e **todas as amostras faciais** (incluindo os 52 blendshapes), em lotes de 500 eventos ou 300 amostras, a cada 30 segundos e ao fim de cada sessão. O servidor confirma até onde recebeu cada sessão; o progresso fica em `.sync_state.tsv`. Sem internet, o jogo segue normalmente e tenta de novo com espera crescente (até 5 minutos); o que faltar é enviado na próxima vez que o jogo abrir — inclusive sessões interrompidas por queda de energia."),
  B("**Backup local garantido:** os arquivos `events_*.jsonl` e `affect_*.csv` continuam sendo gravados e **nunca** são apagados pelo envio. A exportação .zip continua disponível."),
  B("**Sem duplicatas:** o servidor ignora eventos repetidos (pelo `id`) e amostras repetidas (sessão + número da amostra); reenviar é sempre seguro."),
  B("**Segurança:** o jogo só tem a **chave de envio** do estudo, que permite gravar dados novos, mas não ler nem apagar. A leitura usa uma **chave de leitura** separada, que fica só com a equipe de pesquisa. O nome da criança e o `participants_map.csv` nunca são enviados."),
  B("**Acompanhamento:** o Painel do Pesquisador mostra o estado do envio (em dia, sem conexão, quantos itens aguardam) e tem o botão **Sincronizar agora**. No R, `status_servidor()` lista as sessões recebidas de todas as escolas."),
);
add(...analysis(25, "Monitoramento da coleta em várias escolas", {
  pergunta: "A coleta em andamento está completa e equilibrada entre escolas e condições, a ponto de permitir a análise interina prevista no protocolo?",
  dados: "`status_servidor()` (sessões, eventos, amostras e fim de cada sessão por escola) e `ler_coleta_servidor()` com `validar_coleta()`.",
  metodo: "Acompanhamento semanal: número de participantes por escola e condição, taxa de sessões interrompidas, cobertura da câmera e lacunas de sequência; comparação com a meta amostral do projeto.",
  resultado: "Na terceira semana, 214 participantes em 6 escolas; uma escola aparecia sem sessões novas há 4 dias — o painel da máquina indicava “sem conexão”, e os dados locais foram enviados quando a rede voltou, sem perda.",
}));
add(
  H2("3.15 Estados afetivos de aprendizagem (estimados na análise)"),
  P("**O que é feito.** O jogo não grava emoções: grava sinais (rosto e comportamento). Na análise, a função `estados_faciais()` do pacote R estima, para cada janela de 10 segundos, um escore de 0 a 1 para os cinco estados centrais da aprendizagem (D'Mello & Graesser, 2012) e o estado predominante (ou “neutro”):"),
  ...table(["Estado", "Sinais do rosto (escore z em relação à própria criança)", "Sinais do comportamento"], [
    ["Confusão", "sobrancelha abaixada, pálpebras tensas, lábios pressionados", "longe da solução, tentativas sem progresso, erros, consulta à missão"],
    ["Frustração", "lábios pressionados, cantos da boca para baixo, nariz franzido", "erros repetidos sem progresso, reiniciar a fase"],
    ["Tédio", "olhar desviado, cabeça virada, bocejo", "ociosidade, sair da janela do jogo, poucas ações"],
    ["Engajamento", "rosto voltado para a tela", "ações contínuas, tentativas"],
    ["Encantamento", "sorriso, olhos arregalados", "acerto, joia, fase concluída"],
  ], [1700, 3700, 3626]),
  B("**Por que na análise e não no jogo:** as regras serão ajustadas e validadas; calculando na análise, todo o histórico é recalculado com a mesma versão, o banco guarda só o que foi medido e nenhum rótulo interpretativo sobre a criança fica armazenado. As regras têm versão (`regras_estados()`), registrada em cada resultado."),
  B("**Validade:** são indicadores heurísticos, não diagnósticos. A forma recomendada de validar é a observação em sala (BROMP): o relatório compara as regras com as observações e com um detector treinado nelas."),
  B("**Nos relatórios:** o relatório individual mostra o tempo em cada estado (comparado com o grupo), a sequência ao longo do jogo, os estados por fase e as transições; o de grupo mostra estados por puzzle e por condição, a dinâmica das transições e a concordância com a observação."),
);
add(...analysis(26, "Confusão produtiva ou improdutiva", {
  pergunta: "Quando as crianças ficam confusas no jogo, a confusão é resolvida (volta ao engajamento) ou evolui para frustração e tédio?",
  dados: "`estados_faciais()` (estado predominante a cada 10 s) por sessão.",
  metodo: "Probabilidade de transição L de D'Mello e Graesser entre estados consecutivos não neutros; comparação entre puzzles e entre condições A/B.",
  resultado: "Da confusão, L = 0,44 para engajamento e L ≈ 0 para frustração: predomina a confusão produtiva. No Receitas, porém, a confusão durou mais e precedeu mais abandonos — indicando onde uma dica ajudaria.",
}));

// 4. Configuração
add(
  BR(),
  H1("4. Configuração e operação da coleta"),
  P("A coleta é controlada pelo arquivo `research_config.json`. Na build, ele fica em `FabulasComputacionais_Data/StreamingAssets/` e pode ser editado em cada máquina sem recompilar o jogo."),
  ...table(["Campo", "Padrão", "Descrição"], [
    ["`studyId` / `schoolId` / `classId`", "(vazio)", "Identificação do estudo, da escola e da turma (vão para cada sessão)."],
    ["`machineId`", "(vazio)", "Identificação do computador; vazio = nome do computador."],
    ["`conditions` / `conditionAssignment` / `condition`", "[] / fixed / (vazio)", "Condições A/B e como atribuí-las: fixed, random ou by_participant."],
    ["`participantIdMode`", "name", "name (nome no cadastro + mapa separado) ou code (código de estudo, sem nomes)."],
    ["`requireConsent` / `consentTextFile`", "false / consent_text.txt", "Exibe a tela de consentimento antes de iniciar a coleta."],
    ["`researcherPin`", "1234", "PIN do Painel do Pesquisador (troque antes da coleta)."],
    ["`exportFolder` / `exportIncludesNameMap`", "(vazio) / false", "Pasta da exportação e se o mapa de nomes vai junto."],
    ["`telemetryEnabled`", "true", "Liga o log de eventos."],
    ["`idleThresholdSeconds`", "30", "Segundos sem input para registrar ociosidade."],
    ["`affectCaptureEnabled`", "false", "Liga a câmera. Só ative com consentimento assinado."],
    ["`affectSampleRateHz`", "2", "Amostras faciais por segundo."],
    ["`affectSummaryIntervalSeconds`", "10", "Intervalo do resumo afetivo no log."],
    ["`cameraDeviceName`", "(vazio)", "Webcam a usar; vazio = a primeira encontrada."],
    ["`cameraWidth` / `cameraHeight` / `cameraFps`", "640 / 480 / 15", "Resolução pedida à webcam."],
    ["`saveRawBlendshapes`", "true", "Grava os 52 blendshapes brutos."],
    ["`showCameraIndicator`", "true", "Exibe o aviso de câmera ativa."],
    ["`showCameraPreviewOnSignup`", "true", "Mostra uma telinha da câmera na tela de cadastro, para o pesquisador conferir a posição; some quando o estudante inicia."],
  ], [3300, 1500, 4226]),
  P("O envio ao servidor central fica num arquivo separado, `server_sync.json` (mesma pasta), que vem no repositório já configurado para o projeto Supabase do Fábulas (estudo `FABULAS`). A chave de envio é pública de propósito: quem baixar e rodar o jogo, mesmo em testes, grava no servidor — mas ninguém consegue ler ou apagar dados com ela. Para separar testes da coleta real, use outro `studyId` com chaves próprias:", { para: { keepNext: true } }),
  ...table(["Campo", "Padrão", "Descrição"], [
    ["`enabled`", "false", "Liga o envio. Desligado, os dados ficam só no computador."],
    ["`serverUrl`", "(vazio)", "URL do projeto Supabase (`https://<projeto>.supabase.co`)."],
    ["`anonKey`", "(vazio)", "Chave **pública** do projeto (anon/publishable). Nunca a chave secreta (service_role)."],
    ["`studyId`", "(vazio)", "Estudo no servidor; vazio = o `studyId` do `research_config.json`."],
    ["`uploadKey`", "(vazio)", "Chave de envio do estudo, gerada pelo Painel do Estudo."],
    ["`syncIntervalSeconds`", "30", "Intervalo entre envios (mínimo 5 s)."],
  ], [3300, 1500, 4226]),
  P("A forma recomendada de editar é o **Painel do Estudo** no Unity, que valida os campos. Para testes, o jogo aceita `-researchConfig <arquivo>` (outra configuração) e `-analyticsDataFolder <pasta>` (outra pasta de dados) na linha de comando."),
  P("**Ferramentas no editor do Unity** (menu Fabulas → Learning Analytics): Painel do Estudo, configurar o MediaPipe, abrir a pasta de dados, teste rápido do MediaPipe sem câmera e teste rápido da telemetria."),
);

// 5. Ética
add(
  H1("5. Ética, privacidade e LGPD"),
  B("**Pseudonimização:** o nome da criança aparece somente em `participants_map.csv`; todos os demais arquivos usam o código do participante."),
  B("**Dados faciais:** são processados localmente e nenhuma imagem é gravada. Ainda assim, os blendshapes são dados biométricos derivados e, por envolverem crianças, são dados pessoais sensíveis (LGPD, arts. 11 e 14)."),
  B("**Requisitos:** aprovação no Comitê de Ética em Pesquisa (CEP), TCLE assinado pelos responsáveis, TALE para a criança, finalidade explícita, armazenamento seguro e prazo de descarte definidos."),
  B("**Transparência e controle:** a câmera só liga quando `affectCaptureEnabled` está ativo na configuração (na versão atual do repositório está ligado: a câmera começa junto com a sessão, se o computador tiver webcam); há indicador visível durante a captura e a criança ou o pesquisador pode pausá-la com F9. Desligue em máquinas ou turmas sem consentimento para a câmera."),
  B("**Códigos de estudo:** no modo `code`, nenhum nome entra no computador; a lista nome ↔ código fica em papel com o pesquisador."),
  B("**Consentimento registrado:** a tela de consentimento no jogo e os campos `consent`/`camera_consent` de cada sessão documentam a concordância."),
  B("**Exportação anonimizada:** por padrão o .zip não leva o mapa de nomes; o Painel do Estudo alerta quando isso é alterado."),
  B("**Minimização:** coletar apenas o que estiver associado a perguntas de pesquisa definidas no projeto."),
  B("**Servidor central:** com o envio ligado, eventos e dados faciais (pseudonimizados) passam a ficar também no Supabase. Recomenda-se criar o projeto na região **São Paulo**, citar o armazenamento em servidor no TCLE e no protocolo do CEP, restringir o acesso ao painel do Supabase à equipe e guardar a chave de leitura e a senha do banco fora do repositório. As tabelas têm acesso direto bloqueado (RLS); só as funções com chave do estudo leem ou gravam."),
);

// 6. Limitações
add(
  H1("6. Limitações conhecidas"),
  B("Os índices faciais são heurísticos e não medidas validadas de emoção; precisam de triangulação e de validação com crianças."),
  B("A captura facial ainda não foi testada com uma pessoa diante da câmera no ambiente final (a máquina de desenvolvimento não tem webcam); o carregamento do modelo e o processamento foram verificados (cerca de 2,6 ms por quadro sem rosto)."),
  B("No Espelhos não há gabarito fixo: a distância até a solução precisa ser calculada na análise, a partir da configuração vencedora."),
  B("No Musical com repetição, o gabarito posição a posição é o configurado na cena; o veredito oficial está nos campos `part*_repeat`."),
  B("O envio ao servidor depende de internet na escola; sem ela, os dados ficam no computador até a próxima conexão (ou seguem pelo .zip). O plano gratuito do Supabase tem limite de 500 MB de banco: com câmera a 2 amostras/s, cada hora de jogo gera cerca de 7.200 amostras faciais (alguns MB) — para estudos grandes, planeje o plano pago ou reduza `affectSampleRateHz`."),
  B("As condições A/B são registradas, mas o jogo ainda não tem variações implementadas: cada nova variação (ex.: dica) precisa consultar `TelemetryManager.Instance.Condition`."),
  B("O arquivo antigo `<NOME> GameData.csv`, gerado pelo `DataManager` original, continua existindo e contém o nome da criança; recomenda-se desativá-lo ou pseudonimizá-lo."),
);

// 7. Roteiro de análise
add(
  H1("7. Roteiro sugerido de análise"),
  N("**Consolidação:** reunir as pastas `LearningAnalytics` de todas as máquinas e guardar `participants_map.csv` em local separado e restrito."),
  N("**Leitura e limpeza:** carregar todos os `events_*.jsonl` e `affect_*.csv`; remover sessões de teste e aplicar critérios de qualidade (ex.: cobertura facial mínima)."),
  N("**Construção de indicadores:** por participante e fase — tentativas, tempo, perfis de depuração, tipos de erro, ociosidade, atenção e índices faciais."),
  N("**Análises descritivas e exploratórias:** curvas de aprendizagem, mapas de equívocos, mapas de processo."),
  N("**Modelagem:** modelos mistos (alunos aninhados em turmas e escolas), análise de sobrevivência, agrupamentos de perfis e análise de janelas de eventos para a integração multimodal."),
  N("**Triangulação:** cruzar com pré e pós-teste de pensamento computacional, questionários e observação."),
  H2("7.1 Com o pacote R fabulasLA"),
  P("Instalação (uma vez): `install.packages(\"analysis/fabulasLA\", repos = NULL, type = \"source\")`. Depois:", { para: { keepNext: true } }),
  ...code([
    "library(fabulasLA)",
    "",
    "coleta <- ler_coleta(c(\"escola_A.zip\", \"escola_B.zip\"))  # .zip do Painel do Pesquisador",
    "validar_coleta(coleta)            # integridade, duplicatas, lacunas, interrupções",
    "tb <- tabelas_analise(coleta)     # sessoes, fases, tentativas, afeto_sessao",
    "",
    "# Distância até a solução por tentativa, em cada fase",
    "tb$tentativas |> dplyr::group_by(scene, attempt) |> dplyr::summarise(dist = mean(dist_rel))",
    "",
    "gerar_relatorio(c(\"escola_A.zip\", \"escola_B.zip\"), \"relatorio.html\")   # estudo em grupo",
    "gerar_relatorio_individual(\"K7M2X\", c(\"escola_A.zip\", \"escola_B.zip\"))   # um estudante",
    "afeto_por_evento(coleta, \"solution_snapshot\", antes_ms = 2000, depois_ms = 5000)  # rosto em cada tentativa",
    "exportar_codebook(\"codebook.csv\")  # dicionário de variáveis para o artigo",
    "",
    "# Com o servidor central: baixa tudo de todas as escolas (chave de LEITURA no .Renviron)",
    "coleta <- ler_coleta_servidor(\"https://<projeto>.supabase.co\", \"<chave pública>\", \"PILOTO26\")",
    "status_servidor(\"https://<projeto>.supabase.co\", \"<chave pública>\", \"PILOTO26\")",
  ]),
  P("Os arquivos também podem ser lidos em Python ou em outras ferramentas: são JSON Lines e CSV comuns, descritos no Apêndice A."),
);

// Apêndice A — gerado do catálogo (catalogo_eventos.json)
const catalogo = JSON.parse(fs.readFileSync(path.join(__dirname, "..", "catalogo_eventos.json"), "utf8"));
const nomesCategoria = { sessao: "Sessão", navegacao: "Navegação", desempenho: "Desempenho", estado_solucao: "Estado da solução",
  acoes: "Ações", engajamento: "Engajamento", afeto: "Afeto", pesquisa: "Pesquisa" };
add(
  BR(),
  H1("Apêndice A — Dicionário de eventos"),
  P(`Gerado automaticamente do catálogo de eventos (esquema ${catalogo.schema_version}). Todo evento tem também os campos comuns: ` +
    catalogo.common_fields.map((f) => "`" + f.name + "`").join(", ") + ". O codebook completo, com a descrição de cada campo, é obtido com `fabulasLA::exportar_codebook()`."),
  ...table(["Evento", "Categoria", "Quando ocorre", "Campos em data"],
    catalogo.events.map((e) => ["`" + e.name + "`", nomesCategoria[e.category] || e.category, e.when,
      e.fields.length ? e.fields.map((f) => f.name).join(", ") : "—"]),
    [2500, 1300, 2600, 2626]),
);

// Apêndice B — colunas do arquivo facial (do catálogo)
add(
  BR(),
  H1("Apêndice B — Colunas do arquivo facial"),
  ...table(["Coluna", "Tipo", "Descrição"], catalogo.affect_columns.map((c) => ["`" + c.name + "`", c.type, c.description]), [2500, 1300, 5226]),
);

// ---------------------------------------------------------------- documento
const doc = new Document({
  creator: "Rodrigo Lins Rodrigues",
  title: "Fábulas Computacionais — Learning Analytics",
  description: "Funcionalidades de learning analytics implementadas e exemplos de análises de pesquisa",
  styles: {
    default: { document: { run: { font: FONT, size: 22 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { font: FONT, size: 32, bold: true, color: BLUE },
        paragraph: { spacing: { before: 360, after: 160 }, keepNext: true, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { font: FONT, size: 26, bold: true, color: BLUE },
        paragraph: { spacing: { before: 280, after: 120 }, keepNext: true, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { font: FONT, size: 23, bold: true, color: "2E75B6" },
        paragraph: { spacing: { before: 200, after: 80 }, keepNext: true, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [
      { reference: "bullets", levels: [
        { level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
        { level: 1, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 1440, hanging: 360 } } } },
      ] },
      { reference: "numbers", levels: [
        { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
      ] },
    ],
  },

  sections: [{
    properties: { page: { margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } }, titlePage: true },
    headers: {
      default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT, children: [new TextRun({ text: "Fábulas Computacionais — Learning Analytics", size: 16, color: "808080" })] })] }),
    },
    footers: {
      default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ children: ["Página ", PageNumber.CURRENT], size: 16, color: "808080" })] })] }),
    },
    children,
  }],
});

// Uso: node gerar_documento.js [saida.docx]
// Padrão: Assets/LearningAnalytics/Learning_Analytics_Fabulas_Computacionais.docx
const out = process.argv[2] || path.join(__dirname, "..", "Learning_Analytics_Fabulas_Computacionais.docx");
Packer.toBuffer(doc).then((buf) => { fs.writeFileSync(out, buf); console.log("ok", out, buf.length); });

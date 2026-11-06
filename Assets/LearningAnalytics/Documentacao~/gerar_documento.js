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
  new Paragraph({ children: runs("Versão do código: branch `feature/learning-analytics-mediapipe` (commits `9d1cd6d` e `7210a17`)", { size: 20, color: "595959" }), alignment: AlignmentType.CENTER }),
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
  P("A coleta é feita localmente, no computador onde o jogo roda, sem necessidade de internet. Os componentes são criados automaticamente quando o jogo abre — não é preciso alterar nenhuma cena."),
  H2("2.1 Componentes"),
  ...table(["Componente", "Função"], [
    ["`TelemetryManager`", "Gerencia a sessão, gera o código pseudônimo do participante e grava todos os eventos do jogo (JSON Lines). Também detecta ociosidade e perda de foco da janela."],
    ["`SolutionTracker`", "Registra o estado da solução a cada tentativa nos quatro puzzles, comparando com o gabarito e com a tentativa anterior."],
    ["`MirrorsTracker`", "Lógica específica do puzzle Espelhos: configuração dos pilares, alcance do feixe de luz e configuração vencedora."],
    ["`AffectManager`", "Captura de expressões faciais pela webcam com MediaPipe FaceLandmarker (52 blendshapes, índices derivados e orientação da cabeça)."],
    ["`ResearchConfig`", "Lê o arquivo `research_config.json`, que liga ou desliga a coleta em cada máquina sem recompilar o jogo."],
    ["Ganchos nos scripts do jogo", "Linhas de registro adicionadas em `SignupManager`, `LevelDataManager`, `GameManager`, `LoadNewArea`, `checarPartitura`, `LeituraEspacos`, `SnapNovo`, `PickUp`, `EmitLight`, `StopScale`, `showCanvas`, `RotateObj`, `Launch`, `Lente`, `LerMesa` e `Snap`."],
  ], [2400, 6626]),
  H2("2.2 Arquivos gerados"),
  P("Pasta: `%USERPROFILE%\\AppData\\LocalLow\\DefaultCompany\\Fabulas Computacionais\\LearningAnalytics\\`. Cada sessão (do cadastro até a volta ao menu ou fechamento do jogo) gera um conjunto de arquivos:"),
  ...table(["Arquivo", "Formato", "Conteúdo"], [
    ["`events_<sessão>.jsonl`", "JSON Lines", "Um evento por linha: navegação, fases, tentativas, estado da solução, ações, engajamento e resumos afetivos."],
    ["`affect_<sessão>.csv`", "CSV (vírgula)", "Uma linha por amostra da webcam (padrão: 2 por segundo), com 52 blendshapes, índices derivados e pose da cabeça."],
    ["`participants_map.csv`", "CSV (ponto e vírgula)", "Tabela que liga o nome da criança ao código pseudônimo. É o único arquivo com nome e deve ser guardado separadamente."],
  ], [2700, 1700, 4626]),
  H2("2.3 Estrutura de um evento"),
  P("Todos os eventos compartilham os mesmos campos de identificação e de tempo, o que permite cruzá-los entre si e com os dados faciais:"),
  ...code([
    '{"t":"2026-11-05T14:03:22.481Z", "t_ms":81234.5,',
    ' "session":"20261105T140201Z-a1b2c3", "participant":"P-3F9A0C12",',
    ' "scene":"Musical 2", "event":"puzzle_attempt",',
    ' "data":{"level_type":"Musical","area":2,"attempt":3,"time_s":74.2}}',
  ]),
  ...table(["Campo", "Significado"], [
    ["`t`", "Data e hora UTC (ISO 8601)."],
    ["`t_ms`", "Milissegundos desde o início da sessão (relógio monotônico). É o mesmo relógio do arquivo facial, usado para sincronização."],
    ["`session` / `participant`", "Identificador da sessão e código pseudônimo do participante."],
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
add(P("Registrado ao iniciar o preparo. Para cada passo, grava o insumo e a ação colocados e os esperados, e classifica o erro do passo: `ok`, `empty_step`, `empty_food`, `empty_action`, `wrong_food`, `wrong_action` ou `order` (insumo correto de outro passo, isto é, ordem trocada). Observação: as cenas de Receitas estão fora da build atual, mas o código está pronto para quando forem incluídas."));
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
  P("**O que é coletado.** Toda mudança de estado da câmera é registrada: início (`affect_capture_start`, com câmera, resolução e taxa), pausa e retomada pela tecla F9 (`affect_capture_paused`/`affect_capture_resumed`), desativação por configuração ou falta de consentimento (`affect_capture_disabled`), falhas técnicas (`affect_unavailable`, ex.: sem câmera) e alterações de consentimento (`affect_consent`). Durante a captura, um indicador visível ('● Câmera ativa (pesquisa)') é exibido no canto da tela."),
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

// 4. Configuração
add(
  BR(),
  H1("4. Configuração e operação da coleta"),
  P("A coleta é controlada pelo arquivo `research_config.json`. Na build, ele fica em `FabulasComputacionais_Data/StreamingAssets/` e pode ser editado em cada máquina sem recompilar o jogo."),
  ...table(["Campo", "Padrão", "Descrição"], [
    ["`telemetryEnabled`", "true", "Liga o log de eventos."],
    ["`idleThresholdSeconds`", "30", "Segundos sem input para registrar ociosidade."],
    ["`affectCaptureEnabled`", "false", "Liga a câmera. Só ative com consentimento assinado."],
    ["`affectSampleRateHz`", "2", "Amostras faciais por segundo."],
    ["`affectSummaryIntervalSeconds`", "10", "Intervalo do resumo afetivo no log."],
    ["`cameraDeviceName`", "(vazio)", "Webcam a usar; vazio = a primeira encontrada."],
    ["`cameraWidth` / `cameraHeight` / `cameraFps`", "640 / 480 / 15", "Resolução pedida à webcam."],
    ["`saveRawBlendshapes`", "true", "Grava os 52 blendshapes brutos."],
    ["`showCameraIndicator`", "true", "Exibe o aviso de câmera ativa."],
  ], [3300, 1500, 4226]),
  P("**Ferramentas de verificação no editor do Unity** (menu Fabulas → Learning Analytics): configurar o MediaPipe, abrir a pasta de dados coletados, teste rápido do MediaPipe sem câmera e teste rápido da telemetria (grava duas tentativas simuladas, confere o JSON e apaga os arquivos de teste)."),
  P("**Consentimento no jogo (opcional):** um Toggle no painel de cadastro pode ser ligado a `SignupManager.SetCameraConsent`, permitindo que a captura só ocorra com a concordância registrada."),
);

// 5. Ética
add(
  H1("5. Ética, privacidade e LGPD"),
  B("**Pseudonimização:** o nome da criança aparece somente em `participants_map.csv`; todos os demais arquivos usam o código do participante."),
  B("**Dados faciais:** são processados localmente e nenhuma imagem é gravada. Ainda assim, os blendshapes são dados biométricos derivados e, por envolverem crianças, são dados pessoais sensíveis (LGPD, arts. 11 e 14)."),
  B("**Requisitos:** aprovação no Comitê de Ética em Pesquisa (CEP), TCLE assinado pelos responsáveis, TALE para a criança, finalidade explícita, armazenamento seguro e prazo de descarte definidos."),
  B("**Transparência e controle:** a câmera vem desligada por padrão, há indicador visível durante a captura e a criança ou o pesquisador pode pausá-la com F9."),
  B("**Minimização:** coletar apenas o que estiver associado a perguntas de pesquisa definidas no projeto."),
);

// 6. Limitações
add(
  H1("6. Limitações conhecidas"),
  B("Os índices faciais são heurísticos e não medidas validadas de emoção; precisam de triangulação e de validação com crianças."),
  B("A captura facial ainda não foi testada com uma pessoa diante da câmera no ambiente final (a máquina de desenvolvimento não tem webcam); o carregamento do modelo e o processamento foram verificados (cerca de 2,6 ms por quadro sem rosto)."),
  B("No Espelhos não há gabarito fixo: a distância até a solução precisa ser calculada na análise, a partir da configuração vencedora."),
  B("No Musical com repetição, o gabarito posição a posição é o configurado na cena; o veredito oficial está nos campos `part*_repeat`."),
  B("As cenas de Receitas estão fora da build atual; o código de coleta está pronto para quando forem incluídas."),
  B("Os dados ficam em arquivos locais em cada máquina; a consolidação de várias máquinas exige copiar as pastas (ou implementar envio a um servidor)."),
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
  H2("7.1 Exemplo de código (Python)"),
  P("O trecho abaixo carrega os dados de todas as sessões, calcula a evolução da distância até a solução e mede a expressão facial nos 10 segundos seguintes a cada tentativa:", { para: { keepNext: true } }),
  ...code([
    "import glob, json, pandas as pd",
    "",
    "pasta = r\"C:\\dados\\LearningAnalytics\"",
    "events = pd.DataFrame(json.loads(l)",
    "    for f in glob.glob(pasta + r\"\\events_*.jsonl\")",
    "    for l in open(f, encoding=\"utf-8\"))",
    "affect = pd.concat(pd.read_csv(f) for f in glob.glob(pasta + r\"\\affect_*.csv\"))",
    "",
    "# 1) Estado da solução: distância e posições alteradas por tentativa",
    "snap = events[events.event == \"solution_snapshot\"].copy()",
    "snap = snap.join(pd.json_normalize(snap.pop(\"data\")).set_index(snap.index))",
    "print(snap.groupby([\"scene\", \"attempt\"])[\"distance\"].mean())",
    "",
    "# 2) Expressão facial nos 10 s após cada tentativa",
    "def janela(e, ini=0, fim=10_000):",
    "    a = affect[(affect.session == e.session) & (affect.face_present == 1)",
    "               & affect.t_ms.between(e.t_ms + ini, e.t_ms + fim)]",
    "    return a[[\"brow_down\", \"mouth_press\", \"smile\"]].mean()",
    "",
    "pos = snap.apply(janela, axis=1)",
    "print(pd.concat([snap[[\"success\"]], pos], axis=1).groupby(\"success\").mean())",
  ]),
);

// Apêndice A
add(
  BR(),
  H1("Apêndice A — Dicionário de eventos"),
  ...table(["Evento", "Quando ocorre", "Principais campos"], [
    ["`session_start` / `session_end`", "Cadastro concluído / volta ao menu ou saída", "idade, gênero, série, versão, SO, tela, controles / motivo, duração"],
    ["`scene_enter` / `scene_exit`", "Troca de cena", "nome da cena, tempo na cena"],
    ["`door_approach` / `door_leave` / `door_enter`", "Aproximar-se, afastar-se ou entrar numa porta", "porta"],
    ["`puzzle_start`", "Entrada numa fase", "tipo, área, tentativas e tempo anteriores, se já venceu"],
    ["`puzzle_attempt`", "Cada teste de solução", "número da tentativa, tempo"],
    ["`puzzle_complete`", "Vitória na fase", "tentativas, tempo"],
    ["`jewel_earned`", "Nova joia", "fase, total de joias, tempo de jogo"],
    ["`solution_snapshot`", "Cada teste de solução", "slots, expected, posições certas, distância, primeiro erro, posições alteradas, sucesso, campos do puzzle"],
    ["`object_pick` / `object_drop` / `object_place`", "Conjuntos: pegar, soltar, encaixar", "objeto, tipo, espaço, correto"],
    ["`pillar_rotate` / `pillar_push` / `lens_used`", "Espelhos: girar, empurrar, usar lente", "pilar, rotação ou posição anterior, direção"],
    ["`beam_blocked` / `beam_result` / `solution_final`", "Espelhos: bloqueio, fim do disparo, vitória", "onde bateu, trechos acesos, configuração vencedora"],
    ["`recipe_place` / `recipe_remove`", "Receitas: colocar/retirar item", "mesa, item, esperado, correto"],
    ["`pause_open` / `pause_close`", "Menu de pausa", "—"],
    ["`mission_panel_toggle`", "Consulta à missão (Y)", "—"],
    ["`scene_restart`", "Reinício de cena", "—"],
    ["`idle_start` / `idle_end`", "Sem input por N s", "limite, duração"],
    ["`app_focus` / `app_blur`", "Janela ganha/perde foco", "—"],
    ["`affect_capture_start` / `_stop` / `_paused` / `_resumed`", "Estado da câmera", "câmera, resolução, taxa, motivo"],
    ["`affect_capture_disabled` / `affect_unavailable`", "Câmera desligada ou com falha", "motivo, detalhe"],
    ["`affect_consent`", "Mudança de consentimento", "valor"],
    ["`affect_summary`", "A cada 10 s de captura", "amostras, proporção de rosto, médias dos índices"],
  ], [3300, 2600, 3126]),
);

// Apêndice B
add(
  BR(),
  H1("Apêndice B — Colunas do arquivo facial"),
  ...table(["Coluna(s)", "Descrição"], [
    ["`utc`, `t_ms`", "Data e hora UTC e tempo desde o início da sessão (mesmo relógio dos eventos)."],
    ["`session`, `participant`, `scene`", "Identificação da sessão, do participante e da cena."],
    ["`face_present`", "1 se havia rosto na imagem, 0 caso contrário."],
    ["`head_yaw`, `head_pitch`, `head_roll`", "Orientação aproximada da cabeça, em graus."],
    ["`smile` … `gaze_away`", "12 índices derivados (seção 3.7)."],
    ["`bs_neutral` … `bs_noseSneerRight`", "52 blendshapes brutos do MediaPipe (0 a 1; vazio quando não há rosto)."],
  ], [3300, 5726]),
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

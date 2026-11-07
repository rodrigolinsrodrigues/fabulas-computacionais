// Gera o Guia do Pesquisador (Assets/LearningAnalytics/Guia_do_Pesquisador.docx).
// Uso: node gerar_guia.js [saida.docx]
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
// Cada título reinicia a numeração das listas (instância nova).
let numInst = 1;
const H1 = (t) => (numInst++, new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(t)] }));
const H2 = (t) => (numInst++, new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(t)] }));
const H3 = (t) => new Paragraph({ heading: HeadingLevel.HEADING_3, children: [new TextRun(t)] });
const B = (text, level = 0) => new Paragraph({ numbering: { reference: "bullets", level }, children: runs(text), spacing: { after: 60, line: 264 } });
const N = (text, ref = "numbers") => new Paragraph({ numbering: { reference: ref, level: 0, instance: numInst }, children: runs(text), spacing: { after: 60, line: 264 } });
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
const check = (text) => new Paragraph({ children: [new TextRun({ text: "☐  ", size: 24 }), ...runs(text)], spacing: { after: 70, line: 264 }, indent: { left: 360 } });

// Capa
add(
  new Paragraph({ children: [], spacing: { before: 2400 } }),
  new Paragraph({ children: [new TextRun({ text: "Fábulas Computacionais", font: FONT, size: 52, bold: true, color: BLUE })], alignment: AlignmentType.CENTER, spacing: { after: 200 } }),
  new Paragraph({ children: [new TextRun({ text: "Guia do Pesquisador", font: FONT, size: 40, color: "404040" })], alignment: AlignmentType.CENTER, spacing: { after: 120 } }),
  new Paragraph({ children: [new TextRun({ text: "Protocolo de coleta de dados — antes, durante e depois", font: FONT, size: 28, color: "404040" })], alignment: AlignmentType.CENTER, spacing: { after: 1200 } }),
  new Paragraph({ children: [new TextRun({ text: "Responsável: Rodrigo Lins Rodrigues", size: 22, color: "595959" })], alignment: AlignmentType.CENTER, spacing: { after: 80 } }),
  new Paragraph({ children: [new TextRun({ text: "Novembro de 2026 · esquema de dados 1.1", size: 22, color: "595959" })], alignment: AlignmentType.CENTER }),
  BR(),
  new Paragraph({ children: [new TextRun({ text: "Sumário", font: FONT, size: 32, bold: true, color: BLUE })], spacing: { after: 200 } }),
  new TableOfContents("Sumário", { hyperlink: true, headingStyleRange: "1-2" }),
  BR(),
);

// 1. Visão geral
add(
  H1("1. Visão geral"),
  P("Este guia descreve, passo a passo, como conduzir uma coleta de dados com o jogo Fábulas Computacionais. Ele complementa o documento **Learning Analytics — funcionalidades e exemplos de análise**, que explica o que é coletado e como analisar."),
  ...table(["Etapa", "Onde", "O que fazer"], [
    ["1. Preparar o estudo", "Unity (Painel do Estudo)", "Configurar estudo, escolas, turmas, condições e câmera; gerar códigos de participante; gerar a build."],
    ["2. Preparar as máquinas", "Escola", "Instalar o jogo, conferir a configuração de cada computador e testar a câmera (Painel do Pesquisador)."],
    ["3. Aplicar", "Escola", "Entregar os códigos, acompanhar o cadastro e o consentimento, observar a sessão."],
    ["4. Exportar", "Escola", "Com o servidor central, os dados seguem sozinhos pela internet. Mesmo assim, ao fim do dia, exporte um .zip por máquina (Painel do Pesquisador) como backup."],
    ["5. Analisar", "R (pacote fabulasLA)", "Ler todos os .zip, validar a coleta, gerar o relatório e o codebook."],
  ], [2200, 2200, 4626]),
  box("Atalhos que você vai usar", [
    P("**Ctrl+Shift+R** — abre o Painel do Pesquisador no jogo (pede o PIN).   **F9** — pausa/retoma a câmera durante a coleta.   **Esc** — menu de pausa do jogo.", { para: { spacing: { after: 0 } } }),
  ]),
  gap(),
);

// 2. Antes da coleta
add(
  BR(),
  H1("2. Antes da coleta"),
  H2("2.1 Ética e LGPD"),
  P("Os participantes são crianças e, com a câmera ligada, são coletados dados biométricos derivados (dados pessoais sensíveis — LGPD, arts. 11 e 14). Antes de qualquer coleta:"),
  check("Projeto aprovado pelo **Comitê de Ética em Pesquisa (CEP)**, descrevendo os dados do jogo e, se for o caso, a captura facial."),
  check("**TCLE** assinado pelos responsáveis e **TALE** pela criança (em papel), guardados pelo pesquisador."),
  check("Plano de armazenamento seguro, acesso restrito e **prazo de descarte** dos dados."),
  check("Decisão sobre a câmera: só ligue `affectCaptureEnabled` com consentimento específico para isso."),
  check("Se usar o servidor central: TCLE e protocolo do CEP mencionam o armazenamento dos dados (pseudonimizados) em servidor."),
  H2("2.2 Configurar o estudo (Painel do Estudo)"),
  P("No Unity, abra **Fabulas → Learning Analytics → Painel do Estudo** e preencha:"),
  N("**Estudo, escola e turma** — identificadores curtos e sem espaços (ex.: `PILOTO26`, `ESC-A`, `4A`). Eles vão para cada sessão e para o nome dos arquivos exportados."),
  N("**Identificação: Código de estudo (sem nomes)** — recomendado. A criança digita um código impresso; nenhum nome entra no computador."),
  N("**Tela de consentimento** — ligue para registrar a concordância da criança no próprio jogo. O texto fica em `Assets/StreamingAssets/consent_text.txt` e pode ser editado."),
  N("**Condições A/B** (opcional) — ex.: `A, B`. Use \"Por participante (estável)\" para que cada criança fique sempre na mesma condição."),
  N("**Câmera** — ligue só com consentimento; 2 amostras por segundo é suficiente."),
  N("**PIN do painel** — troque o padrão `1234`."),
  P("Clique em **Salvar configuração**. A seção **Verificação** mostra avisos em amarelo e erros em vermelho: resolva os erros antes de seguir."),
  H2("2.3 Servidor central (Supabase) — opcional"),
  P("Com o servidor, os dados de todas as escolas chegam sozinhos a um banco central sempre que houver internet. Os arquivos continuam salvos em cada computador (backup) e o .zip continua funcionando. A preparação é feita **uma vez** pela coordenação:"),
  N("Em supabase.com, crie um projeto (região **South America — São Paulo**) e guarde a senha do banco num gerenciador de senhas."),
  N("No projeto, abra **SQL Editor → New query**, cole todo o conteúdo de `server/supabase/schema.sql` e clique em **Run**."),
  N("Em **Project Settings → API Keys** (ou **Data API**), copie a **URL do projeto** e a chave **publishable/anon**. **Nunca** use a chave **secret/service_role** no jogo nem a envie a ninguém."),
  N("No Painel do Estudo, seção **Servidor central**: marque **Enviar ao servidor**, cole a URL e a chave pública e clique em **Gerar chaves do estudo**. É criado o arquivo `Pesquisa/servidor_<estudo>.txt`."),
  N("Abra esse arquivo, copie o comando `select public.fc_create_study(...)` e rode no **SQL Editor** do Supabase. Isso cadastra o estudo com a chave de envio (vai no jogo) e a chave de leitura (fica com a equipe, para o R)."),
  N("Clique em **Salvar configuração** e depois em **Testar conexão**: deve aparecer \"Conectado ao estudo ...\"."),
  box("Duas chaves, dois papéis", [
    P("A **chave de envio** está no jogo e só permite **gravar** dados novos daquele estudo — não lê nem apaga nada. A **chave de leitura** permite baixar os dados e fica **só** com a equipe (arquivo em `Pesquisa/`, fora do GitHub). O repositório já traz a chave de envio do estudo `FABULAS`: qualquer pessoa que rode o jogo grava nele. Para a coleta real, gere chaves de um estudo próprio (ex.: `PILOTO26`), para não misturar com testes.", { para: { spacing: { after: 0 } } }),
  ]),
  gap(),
  H2("2.4 Gerar e imprimir os códigos de participante"),
  N("No Painel do Estudo, escolha a quantidade (uma por criança, com folga de 10%) e clique em **Gerar códigos e etiquetas para imprimir**."),
  N("São criados, na pasta `Pesquisa/` do projeto, uma planilha `codigos_<estudo>_<data>.csv` e uma página `.html` com as etiquetas."),
  N("Imprima as etiquetas e recorte. Na **lista em papel** (ou na planilha guardada em local seguro), anote o nome de cada criança ao lado do código entregue. **Essa lista nunca vai para o computador da escola.**"),
  box("Por que os códigos têm dígito verificador?", [
    P("O último caractere do código é calculado a partir dos outros quatro. Se a criança errar **um** caractere ou **inverter dois vizinhos**, o jogo recusa o código e pede para digitar de novo — evitando que dados de uma criança sejam gravados com o código de outra. Letras e números parecidos (0/O, 1/I/L) não são usados.", { para: { spacing: { after: 0 } } }),
  ]),
  gap(),
  H2("2.5 Gerar a build e instalar"),
  N("No Unity: **File → Build Settings → Build** (Windows). A pasta gerada contém o `.exe` e a pasta `FabulasComputacionais_Data`."),
  N("Copie a **pasta inteira** para cada computador da escola."),
  N("Se cada máquina precisar de configuração diferente (ex.: `machineId` ou condição fixa), edite `FabulasComputacionais_Data/StreamingAssets/research_config.json` naquela máquina."),
  H2("2.6 Checklist — antes de ir à escola"),
  check("Configuração salva e sem erros no Painel do Estudo; PIN trocado."),
  check("Se usar o servidor: **Testar conexão** deu \"Conectado\" e `servidor_<estudo>.txt` está guardado em local seguro."),
  check("Etiquetas impressas e lista nome ↔ código preparada (em papel)."),
  check("Build copiada para um pendrive; pendrive extra para as exportações."),
  check("Webcams testadas (se a câmera for usada) e controles carregados."),
  check("TCLE/TALE conferidos para todas as crianças que vão participar."),
);

// 3. No dia da coleta
add(
  BR(),
  H1("3. No dia da coleta"),
  H2("3.1 Preparar cada computador"),
  N("Abra o jogo e, no menu, pressione **Ctrl+Shift+R** e digite o PIN."),
  N("Confira no Painel do Pesquisador: **estudo, escola, turma e máquina** corretos; **pasta de dados** e **espaço livre** (avisa se houver menos de 1 GB)."),
  N("Se a câmera for usada, clique em **Testar câmera**: a imagem aparece espelhada e o painel indica **\"✓ rosto detectado\"** quando a posição e a luz estão boas. Ajuste até a detecção ficar estável (acima de 80%). Nada é gravado durante o teste."),
  N("Se o servidor central estiver ligado, confira a linha **Envio**: \"em dia\" indica que a internet está funcionando. \"Sem conexão\" não impede a coleta — os dados ficam no computador e são enviados depois."),
  N("Clique em **Fechar**."),
  H2("3.2 Receber a criança"),
  N("Com a câmera ligada, ao abrir o cadastro aparece uma **telinha da câmera** no canto superior direito. Peça para a criança se sentar e ajuste a webcam até aparecer **\"✓ Rosto detectado\"** (borda verde). Amarelo ou vermelho: ajuste a posição ou a luz. A telinha some quando a criança clica em **Iniciar** e nada é gravado enquanto ela está aberta."),
  N("Entregue a etiqueta com o código e anote na lista em papel."),
  N("No jogo, clique em **Jogar** e preencha o cadastro: no campo do nome a criança digita o **código** (maiúsculas, minúsculas e hífen são aceitos), depois idade e série."),
  N("Se o código estiver errado, aparece **\"Código não reconhecido\"** — confira a etiqueta e digite de novo."),
  N("Na **tela de consentimento**, leia o texto com a criança. Ela pode autorizar ou não a câmera. Se escolher **\"Não quero participar\"**, ela joga normalmente e **nada é registrado**."),
  H2("3.3 Durante o jogo"),
  B("Com a câmera ativa, o aviso **\"● Câmera ativa (pesquisa)\"** fica no canto da tela. **F9** pausa e retoma a câmera (o evento fica registrado)."),
  B("Você pode abrir o Painel do Pesquisador a qualquer momento para ver eventos sendo gravados e a qualidade da detecção do rosto."),
  B("Anote ocorrências relevantes (interrupções, ajuda dada à criança, problemas técnicos) com o horário e o código — elas ajudam a interpretar os dados."),
  H2("3.4 Ao terminar"),
  B("O ideal é voltar ao **menu principal** (pausa → menu): isso encerra a sessão normalmente."),
  B("Se o jogo travar, for fechado ou faltar energia, **nada se perde**: na próxima abertura o jogo registra que a sessão foi interrompida, e a análise identifica esses casos."),
  H2("3.5 Exportar os dados (fim do dia)"),
  N("Em cada computador, abra o jogo, **Ctrl+Shift+R**, PIN, **Exportar coleta (.zip)**."),
  N("Clique em **Abrir pasta de dados** e copie o arquivo da pasta `Exportacoes` (ex.: `coleta_PILOTO26_ESC-A_PC-03_20261110_163000.zip`) para o pendrive."),
  N("O .zip é **anonimizado** (não leva o mapa de nomes) e tem um manifesto com checksums que permitem verificar se nada foi corrompido na cópia."),
  N("Exportar mais de uma vez não é problema: o pacote R junta tudo e lê cada sessão uma única vez."),
  N("Com o servidor central: antes de desligar, abra o painel e veja se **Aguardando envio** está em 0 (se não, clique em **Sincronizar agora**). Se a escola não tiver internet, tudo bem: o envio acontece quando o computador voltar a ter conexão e o jogo for aberto."),
  H2("3.6 Checklist — fim do dia"),
  check("Um .zip exportado de cada computador e copiado para o pendrive."),
  check("Lista nome ↔ código completa e guardada separadamente."),
  check("Anotações de campo com horários e códigos."),
);

// 4. Depois da coleta
add(
  BR(),
  H1("4. Depois da coleta"),
  H2("4.1 Organizar os arquivos"),
  P("Guarde os .zip numa pasta por estudo (ex.: `PILOTO26/escola_A/`, `PILOTO26/escola_B/`), com backup. A lista nome ↔ código fica em outro local, com acesso restrito."),
  H2("4.2 Analisar com o pacote R fabulasLA"),
  P("Instalação (uma vez), a partir da pasta do projeto:"),
  ...code(["install.packages(\"analysis/fabulasLA\", repos = NULL, type = \"source\")"]),
  P("Uso:"),
  ...code([
    "library(fabulasLA)",
    "coleta <- ler_coleta(\"C:/PILOTO26\")          # lê todos os .zip e pastas dentro",
    "validar_coleta(coleta)                       # confere a qualidade (ver 4.3)",
    "tb <- tabelas_analise(coleta)                # tabelas prontas para análise",
    "gerar_relatorio(\"C:/PILOTO26\", \"relatorio_PILOTO26.html\", titulo_estudo = \"Piloto 2026\")  # grupo",
    "gerar_relatorio_individual(\"K7M2X\", \"C:/PILOTO26\")      # um estudante: relatorio_K7M2X.html",
    "exportar_codebook(\"codebook_PILOTO26.csv\")   # dicionário de variáveis",
  ]),
  P("Com o servidor central, os dados de todas as escolas vêm direto do Supabase. Guarde a chave de leitura no arquivo `.Renviron` (linha `FABULAS_CHAVE_LEITURA=lei_...`) para não escrevê-la nos scripts:"),
  ...code([
    "url <- \"https://<projeto>.supabase.co\"; chave_publica <- \"sb_publishable_...\"",
    "status_servidor(url, chave_publica, \"PILOTO26\")          # sessões recebidas por escola",
    "coleta <- ler_coleta_servidor(url, chave_publica, \"PILOTO26\", pasta = \"C:/PILOTO26/servidor\")",
    "validar_coleta(coleta)                                   # o resto é igual",
  ]),
  P("`tabelas_analise()` devolve quatro tabelas: **sessoes** (uma por sessão, com escola, turma, condição e se foi interrompida), **fases** (tentativas, tempo, conclusão e abandono por fase), **tentativas** (estado da solução a cada tentativa) e **afeto_sessao** (cobertura e médias faciais)."),
  P("**Dois relatórios:** `gerar_relatorio()` analisa o **estudo em grupo** (todas as crianças, escolas e condições); `gerar_relatorio_individual(\"CÓDIGO\", ...)` faz a **devolutiva de uma criança**: percurso, desempenho em cada fase comparado com os colegas, estratégia de resolução, erros repetidos, engajamento, expressões faciais em cada cena e logo depois de cada erro ou acerto, e os **estados de aprendizagem estimados** (confusão, frustração, tédio, engajamento, encantamento — indicadores, não diagnósticos). Para editar os modelos no RStudio: `copiar_modelos_relatorio(\"meus_relatorios\")`."),
  P("Para testar o fluxo antes de ter dados reais: `gerar_relatorio(NULL)` e `gerar_relatorio_individual()` geram relatórios completos com dados simulados."),
  H2("4.3 Interpretar a validação"),
  ...table(["Verificação", "Se aparecer aviso/erro", "O que fazer"], [
    ["Integridade dos arquivos", "Checksum diferente do manifesto", "O .zip foi corrompido na cópia: exporte de novo daquela máquina."],
    ["Eventos duplicados", "Ids repetidos", "Raro; normalmente a mesma sessão lida de fontes diferentes. Remova arquivos duplicados manualmente."],
    ["Sequência de eventos", "Lacunas na sequência", "Eventos perdidos (ex.: disco cheio). Avalie se a sessão pode ser usada."],
    ["Início das sessões", "Sessão sem session_start", "Arquivo incompleto; exclua a sessão."],
    ["Fim das sessões", "Sessão sem session_end", "Sessão em andamento na exportação ou jogo não reaberto após queda. Normalmente pode ser usada."],
    ["Sessões interrompidas", "Fim reconstruído", "Use normalmente, mas faça análise de sensibilidade (com e sem essas sessões)."],
    ["Eventos fora do catálogo", "Tipos desconhecidos", "Versão diferente do jogo; atualize o pacote R."],
    ["Cobertura da câmera", "Rosto detectado em < 60% das amostras", "Exclua essas sessões das análises faciais (o relatório já faz isso)."],
  ], [2200, 2600, 4226]),
  H2("4.4 Armazenamento e descarte"),
  B("Mantenha os dados em local com acesso restrito e backup; não envie por e-mail ou mensageiros."),
  B("Para compartilhar dados (ex.: repositório de dados abertos), use apenas os .zip anonimizados e o codebook; nunca a lista nome ↔ código."),
  B("Respeite o prazo de descarte definido no projeto aprovado pelo CEP."),
);

// 5. Problemas
add(
  BR(),
  H1("5. Solução de problemas"),
  ...table(["Problema", "Causa provável", "Solução"], [
    ["\"Código não reconhecido\"", "Erro de digitação", "Conferir a etiqueta: são 5 caracteres, sem 0, O, 1, I ou L."],
    ["Ctrl+Shift+R não abre o painel", "Janela do jogo sem foco", "Clicar na janela do jogo e repetir."],
    ["\"Nenhuma webcam encontrada\"", "Câmera desconectada ou usada por outro programa", "Reconectar a câmera, fechar outros programas (Teams, Zoom) e reabrir o jogo."],
    ["\"Nenhum rosto\" no teste da câmera", "Posição ou luz", "Centralizar a câmera no rosto, evitar janela atrás da criança, aumentar a luz."],
    ["\"MediaPipe não instalado\"", "Build sem o plugin", "Gerar a build com o plugin instalado (ver README da pasta LearningAnalytics)."],
    ["Pouco espaço em disco (aviso no painel)", "Disco quase cheio", "Exportar, copiar o .zip e liberar espaço antes de continuar."],
    ["A tela de consentimento não aparece", "requireConsent desligado", "Ligar no Painel do Estudo (ou no research_config.json da máquina)."],
    ["O relatório não gera no R", "Pacote ausente", "Instalar o pacote indicado na mensagem de erro (install.packages)."],
    ["Painel: \"sem conexão — tentando de novo\"", "Escola sem internet ou rede bloqueando o Supabase", "Seguir a coleta normalmente; os dados são enviados depois. Exportar o .zip como backup."],
    ["Painel: \"chave de envio inválida\"", "Estudo não cadastrado no Supabase ou chaves geradas de novo", "Rodar no SQL Editor o comando de `Pesquisa/servidor_<estudo>.txt` e gerar a build com o `server_sync.json` atual."],
    ["Painel: \"função não encontrada\"", "`schema.sql` não foi executado no projeto", "Rodar `server/supabase/schema.sql` no SQL Editor."],
  ], [2600, 2400, 4026]),
);

// 6. Referência
add(
  H1("6. Referência rápida"),
  ...table(["Item", "Onde / como"], [
    ["Configuração da máquina", "`FabulasComputacionais_Data/StreamingAssets/research_config.json`"],
    ["Texto do consentimento", "`FabulasComputacionais_Data/StreamingAssets/consent_text.txt`"],
    ["Dados coletados", "`%USERPROFILE%\\AppData\\LocalLow\\DefaultCompany\\Fabulas Computacionais\\LearningAnalytics`"],
    ["Exportações", "`...\\LearningAnalytics\\Exportacoes\\coleta_*.zip`"],
    ["Envio ao servidor", "`FabulasComputacionais_Data/StreamingAssets/server_sync.json` (já vem configurado no repositório)"],
    ["Chaves do estudo", "`Pesquisa/servidor_<estudo>.txt` (guardar em local seguro)"],
    ["Banco de dados", "`server/supabase/schema.sql` (rodar uma vez no SQL Editor)"],
    ["Painel do Pesquisador", "No jogo: Ctrl+Shift+R + PIN"],
    ["Painel do Estudo", "No Unity: Fabulas → Learning Analytics → Painel do Estudo"],
    ["Pacote R", "`analysis/fabulasLA` — `ler_coleta`, `ler_coleta_servidor`, `status_servidor`, `validar_coleta`, `tabelas_analise`, `gerar_relatorio` (grupo), `gerar_relatorio_individual` (estudante), `afeto_por_evento`, `estados_faciais`, `exportar_codebook`"],
    ["Documento de funcionalidades", "`Assets/LearningAnalytics/Learning_Analytics_Fabulas_Computacionais.docx`"],
  ], [2600, 6426]),
);

// ---------------------------------------------------------------- documento
const doc = new Document({
  creator: "Rodrigo Lins Rodrigues",
  title: "Fábulas Computacionais — Guia do Pesquisador",
  description: "Protocolo de coleta de dados com o jogo Fábulas Computacionais",
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
      default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT, children: [new TextRun({ text: "Fábulas Computacionais — Guia do Pesquisador", size: 16, color: "808080" })] })] }),
    },
    footers: {
      default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ children: ["Página ", PageNumber.CURRENT], size: 16, color: "808080" })] })] }),
    },
    children,
  }],
});

// Uso: node gerar_documento.js [saida.docx]
// Padrão: Assets/LearningAnalytics/Guia_do_Pesquisador.docx
const out = process.argv[2] || path.join(__dirname, "..", "Guia_do_Pesquisador.docx");
Packer.toBuffer(doc).then((buf) => { fs.writeFileSync(out, buf); console.log("ok", out, buf.length); });

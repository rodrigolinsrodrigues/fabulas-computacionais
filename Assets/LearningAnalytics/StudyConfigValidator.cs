using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Confere a configuração do estudo antes de uma coleta e devolve avisos em linguagem simples.
/// Usado pelo Painel do Estudo (Unity) e pelos testes automatizados.
/// </summary>
public static class StudyConfigValidator
{
    public enum Level { Info, Warning, Error }

    public struct Issue
    {
        public Level level;
        public string message;
        public override string ToString() => $"[{level}] {message}";
    }

    public static List<Issue> Validate(ResearchConfig c, bool pluginAvailable, string streamingAssetsPath = null)
    {
        streamingAssetsPath ??= Application.streamingAssetsPath;
        var list = new List<Issue>();
        void Add(Level l, string m) => list.Add(new Issue { level = l, message = m });

        if (!c.telemetryEnabled) Add(Level.Error, "A telemetria está desligada: nenhum dado será coletado.");
        if (string.IsNullOrWhiteSpace(c.studyId)) Add(Level.Warning, "Informe o identificador do estudo (studyId) para separar os dados de cada pesquisa.");
        if (string.IsNullOrWhiteSpace(c.schoolId)) Add(Level.Warning, "Informe a escola (schoolId): sem ela não dá para comparar escolas na análise.");
        if (string.IsNullOrWhiteSpace(c.classId)) Add(Level.Info, "Turma (classId) não informada.");

        if (c.participantIdMode != "name" && c.participantIdMode != "code")
            Add(Level.Error, $"participantIdMode deve ser \"name\" ou \"code\" (está \"{c.participantIdMode}\").");
        if (c.participantIdMode == "name")
            Add(Level.Info, "Participantes identificados por nome: o mapa nome↔código fica no computador. Para mais privacidade, use códigos de estudo (participantIdMode = code).");

        if (c.affectCaptureEnabled)
        {
            if (!pluginAvailable) Add(Level.Error, "A câmera está ligada, mas o MediaPipe Unity Plugin não está instalado.");
            if (!c.requireConsent) Add(Level.Warning, "A câmera está ligada sem a tela de consentimento no jogo. Confirme que o TCLE/TALE foi coletado em papel.");
            string model = Path.Combine(streamingAssetsPath, c.modelFileName ?? "");
            if (!File.Exists(model)) Add(Level.Error, $"Modelo facial não encontrado: {c.modelFileName}");
            if (c.affectSampleRateHz < 0.5f || c.affectSampleRateHz > 10f) Add(Level.Warning, "affectSampleRateHz fora do recomendado (0,5 a 10 amostras por segundo).");
        }

        if (c.requireConsent && !File.Exists(Path.Combine(streamingAssetsPath, c.consentTextFile ?? "")))
            Add(Level.Warning, $"Texto de consentimento não encontrado ({c.consentTextFile}); será usado um texto padrão.");

        if (c.conditions != null && c.conditions.Length > 0)
        {
            if (c.conditionAssignment != "fixed" && c.conditionAssignment != "random" && c.conditionAssignment != "by_participant")
                Add(Level.Error, "conditionAssignment deve ser fixed, random ou by_participant.");
            if (c.conditionAssignment == "fixed" && !string.IsNullOrEmpty(c.condition) && System.Array.IndexOf(c.conditions, c.condition) < 0)
                Add(Level.Error, $"A condição fixa \"{c.condition}\" não está na lista de condições.");
        }

        if (c.researcherPin == "1234") Add(Level.Warning, "O PIN do Painel do Pesquisador ainda é o padrão (1234). Troque antes de levar à escola.");
        if (string.IsNullOrEmpty(c.researcherPin)) Add(Level.Warning, "O Painel do Pesquisador está sem PIN: qualquer pessoa pode abri-lo com Ctrl+Shift+R.");
        if (c.exportIncludesNameMap) Add(Level.Warning, "A exportação vai incluir o mapa de nomes (dado pessoal). Desligue se não for necessário.");
        if (c.idleThresholdSeconds < 5) Add(Level.Warning, "idleThresholdSeconds muito baixo: a ociosidade será registrada com frequência excessiva.");

        return list;
    }
}

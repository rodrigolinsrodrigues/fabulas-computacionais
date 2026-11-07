using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Envia ao servidor central (Supabase) os eventos e TODAS as amostras faciais, em segundo plano.
///
/// - Os arquivos locais continuam sendo gravados e nunca são apagados: são o backup.
/// - Sem internet, o jogo segue normalmente; o envio é tentado de novo com espera crescente
///   (até 5 min) e o que faltar é enviado na próxima vez que o jogo abrir.
/// - Ao fechar o jogo, tenta enviar o final da sessão por alguns segundos.
/// - Configuração: StreamingAssets/server_sync.json (ver ServerSyncConfig). -noServerSync desliga.
/// </summary>
public class DataUploader : MonoBehaviour
{
    public static DataUploader Instance { get; private set; }

    // Estado para o painel do pesquisador (lido na thread principal)
    public bool Enabled { get; private set; }
    public bool Busy { get; private set; }
    public DateTime? LastSuccessUtc { get; private set; }
    public string LastError { get; private set; } = "";
    public long PendingEvents { get; private set; }
    public long PendingAffect { get; private set; }
    public long TotalEventsSent { get; private set; }
    public long TotalAffectSent { get; private set; }

    private ServerSyncConfig _cfg;
    private Thread _worker;
    private readonly AutoResetEvent _wake = new AutoResetEvent(false);
    private volatile bool _stop;
    private int _failures;
    private string _folder;
    private readonly object _runLock = new object();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _cfg = ServerSyncConfig.Current;
        _folder = TelemetryManager.DataFolder;
        Enabled = _cfg.IsComplete && !ResearchConfig.HasCommandLineFlag("-noServerSync");
        if (!Enabled)
        {
            if (_cfg.enabled) Debug.LogWarning("[Servidor] server_sync.json incompleto (URL, chave pública, estudo ou chave de envio). Envio desligado.");
            return;
        }
        Debug.Log($"[Servidor] Envio ao servidor ligado (estudo {_cfg.EffectiveStudyId}).");
        _worker = new Thread(Loop) { IsBackground = true, Name = "FabulasDataUploader" };
        _worker.Start();
    }

    /// <summary>Pede um envio imediato (botão "Sincronizar agora" e fim de sessão).</summary>
    public void SyncNow()
    {
        if (!Enabled) return;
        _failures = 0;
        _wake.Set();
    }

    private void Loop()
    {
        Thread.Sleep(2000);   // deixa o jogo abrir primeiro
        while (!_stop)
        {
            RunOnce(15000);
            float interval = Mathf.Max(5f, _cfg.syncIntervalSeconds);
            double wait = _failures == 0 ? interval : Math.Min(300, interval * Math.Pow(2, Math.Min(_failures, 6)));
            _wake.WaitOne(TimeSpan.FromSeconds(wait));
        }
    }

    private void RunOnce(int timeoutMs)
    {
        lock (_runLock)
        {
            Busy = true;
            try
            {
                var engine = new SyncEngine(_folder, _cfg.EffectiveStudyId, _cfg.uploadKey,
                    (rpc, body) => Post(_cfg.serverUrl, _cfg.anonKey, rpc, body, timeoutMs));
                var r = engine.SyncOnce();
                PendingEvents = r.pendingEvents;
                PendingAffect = r.pendingAffect;
                TotalEventsSent += r.eventsSent;
                TotalAffectSent += r.affectSent;
                if (r.ok)
                {
                    _failures = 0;
                    LastError = "";
                    LastSuccessUtc = DateTime.UtcNow;
                    if (r.eventsSent + r.affectSent > 0)
                        Debug.Log($"[Servidor] Enviados {r.eventsSent} eventos e {r.affectSent} amostras faciais.");
                }
                else
                {
                    _failures++;
                    if (LastError != r.error) Debug.LogWarning("[Servidor] Envio falhou (dados seguem salvos no computador): " + r.error);
                    LastError = r.error;
                }
            }
            catch (Exception e)
            {
                _failures++;
                LastError = e.Message;
                Debug.LogWarning("[Servidor] Erro no envio (dados seguem salvos no computador): " + e.Message);
            }
            finally
            {
                Busy = false;
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (!Enabled) return;
        // Garante que o fim da sessão e o CSV facial estejam no disco antes do último envio.
        if (TelemetryManager.Instance != null && TelemetryManager.Instance.SessionActive)
            TelemetryManager.Instance.EndSession("app_quit");
        _stop = true;
        _wake.Set();
        // Último envio com prazo curto; o que não der tempo vai na próxima abertura do jogo.
        var t = new Thread(() => RunOnce(4000)) { IsBackground = true };
        t.Start();
        t.Join(6000);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ------------------------------------------------------------------ HTTP

    /// <summary>POST {url}/rest/v1/rpc/{função} com a chave pública do Supabase.</summary>
    public static (bool ok, int status, string body) Post(string serverUrl, string anonKey, string rpc, string json, int timeoutMs)
    {
        try
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var req = (HttpWebRequest)WebRequest.Create(serverUrl.TrimEnd('/') + "/rest/v1/rpc/" + rpc);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Accept = "application/json";
            req.Timeout = timeoutMs;
            req.ReadWriteTimeout = timeoutMs;
            req.Headers["apikey"] = anonKey;
            if (anonKey.StartsWith("eyJ")) req.Headers["Authorization"] = "Bearer " + anonKey;   // chave "anon" antiga (JWT)
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            req.ContentLength = bytes.Length;
            using (var s = req.GetRequestStream()) s.Write(bytes, 0, bytes.Length);
            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return (true, (int)resp.StatusCode, sr.ReadToEnd());
        }
        catch (WebException e) when (e.Response is HttpWebResponse r)
        {
            using (r)
            using (var sr = new StreamReader(r.GetResponseStream(), Encoding.UTF8))
                return (false, (int)r.StatusCode, sr.ReadToEnd());
        }
        catch (Exception e)
        {
            return (false, 0, e.Message);
        }
    }
}

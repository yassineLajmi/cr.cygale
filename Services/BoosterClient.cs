using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CyberesaBoosterBridge.Models;

namespace CyberesaBoosterBridge.Services;

public class BoosterOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Password { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public bool UseMock { get; set; } = false;
}

public interface IBoosterClient
{
    Task<BoosterAvailabilityResponse?> GetAvailabilityAsync(
        IDictionary<string, string?> query,
        CancellationToken ct);
}

public class BoosterClient : IBoosterClient
{
    private readonly HttpClient _http;
    private readonly BoosterOptions _opt;
    private readonly ILogger<BoosterClient> _log;

    public BoosterClient(HttpClient http, BoosterOptions opt, ILogger<BoosterClient> log)
    {
        _http = http;
        _opt = opt;
        _log = log;
        _http.BaseAddress = new Uri(opt.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
    }

    public async Task<BoosterAvailabilityResponse?> GetAvailabilityAsync(
        IDictionary<string, string?> query,
        CancellationToken ct)
    {
        var (ts, sig) = Sign();
        var qs = string.Join("&", query
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

        var req = new HttpRequestMessage(HttpMethod.Get, $"/hotels/availability?{qs}");
        req.Headers.Add("apiKey", _opt.ApiKey);
        req.Headers.Add("timestamp", ts);
        req.Headers.Add("signature", sig);

        var fullUrl = new Uri(_http.BaseAddress!, req.RequestUri!).ToString();
        var callId = BoosterCallLogger.NewCallId();

        var reqDump = $"GET {fullUrl}\napiKey: {_opt.ApiKey}\ntimestamp: {ts}\nsignature: {sig}\n";
        BoosterCallLogger.SaveRequest(callId, reqDump);
        _log.LogInformation(
            "\n===== BOOSTER REQ [{CallId}] =====\n{Dump}==============================",
            callId, reqDump);

        using var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        var pretty = PrettyJson(body);

        BoosterCallLogger.SaveResponse(callId, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}\n{pretty}");
        _log.LogInformation(
            "\n===== BOOSTER RES [{CallId}] =====\nHTTP {Code} {Reason}\n{Body}\n==============================",
            callId, (int)resp.StatusCode, resp.ReasonPhrase, pretty);

        if (!resp.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Booster availability failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }

        return JsonSerializer.Deserialize<BoosterAvailabilityResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private static string PrettyJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return raw;
        }
    }

    private (string ts, string sig) Sign()
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var payload = _opt.ApiKey + _opt.Password + ts;
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(payload));
        var hex = Convert.ToHexStringLower(hash);
        return (ts, hex);
    }
}

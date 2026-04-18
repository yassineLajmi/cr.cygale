using System.Globalization;

namespace CyberesaBoosterBridge.Services;

public static class BoosterCallLogger
{
    private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, "logs", "booster");
    private static readonly object Gate = new();

    public static string NewCallId() =>
        DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture)
        + "_" + Guid.NewGuid().ToString("N")[..6];

    public static void SaveRequest(string callId, string content)
        => Write($"{callId}_REQ.txt", content);

    public static void SaveResponse(string callId, string content)
        => Write($"{callId}_RES.json", content);

    private static void Write(string fileName, string content)
    {
        lock (Gate)
        {
            Directory.CreateDirectory(LogDir);
            File.WriteAllText(Path.Combine(LogDir, fileName), content);
        }
    }

    public static string Dir => LogDir;
}

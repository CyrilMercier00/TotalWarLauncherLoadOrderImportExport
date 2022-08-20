using System.Diagnostics;

namespace LauncherMiddleware;

internal static class Logger
{
    private static string? OutputFile { get; set; }

    public static void Log(Exception e) => Log(e, string.Empty);
    public static void Log(string message) => Log(null, message);
    public static void Log(Exception? e, string message)
    {
        if (string.IsNullOrEmpty(OutputFile)) OutputFile = GenerateDefaultLogPath();

        var stream = File.Open(OutputFile, FileMode.Append);
        using var streamWriter = new StreamWriter(stream);

        streamWriter.WriteLine(DateTime.Now);
        if (!string.IsNullOrEmpty(message)) streamWriter.WriteLine(message);
        if (e is not null) streamWriter.WriteLine(e.InnerException);
        streamWriter.WriteLine();

        streamWriter.Close();
    }

    private static string GenerateDefaultLogPath()
    {
        var path = Directory.GetCurrentDirectory() + "\\latest.log";
        Console.Write($"Log output : {path}");
        return path;
    }
}
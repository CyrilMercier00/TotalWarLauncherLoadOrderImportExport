using System.Reflection;

namespace Logging;

public class Logger
{
    private const string OutputDirectory = "./Logs";
    public static bool Toggled = true;

    public Logger ()
    {
        // Backup old logs
        var existingLogs = new List<string>();
        if (File.Exists(OutputDirectory)) existingLogs = Directory.EnumerateFiles(OutputDirectory).Where(file => file.EndsWith(".log")).ToList();
        foreach (string file in existingLogs)
        {
            string newName = string.Concat(existingLogs, ".old");
            File.Move(file, newName);
        }

        // Create new logs
        string name = Assembly.GetCallingAssembly().GetName().Name ?? "defaultName";
        Filename = string.Concat(name, ".log");
        File.Create(Filename);
    }

    /// <summary>
    /// Name of the file where the logs are written
    /// </summary>
    private string Filename { get; }

    public void Log (Exception e)
    {
        Log(e, string.Empty);
    }

    public void Log (string message)
    {
        Log(null, message);
    }

    public void Log (Exception? e, string message)
    {
        if (!Toggled) return;

        string name = Assembly.GetCallingAssembly().GetName().Name ?? "defaultName";
        string fileName = string.Concat(name, ".log");

        var stream = File.Open(fileName, FileMode.Append);
        using var streamWriter = new StreamWriter(stream);

        streamWriter.WriteLine(DateTime.Now);
        if (!string.IsNullOrEmpty(message)) streamWriter.WriteLine(message);
        if (e is not null) streamWriter.WriteLine(e.InnerException);
        streamWriter.WriteLine();

        streamWriter.Close();
    }
}

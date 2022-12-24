using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImportExportInterface.Logging;

public class Logger
{
    private string Filename { get; }
    private const string OutputDirectory = "./Logs";
    private bool _toggled;

    public Logger()
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

    public void On() { _toggled = true; }
    public void Off() { _toggled = false; }

    public void Log(Exception e) => Log(e, string.Empty);

    public void Log(string message) => Log(null, message);

    public void Log(Exception? e, string message)
    {
        if (!_toggled) return;

        string name = Assembly.GetCallingAssembly().GetName().Name ?? "defaultName";
        string fileName = string.Concat(name, ".log");

        var stream = File.Open(fileName, FileMode.Append);
        using var streamWriter = new StreamWriter(stream);

        streamWriter.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(message)) streamWriter.WriteLine(message);
        if (e is not null) streamWriter.WriteLine(e.InnerException);
        streamWriter.WriteLine();

        streamWriter.Close();
    }
}

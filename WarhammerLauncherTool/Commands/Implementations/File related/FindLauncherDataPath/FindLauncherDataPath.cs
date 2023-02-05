using System;
using System.IO;
using Serilog;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;

public class FindLauncherDataPath : IFindLauncherDataPath
{
    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";
    private readonly ILogger _logger;

    public FindLauncherDataPath(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public string Execute()
    {
        try
        {
            string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string launcherFolder = $"{appdataFolder}{DefaultLauncherFolderPath}";
            string path = $"{launcherFolder}{DefaultLauncherDataFilename}";

            _logger.Information("Launcher data path generated: {Path}", path);

            if (File.Exists(path)) return path;

            _logger.Warning("Unable to find the file containing the launcher data");
            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while retrieving launcher data");
            throw;
        }
    }
}

using System;
using System.IO;
using Serilog;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;

public class FindLauncherDataPath : IFindLauncherDataPath
{
    private readonly ILogger _logger;

    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";

    public FindLauncherDataPath(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    /// <summary>
    /// Attempts to retrieve the path of a file containing launcher data from the user's application data folder.
    /// It will log a warning if the file is not found.
    /// </summary>
    /// <returns></returns>
    public string Execute()
    {
        try
        {
            string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string launcherFolder = $"{appdataFolder}{DefaultLauncherFolderPath}";

            string path = $"{launcherFolder}{DefaultLauncherDataFilename}";

            _logger.Information("Launcher data path generated: {Path}", path);

            if (File.Exists(path)) return path;

            _logger.Warning("Unable to find the file containing the launcher data at : {Path}", path);

            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while retrieving launcher data");
            throw;
        }
    }
}

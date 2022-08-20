namespace LauncherMiddleware.Utils;

internal static class Config
{
    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";

    private static string _launcherDataPath = string.Empty;
    /// <summary>
    /// <para> Path to the launcher config file. </para>
    /// <para> If not set, return a default path. </para>
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    public static string LauncherDataPath
    {
        get
        {
            Logger.Log($"Retrieving launcherDataPath");
            if (!string.IsNullOrEmpty(_launcherDataPath)) return _launcherDataPath;
            
            Logger.Log($"Path is not configured ! Setting it to the default one.");
            
            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var launcherFolder = appdataFolder + DefaultLauncherFolderPath;
            var path = launcherFolder + DefaultLauncherDataFilename;

            Logger.Log($"Launcher data path generated: {_launcherDataPath}");

            if (File.Exists(path))
            {
                _launcherDataPath = path;
                return _launcherDataPath;
            }
            else
            {
                const string message = "Unable to find the file containing the launcher data";
                Logger.Log(message);
                throw new FileNotFoundException(message);
            }
        }

        set
        {
            if (File.Exists(value)) _launcherDataPath = value;
            else throw new FileNotFoundException($"Unable to access {value}. The file does not exists or is protected");
        }
        

    }
}
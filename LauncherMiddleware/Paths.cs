namespace LauncherMiddleware;

internal static class Paths
{
    private const string LauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string LauncherDataFilename = "20190104-moddata.dat";
    
    private static string _launcherDataPath = string.Empty;
    public static string LauncherDataPath
    {
        get
        {
            Logger.Log($"Retreiving launcherDataPath");
            if (!string.IsNullOrEmpty(_launcherDataPath)) return _launcherDataPath;
            
            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var launcherFolder = appdataFolder + LauncherFolderPath;
            _launcherDataPath = launcherFolder + LauncherDataFilename;

            Logger.Log($"Launcher data path generated: {_launcherDataPath}");
            return _launcherDataPath;
        }
        
        private set =>  _launcherDataPath = value;
    }
}
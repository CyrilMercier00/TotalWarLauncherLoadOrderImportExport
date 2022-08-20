using LauncherMiddleware.Utils;
using Newtonsoft.Json;

namespace LauncherMiddleware;

public static class Export
{
    /// <summary>
    /// Returns a file containing the load order for a game
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    public static MemoryStream ExportMods(GameName gameName)
    {
        Logger.Log($"Exporting mod data to stream for {gameName}");

        var path = Config.LauncherDataPath;
        var mods = Commons.GetModsFromFile(path).Where(mod => mod.Game == gameName).ToList();
        var stream = ExportMods(mods);
        
        return stream;
    }

    /// <summary>
    /// Returns a file containing the load order for a game
    /// </summary>
    /// <param name="mods"></param>
    /// <returns></returns>
    public static MemoryStream ExportMods(List<ModData> mods)
    {
        Logger.Log($"Exporting mod data to stream for {mods.Count} mods");
        var stream = new MemoryStream();
        try
        {
            var writer = new StreamWriter(stream);

            var modString = JsonConvert.SerializeObject(mods);
            writer.Write(modString);

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            Logger.Log(e);
            throw;
        }
    }
}
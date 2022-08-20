using LauncherMiddleware.Utils;
using Newtonsoft.Json;

namespace LauncherMiddleware;

public static class Export
{
    /// <summary>
    ///     Returns a file containing the load order for a game
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    public static MemoryStream ExportMods (GameName gameName)
    {
        try
        {
            Logger.Log($"Exporting mod data to stream for {gameName}");

            string? path = Config.LauncherDataPath;
            var mods = Commons.GetModsFromFile(path);
            var filteredMods = mods.Where(mod => mod.Game == gameName).ToList();
            var stream = ExportMods(filteredMods);

            return stream;
        }
        catch (Exception e)
        {
            Logger.Log(e, e.Message);
            throw;
        }
    }

    /// <summary>
    ///     Returns a file containing the load order for a game
    /// </summary>
    /// <param name="mods"></param>
    /// <returns></returns>
    public static MemoryStream ExportMods (List<ModData> mods)
    {
        Logger.Log($"Exporting mod data to stream for {mods.Count} mods");
        var stream = new MemoryStream();
        try
        {
            var writer = new StreamWriter(stream);

            string? modString = JsonConvert.SerializeObject(mods);
            writer.Write(modString);

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            Logger.Log(e, e.Message);
            throw;
        }
    }
}
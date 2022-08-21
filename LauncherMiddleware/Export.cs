using LauncherMiddleware.Models;
using Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LauncherMiddleware;

public static class Export
{
    /// <summary>
    /// Returns a file containing the load order for a game
    /// </summary>
    /// <param name="gameName"></param>
    /// <param name="launcherDataPath"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static MemoryStream CreateFile (GameName gameName, string launcherDataPath, Logger? logger)
    {
        try
        {
            logger?.Log($"Exporting mod data to stream for {gameName}");

            var stream = File.Open(launcherDataPath, FileMode.Open);
            var mods = Commons.GetModsFromStream(stream, logger);
            var filteredMods = mods.Where(mod => mod.Game == gameName && mod.Active).ToList();
            var exportStream = CreateFile(filteredMods, logger);

            return exportStream;
        }
        catch (Exception e)
        {
            logger?.Log(e, e.Message);
            throw;
        }
    }

    public static MemoryStream CreateFile (GameName gameName, string launcherDataPath)
    {
        return CreateFile(gameName, launcherDataPath, null);
    }

    /// <summary>
    /// Returns a file containing the load order for a game
    /// </summary>
    /// <param name="mods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static MemoryStream CreateFile (List<Mod> mods, Logger? logger)
    {
        logger?.Log($"Exporting mod data to stream for {mods.Count} mods");
        var stream = new MemoryStream();
        try
        {
            var options = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };

            string? modString = JsonConvert.SerializeObject(mods, options);
            var writer = new StreamWriter(stream);
            writer.Write(modString);

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            logger?.Log(e, e.Message);
            throw;
        }
    }
}

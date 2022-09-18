using LauncherMiddleware.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LauncherMiddleware;

public static class LauncherMethods

{
    /// <summary>
    /// Returns a stream containing the load order for a game
    /// </summary>
    /// <param name="gameName"></param>
    /// <param name="launcherDataPath"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static MemoryStream ExportToStream(GameName gameName, string launcherDataPath, Logger.Logger? logger)
    {
        try
        {
            logger?.Log($"Exporting mod data to stream for {gameName}");

            var stream = File.Open(launcherDataPath, FileMode.Open);
            var mods = GetModsFromStream(stream, logger);
            var filteredMods = mods.Where(mod => mod.Game == gameName).ToList();
            var exportStream = ExportToStream(filteredMods, logger);

            return exportStream;
        }
        catch (Exception e)
        {
            logger?.Log(e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Returns a stream containing the load order for a game
    /// </summary>
    /// <param name="mods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static MemoryStream ExportToStream(List<Mod> mods, Logger.Logger? logger)
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
            writer.Flush();

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            logger?.Log(e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieve the contents of a file and parse it into a mod list
    /// </summary>
    /// <returns></returns>
    public static List<Mod> GetModsFromStream(Stream stream, Logger.Logger? logger)
    {
        var extractedMods = new List<Mod>();
        var streamReader = new StreamReader(stream);

        // Deserialize
        logger?.Log("Deserializing stream");
        var serializer = new JsonSerializer();
        using (var reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            while (reader.Read())
            {
                var data = serializer.Deserialize<List<Mod>>(reader);
                if (data is null) { logger?.Log("No mods were deserialized !"); }
                else { extractedMods.AddRange(data); }
            }
        }

        return extractedMods;
    }
}

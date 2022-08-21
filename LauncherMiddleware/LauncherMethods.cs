using LauncherMiddleware.Models;
using Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LauncherMiddleware;

public static class LauncherMethods

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
            var mods = GetModsFromStream(stream, logger);
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

    /// <summary>
    /// Returns a file containing the load order for a game
    /// </summary>
    /// <param name="mods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static MemoryStream CreateFile (List<Mod> mods, Logger? logger)
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

    /// <summary>
    /// Retrieve the contents of a file
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Mod> GetModsFromStream (Stream stream, Logger? logger)
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

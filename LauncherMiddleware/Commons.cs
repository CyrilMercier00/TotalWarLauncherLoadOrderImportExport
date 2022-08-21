using LauncherMiddleware.Models;
using Logging;
using Newtonsoft.Json;

namespace LauncherMiddleware;

public static class Commons
{
    /// <summary>
    /// Retrieve the contents of a file
    /// </summary>
    /// <returns></returns>
    public static List<Mod> GetModsFromStream (Stream stream, Logger? logger)
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

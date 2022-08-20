using Newtonsoft.Json;

namespace LauncherMiddleware;



public static class Commons
{
    /// <summary>
    /// Retrieve the contents of a file
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ModData> GetModsFromFile(string path)
    {
        var extractedMods = new List<ModData>();

        // Read
        Logger.Log($"Opening file {path}");
        var stream = File.Open(path, FileMode.Open);
        var streamReader = new StreamReader(stream);

        // Deserialize
        Logger.Log($"Deserializing stream");
        var serializer = new Newtonsoft.Json.JsonSerializer();
        using (var reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            while (reader.Read())
            {
                var data = serializer.Deserialize<List<ModData>>(reader);
                if (data is null) Logger.Log("No mods were deserialized !");
                else extractedMods.AddRange(data);
            }
        }
        
        return extractedMods;
    }
}
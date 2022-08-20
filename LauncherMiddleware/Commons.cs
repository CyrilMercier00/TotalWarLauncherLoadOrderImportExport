using Newtonsoft.Json;

namespace LauncherMiddleware;

using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

public static class Commons
{
    /// <summary>
    ///  Get a list of all the mods present in the targeted file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<ModData> GetModsFromFile(string path)
    {
        Logger.Log($"Retrieving mods from {path}");
        
        var lines = ReadFile(path);
        var mods = ExtractMods(lines);
        
        Logger.Log($"Extracted {mods.Count} mods");

        return mods;
    }

    /// <summary>
    /// Retrieve the contents of a file
    /// </summary>
    /// <returns></returns>
    private static string[] ReadFile(string path)
    {
        Logger.Log($"Reading data from {path} ");

        var lines = File.ReadAllLines(path);
        if (!lines.Any())
        {
            Logger.Log($"No text found in {path}");
            return lines;
        }

        Logger.Log($"{lines.Length} lines read from file");
        return lines;
    }

    /// <summary>
    /// Parse JSON data to a list of mods
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    private static List<ModData> ExtractMods(string[] lines)
    {
        var extractedMods = new List<ModData>();

        Logger.Log($"Converting mod data");

        var deserializedMods = JsonConvert.DeserializeObject<List<ModData>>(lines[0]);
        if (deserializedMods != null)
        {
            Logger.Log($"Mods extracted successfully, {deserializedMods.Count} mods found");
            extractedMods.AddRange(deserializedMods);
        }
        else
        {
            Logger.Log($"No mods found");
        }

        return extractedMods;
    }
}
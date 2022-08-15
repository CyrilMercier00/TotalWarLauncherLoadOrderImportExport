using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

namespace LauncherMiddleware;

public static class Export
{
    public static MemoryStream ExportDataToStream(GameName gameName)
    {
        Logger.Log($"Exporting mod data to stream for {gameName}");
        var modData = GetModData(gameName);
        return ExportDataToStream(modData);
    }

    public static MemoryStream ExportDataToStream(List<ModData> mods)
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

    public static List<ModData> GetModData(GameName gameName)
    {
        var lines = GetRawLauncherData();
        var mods = GetConvertedModData(lines);
        var gameMods = mods.Where(mod => mod.Game == gameName).ToList();

        Logger.Log($"Extracted {gameMods.Count} for {gameName}");

        return gameMods;
    }

    private static List<ModData> GetConvertedModData(string[] lines)
    {
        var extractedMods = new List<ModData>();

        Logger.Log($"Converting mod data");

        var deserializedMods = JsonConvert.DeserializeObject<List<ModData>>(lines[0]);
        if (deserializedMods != null)
        {
            Logger.Log($"Mods extracted succesfully, {deserializedMods.Count} mods found");
            extractedMods.AddRange(deserializedMods);
        }
        else
        {
            Logger.Log($"No mods found");
        }

        return extractedMods;
    }

    private static string[] GetRawLauncherData()
    {
        Logger.Log($"Reading launcher data from {Paths.LauncherDataPath} ");

        var lines = File.ReadAllLines(Paths.LauncherDataPath);
        if (!lines.Any())
        {
            Logger.Log($"No text found in {Paths.LauncherDataPath}");
            return lines;
        }

        Logger.Log($"{lines.Length} lines read from file");
        return lines;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LauncherMiddleware.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace WarhammerLauncherTool.Class;

public class Mod
{
    public string Uuid { get; set; }
    public int Order { get; set; }
    public bool Active { get; set; }
    public GameName Game { get; set; }
    public string PackFilePath { get; set; }
    public string Name { get; set; }
    public string Short { get; set; }
    public string Category { get; set; }
    public bool Owned { get; set; }

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = { new StringEnumConverter() }
    };

    public static MemoryStream ExportToStream(GameName gameName, string launcherDataPath, ILogger logger)
    {
        try
        {
            logger.Information("Exporting mod data to stream for {GameName}", gameName);

            var stream = File.Open(launcherDataPath, FileMode.Open);
            var mods = GetModsFromStream(stream, logger);
            var filteredMods = mods.Where(mod => mod.Game == gameName).ToList();
            var exportStream = ExportToStream(filteredMods, logger);

            return exportStream;
        }
        catch (Exception e)
        {
            logger.Error(e.Message, e);
            throw;
        }
    }

    public static MemoryStream ExportToStream(List<Mod> mods, ILogger logger)
    {
        logger.Information("Exporting mod data to stream for {ModsCount} mods", mods.Count);
        var stream = new MemoryStream();
        try
        {
            string? modString = JsonConvert.SerializeObject(mods, _jsonSerializerSettings);
            var writer = new StreamWriter(stream);
            writer.Write(modString);
            writer.Flush();

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            logger.Error(e, e.Message);
            throw;
        }
    }

    public static Mod[] GetModsFromStream(Stream stream, ILogger logger)
    {
        var serializer = JsonSerializer.CreateDefault();
        var extractedMods = Array.Empty<Mod>();

        using (var streamReader = new StreamReader(stream))
        using (var reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            while (reader.Read())
            {
                var data = serializer.Deserialize<Mod[]>(reader);
                if (data is null) logger.Information("No mods were deserialized !");
                else
                {
                    int oldLength = extractedMods.Length;
                    Array.Resize(ref extractedMods, oldLength + data.Length);
                    Array.Copy(data, 0, extractedMods, oldLength, data.Length);
                }
            }
        }

        return extractedMods;
    }
}

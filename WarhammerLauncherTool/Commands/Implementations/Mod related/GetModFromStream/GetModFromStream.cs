using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;

public class GetModFromStream : IGetModFromStream
{
    private ILogger Logger { get; }
    public GetModFromStream(ILogger logger) { Logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public Mod[] Execute(Stream stream)
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
                if (data is null) Logger.Information("No mods were deserialized !");
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

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.ModListToStream;

public class ModListToStream : IModListToStream
{
    public required ILogger _logger { get; init; }

    private JsonSerializerSettings? _jsonSerializerSettings;

    private JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            return _jsonSerializerSettings ??= new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };
        }
    }

    public MemoryStream Execute(List<Mod> mods)
    {
        var stream = new MemoryStream();
        try
        {
            string modString = JsonConvert.SerializeObject(mods, JsonSerializerSettings);
            var writer = new StreamWriter(stream);
            writer.Write(modString);
            writer.Flush();

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            _logger.Error(e, "An error has occured while exporting {Count} mods", mods.Count);
            throw;
        }
    }
}

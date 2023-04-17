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
    private ILogger Logger { get; }

    private readonly JsonSerializerSettings? _jsonSerializerSettings;

    public ModListToStream(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };
    }

    /// <summary>
    /// <see cref="IModListToStream" />
    /// </summary>
    /// <param name="mods"></param>
    /// <returns></returns>
    public MemoryStream Execute(List<Mod> mods)
    {
        var stream = new MemoryStream();
        try
        {
            string modString = JsonConvert.SerializeObject(mods, _jsonSerializerSettings);
            var writer = new StreamWriter(stream);
            writer.Write(modString);
            writer.Flush();

            return stream;
        }
        catch (Exception e)
        {
            stream.Dispose();
            Logger.Error(e, "An error has occured while exporting {Count} mods", mods.Count);
            throw;
        }
    }
}

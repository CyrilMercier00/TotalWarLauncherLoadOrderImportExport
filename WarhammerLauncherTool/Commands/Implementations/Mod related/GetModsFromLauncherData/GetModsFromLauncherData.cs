using System;
using System.IO;
using System.Linq;
using Serilog;
using WarhammerLauncherTool.Commands.Implementations.ModComands.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.ModComands.ModListToStream;

namespace WarhammerLauncherTool.Commands.Implementations.ModComands.GetModsFromFile;

public class GetModsFromLauncherData : IGetModsFromLauncherData
{
    private ILogger Logger { get; }
    private IGetModFromStream GetModFromStream { get; }
    private IModListToStream ModListToStream { get; }

    public GetModsFromLauncherData(ILogger logger, IGetModFromStream getModFromStream, IModListToStream modListToStream)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        GetModFromStream = getModFromStream ?? throw new ArgumentNullException(nameof(getModFromStream));
        ModListToStream = modListToStream ?? throw new ArgumentNullException(nameof(modListToStream));
    }

    public MemoryStream Execute(GetModsFromLauncherDataParameter parameters)
    {
        try
        {
            var stream = File.Open(parameters.FilePath, FileMode.Open);
            var mods = GetModFromStream.Execute(stream);
            var filteredMods = mods.Where(mod => mod.Game == parameters.Name).ToList();
            var exportStream = ModListToStream.Execute(filteredMods);

            return exportStream;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Unable to retrieve mods from file. GameName is {Name} and filepath is {Path}", parameters.Name, parameters.FilePath);
            throw;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Serilog;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.ModListToStream;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsForGame;

public class GetModsForGame : IGetModsForGame
{
    private ILogger Logger { get; }
    private IGetModFromStream GetModFromStream { get; }
    private IModListToStream ModListToStream { get; }

    public GetModsForGame(ILogger logger, IGetModFromStream getModFromStream, IModListToStream modListToStream)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        GetModFromStream = getModFromStream ?? throw new ArgumentNullException(nameof(getModFromStream));
        ModListToStream = modListToStream ?? throw new ArgumentNullException(nameof(modListToStream));
    }

    /// <summary>
    /// <see cref="IGetModsForGame" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public MemoryStream Execute(GetModsForGameParameter parameters)
    {
        try
        {
            var stream = File.Open(parameters.FilePath, FileMode.Open);
            var mods = GetModFromStream.Execute(stream);
            var filteredMods = mods.Where(mod => mod.Game == parameters.GameName).ToList();
            var exportStream = ModListToStream.Execute(filteredMods);

            return exportStream;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Unable to retrieve mods from {Path} for game {GameName}", parameters.GameName, parameters.FilePath);
            throw;
        }
    }
}

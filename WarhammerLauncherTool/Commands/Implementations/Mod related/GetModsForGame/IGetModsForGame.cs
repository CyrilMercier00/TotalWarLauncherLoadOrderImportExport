using System.IO;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsForGame;

/// <summary>
/// Reads a stream and retrieve the mods for a game
/// </summary>
public interface IGetModsForGame : ICommand<MemoryStream, GetModsForGameParameter> {}

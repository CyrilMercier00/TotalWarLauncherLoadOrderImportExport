using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsForGame;

public class GetModsForGameParameter
{
    public required GameName GameName { get; init; }
    public required string FilePath { get; init; }
}

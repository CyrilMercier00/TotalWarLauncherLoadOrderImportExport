using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;

public class GetModsFromLauncherDataParameter
{
    public required GameName GameName { get; init; }
    public required string FilePath { get; init; }
}

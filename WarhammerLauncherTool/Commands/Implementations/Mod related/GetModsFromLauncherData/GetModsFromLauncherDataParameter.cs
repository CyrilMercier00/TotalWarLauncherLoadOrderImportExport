using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.ModComands.GetModsFromFile;

public class GetModsFromLauncherDataParameter
{
    public GameName Name { get; set; }
    public string FilePath { get; set; }
}

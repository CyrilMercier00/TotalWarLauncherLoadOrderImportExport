using System.Collections.Generic;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;

public class SaveModsToFileParameters
{
    public required string savePath { get; init; }
    public required List<Mod> mods { get; init; }
}

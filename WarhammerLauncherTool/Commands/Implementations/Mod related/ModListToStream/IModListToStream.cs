using System.Collections.Generic;
using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.ModListToStream;

/// <summary>
/// Serialize <see cref="Mod" /> into a MemoryStream>
/// </summary>
/// <returns></returns>
public interface IModListToStream : ICommand<MemoryStream, List<Mod>> {}

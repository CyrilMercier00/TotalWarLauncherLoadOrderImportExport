using System.Collections.Generic;
using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.ModComands.ModListToStream;

public interface IModListToStream : ICommand<List<Mod>, MemoryStream> {}

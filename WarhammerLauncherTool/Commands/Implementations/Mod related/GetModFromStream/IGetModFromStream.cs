using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;

public interface IGetModFromStream : ICommand<Stream, Mod[]> {}

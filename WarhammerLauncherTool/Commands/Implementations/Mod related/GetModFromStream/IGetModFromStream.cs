using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.ModComands.GetModFromStream;

public interface IGetModFromStream : ICommand<Stream, Mod[]> {}

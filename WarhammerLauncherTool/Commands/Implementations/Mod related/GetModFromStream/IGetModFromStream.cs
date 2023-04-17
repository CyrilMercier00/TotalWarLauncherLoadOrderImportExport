using System.IO;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;

/// <summary>
/// Read a .json file and deserialiaze <see cref="Mod" /> objects fron it
/// </summary>
/// <returns></returns>
public interface IGetModFromStream : ICommand<Mod[], Stream> {}

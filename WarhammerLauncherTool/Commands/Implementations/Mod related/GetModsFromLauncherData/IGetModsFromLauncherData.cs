using System.IO;

namespace WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;

public interface IGetModsFromLauncherData : ICommand<MemoryStream, GetModsFromLauncherDataParameter> {}

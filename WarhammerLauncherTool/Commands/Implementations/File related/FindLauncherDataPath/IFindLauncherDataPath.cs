namespace WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;

/// <summary>
/// Attempts to retrieve the path of a file containing launcher data from the user's application data folder.
/// It will log a warning if the file is not found.
/// </summary>
/// <returns></returns>
public interface IFindLauncherDataPath : ICommand<string> {}

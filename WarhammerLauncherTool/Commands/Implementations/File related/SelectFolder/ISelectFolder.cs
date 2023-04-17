namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;

/// <summary>
/// This method is used to open a Windows dialogue box for the user to select a folder.
/// </summary>
/// <returns>The selected folder fullpath as a string.</returns>
public interface ISelectFolder : ICommand<string, SelectFolderParameters> {}

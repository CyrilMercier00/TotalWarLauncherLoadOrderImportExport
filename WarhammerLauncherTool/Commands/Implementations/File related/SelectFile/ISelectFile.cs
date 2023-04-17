namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;

/// <summary>
/// Opens a file dialogue window to select a file with either a .json or .txt extension.
/// </summary>
/// <returns> The path of the file is returned, otherwise an empty string is returned. </returns>
public interface ISelectFile : ICommand<string, SelectFileParameters> {}

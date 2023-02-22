using Microsoft.WindowsAPICodePack.Dialogs;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;

public class SelectFolder : ISelectFolder
{
    /// <summary>
    /// This method is used to open a Windows dialogue box for the user to select a folder.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>The selected folder fullpath as a string.</returns>
    public string Execute(SelectFolderParameters parameters)
    {
        var dialogue = new CommonOpenFileDialog
        {
            AddToMostRecentlyUsedList = false,
            AllowNonFileSystemItems = false,
            DefaultDirectory = parameters.StartingDirectory,
            EnsureFileExists = true,
            EnsurePathExists = true,
            EnsureReadOnly = false,
            EnsureValidNames = true,
            InitialDirectory = parameters.StartingDirectory,
            IsFolderPicker = true,
            Multiselect = false,
            ShowPlacesList = true,
            Title = parameters.ModalTitle
        };

        string folder = string.Empty;
        if (dialogue.ShowDialog() == CommonFileDialogResult.Ok) folder = dialogue.FileName;

        return folder;
    }
}

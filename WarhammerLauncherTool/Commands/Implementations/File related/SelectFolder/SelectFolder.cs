using Microsoft.WindowsAPICodePack.Dialogs;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;

public class SelectFolder : ISelectFolder
{
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

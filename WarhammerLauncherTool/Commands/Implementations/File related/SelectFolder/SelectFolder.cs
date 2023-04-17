using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;

public class SelectFolder : ISelectFolder
{
    private readonly ILogger _logger;
    public SelectFolder(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    /// <summary>
    /// see : <see cref="ISelectFolder" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>The selected folder fullpath as a string.</returns>
    public string Execute(SelectFolderParameters parameters)
    {
        try
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
        catch (Exception e)
        {
            _logger.Error(e, "Unable to open a folder select dialog to {Directory}", parameters.StartingDirectory);
            throw;
        }
    }
}

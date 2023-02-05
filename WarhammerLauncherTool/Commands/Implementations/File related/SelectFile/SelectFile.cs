using System;
using Microsoft.Win32;
using Serilog;

namespace WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;

public class SelectFile : ISelectFile
{
    private readonly ILogger _logger;

    public SelectFile(ILogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public string Execute(SelectFileParameters parameters)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt",
                InitialDirectory = parameters.StartingDirectory,
                Title = parameters.ModalTitle
            };

            bool? result = dialog.ShowDialog();

            return result is null or false ? string.Empty : dialog.FileName;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unable to display dialog, starting directory : {Directory}", parameters.StartingDirectory);
            throw;
        }
    }
}

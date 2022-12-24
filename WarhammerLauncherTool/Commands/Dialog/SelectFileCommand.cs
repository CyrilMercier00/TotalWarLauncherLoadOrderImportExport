using System.Windows.Input;
using Microsoft.Win32;

public class SelectFileCommand : ICommand
{
    public string Execute(string modalTitle, string startingDirectory)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt",
            InitialDirectory = startingDirectory,
            Title = modalTitle
        };

        bool? result = dialog.ShowDialog();

        return result is null or false ? string.Empty : dialog.FileName;
    }
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImportExportInterface;

internal static class FileUtilities
{
    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";
    public static readonly Guid RoamingFolderGuid = new("374DE290-123F-4565-9164-39C4925E467B");

    public static string FindLauncherDataPath(Logger.Logger? logger)
    {
        logger?.Log("Retrieving launcherDataPath");

        string? appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string? launcherFolder = appdataFolder + DefaultLauncherFolderPath;
        string? path = launcherFolder + DefaultLauncherDataFilename;

        logger?.Log($"Launcher data path generated: {path}");

        if (File.Exists(path)) return path;

        logger?.Log("Unable to find the file containing the launcher data");
        return string.Empty;
    }

    public static string SelectFile(string modalTitle, string startingDirectory)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt",
            InitialDirectory = startingDirectory,
            Title = modalTitle
        };

        dialog.ShowDialog();
        return string.IsNullOrEmpty(dialog.FileName) ? string.Empty : dialog.FileName;
    }

    public static string SelectFolder(string modalTitle, string startingDirectory)
    {
        var dialogue = new CommonOpenFileDialog
        {
            AddToMostRecentlyUsedList = false,
            AllowNonFileSystemItems = false,
            DefaultDirectory = startingDirectory,
            EnsureFileExists = true,
            EnsurePathExists = true,
            EnsureReadOnly = false,
            EnsureValidNames = true,
            InitialDirectory = startingDirectory,
            IsFolderPicker = true,
            Multiselect = false,
            ShowPlacesList = true,
            Title = modalTitle
        };

        string? folder = string.Empty;
        if (dialogue.ShowDialog() == CommonFileDialogResult.Ok) folder = dialogue.FileName;

        return folder;
    }

    [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    public static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = 0);
}

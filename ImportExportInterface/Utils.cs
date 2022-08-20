using System;
using System.IO;
using Logging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImportExportInterface;

internal static class Utils
{
    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";

    public static string FindLauncherDataPath (Logger? logger)
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

    public static string SelectFolder (string modalTitle, string startingDirectory)
    {
        var dlg = new CommonOpenFileDialog
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
        if (dlg.ShowDialog() == CommonFileDialogResult.Ok) folder = dlg.FileName;

        return folder;
    }
}

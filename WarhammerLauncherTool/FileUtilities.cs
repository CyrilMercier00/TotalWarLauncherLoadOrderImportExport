using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;

namespace WarhammerLauncherTool;

internal static class FileUtilities
{
    private const string DefaultLauncherFolderPath = "\\The Creative Assembly\\Launcher\\";
    private const string DefaultLauncherDataFilename = "20190104-moddata.dat";
    public static readonly Guid RoamingFolderGuid = new("374DE290-123F-4565-9164-39C4925E467B");

    public static ILogger Logger { get; set; }

    public static string FindLauncherDataPath()
    {
        Logger.Information("Retrieving launcherDataPath");

        string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string launcherFolder = $"{appdataFolder}{DefaultLauncherFolderPath}";
        string path = $"{launcherFolder}{DefaultLauncherDataFilename}";

        Logger.Information("Launcher data path generated: {Path}", path);

        if (File.Exists(path)) return path;

        Logger.Warning("Unable to find the file containing the launcher data");
        return string.Empty;
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

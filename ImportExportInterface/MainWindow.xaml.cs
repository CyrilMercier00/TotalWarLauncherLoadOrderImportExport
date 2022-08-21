using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using LauncherMiddleware;
using LauncherMiddleware.Models;
using Logging;
using Newtonsoft.Json;

namespace ImportExportInterface;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string ExportFileName = "\\loadOrder.json";
    private const GameName SelectedGame = GameName.warhammer3; // TODO : Implement game selection
    private readonly string _launcherDataPath;
    private readonly Logger _logger;

    public MainWindow ()
    {
        InitializeComponent();

        // Inintalize logginng
        _logger = new Logger();
        Logger.Toggled = true;

        // Get launcher data file
        _launcherDataPath = Utils.FindLauncherDataPath(_logger);

        if (!string.IsNullOrEmpty(_launcherDataPath)) return;

        string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Utils.SelectFolder("roaming", roaming);
        _launcherDataPath = folder;
    }

    [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath ([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = 0);

    /// <summary>
    /// Exprort to file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonExport_Click (object sender, RoutedEventArgs e)
    {
        string dlFolder = SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0);
        string savePath = Utils.SelectFolder("Save exported file", dlFolder);

        var stream = Export.CreateFile(SelectedGame, _launcherDataPath, _logger);
        var fileStream = File.Create(savePath + ExportFileName);

        stream.Position = 0;
        stream.CopyTo(fileStream);
        fileStream.Close();
    }

    /// <summary>
    /// Import to launcher
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonImport_Click (object sender, RoutedEventArgs e)
    {
        // Select import file
        string dlFolder = SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0);
        string savePath = Utils.SelectFolder("Select the exported load order to import", dlFolder);

        // Extract mods from file
        var importedMods = new List<Mod>();
        using (var stream = File.Open(savePath, FileMode.Open))
        {
            var mods = Commons.GetModsFromStream(stream, _logger);
            if (!mods.Any())
            {
                DisplayMessage("No mods could be found in the selected file");
                return;
            }

            importedMods.AddRange(mods);
        }

        // Retrieve existing mods
        using var fileStream = File.Open(_launcherDataPath, FileMode.Open);
        var launcherModsConfig = Commons.GetModsFromStream(fileStream, _logger);

        // Filter out other games
        var isForCurrentGame = launcherModsConfig.ToLookup(mod => mod.Game == SelectedGame);
        var modsCurrent = isForCurrentGame[true].ToList();
        var modsOthers = isForCurrentGame[false].ToList();

        // Filter out mods that were in the config before the import
        var existingMods = modsCurrent.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid == existingMod.Uuid)).ToList();
        var missingMods = modsCurrent.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid != existingMod.Uuid)).ToList();

        // Subscribe to missing mods
        foreach (var mod in missingMods)
        {
            // TODO 
        }

        // Get mods that are already present and change thet status
        foreach (var mod in existingMods)
        {
            var importedMod = importedMods.Single(m => m.Uuid == mod.Uuid);
            mod.Active = importedMod.Active;
            mod.Order = importedMod.Order;
        }

        // Build new modList
        modsOthers.AddRange(modsCurrent);

        // Backup old config        
        string backupName = string.Format(_launcherDataPath, ".backup");
        File.Copy(_launcherDataPath, backupName);

        // Write to file
        using (var newFileStream = File.Open(_launcherDataPath, FileMode.Create))
        {
            string serializedData = JsonConvert.SerializeObject(modsOthers);
            using var writer = new StreamWriter(newFileStream);
            writer.Write(serializedData);
            writer.Close();
        }
    }

    private static void DisplayMessage (string message)
    {
        // TODO
    }
}

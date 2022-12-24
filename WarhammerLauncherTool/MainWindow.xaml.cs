using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LauncherMiddleware.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using WarhammerLauncherTool.Class;

namespace WarhammerLauncherTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string ExportFileName = "\\loadOrder.json";
    private const GameName SelectedGame = GameName.warhammer3; // TODO : Implement game selection
    private readonly string _launcherData;
    private readonly FileInfo _launcherDataInfo;
    private readonly Logger _logger;

    public MainWindow()
    {
        InitializeComponent();

        // Inintalize logginng
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/log-{Date}.txt", retainedFileCountLimit: 20)
            .CreateLogger();

        // Get launcher data file
        _launcherData = FileUtilities.FindLauncherDataPath(_logger);

        // Ask to select the launcher data folder if it wasn't found
        if (string.IsNullOrEmpty(_launcherData))
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DisplayMessage("Select the folder containing the data for the launcher");
            string folder = FileUtilities.SelectFolder("", roaming);
            _launcherData = folder;
        }

        _launcherDataInfo = new FileInfo(_launcherData);
    }

    /// <summary>
    /// Exprort to file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonExport_Click(object sender, RoutedEventArgs e)
    {
        string dlFolder = FileUtilities.SHGetKnownFolderPath(FileUtilities.RoamingFolderGuid, 0);
        DisplayMessage("Select the folder where the file will be saved");
        string savePath = FileUtilities.SelectFolder("", dlFolder);
        if (string.IsNullOrEmpty(savePath)) return;

        var stream = Mod.ExportToStream(SelectedGame, _launcherData, _logger);
        var fileStream = File.Create(savePath + ExportFileName);

        stream.Position = 0;
        fileStream.Position = 0;

        stream.CopyTo(fileStream);
        fileStream.Close();
    }

    /// <summary>
    /// Import to launcher
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonImport_Click(object sender, RoutedEventArgs e)
    {
        // Select import file
        string dlFolder = FileUtilities.SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0);
        DisplayMessage("Select the exported load order to import");
        string savePath = Mod.SelectFile("", dlFolder);
        if (string.IsNullOrEmpty(savePath)) return;

        // Extract mods from file
        var importedMods = new List<Mod>();
        using (var stream = File.Open(savePath, FileMode.Open))
        {
            var mods = Mod.GetModsFromStream(stream, _logger);
            if (!mods.Any())
            {
                DisplayMessage("No mods could be found in the selected file");
                return;
            }

            importedMods.AddRange(mods);
        }

        // Reorder mods that were already downloaded to fill gaps in the order
        importedMods = importedMods.OrderBy(mod => mod.Order).ToList();
        for (int i = 0; i < importedMods.Count; i++) importedMods[i].Order = i;

        // Retrieve existing mods
        using var fileStream = File.Open(_launcherData, FileMode.Open);
        var launcherModsConfig = Mod.GetModsFromStream(fileStream, _logger);

        // Filter out other games
        var isForCurrentGame = launcherModsConfig.ToLookup(mod => mod.Game == SelectedGame);
        var modsForCurrentGame = isForCurrentGame[true].ToList();
        var modsForOtherGames = isForCurrentGame[false].ToList();

        // Separate mods that are in the import but are not downloaded
        var alreadyDownloaded = modsForCurrentGame.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid == existingMod.Uuid)).ToList();
        var notDownloaded = modsForCurrentGame.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid != existingMod.Uuid)).ToList();

        // Subscribe to missing mods
        foreach (var mod in notDownloaded)
        {
            // TODO 
        }

        // Update mods that were already downloaded
        foreach (var mod in alreadyDownloaded)
        {
            var importedMod = importedMods.Single(m => m.Uuid == mod.Uuid);
            mod.Active = importedMod.Active;
            mod.Order = importedMod.Order;
        }

        // Filter out mods that are installed but not present in the import
        var modsNotInImport = modsForCurrentGame.Where(mod => importedMods.Exists(existingMod => existingMod.Uuid == mod.Uuid)).OrderBy(mods => mods.Order).ToList();
        for (int i = 0; i < modsNotInImport.Count; i++)
        {
            modsNotInImport[i].Active = false;
            modsNotInImport[i].Order = importedMods.Count + i;
        }

        // Build new modList
        var newModList = new List<Mod>();
        newModList.AddRange(alreadyDownloaded);
        newModList.AddRange(notDownloaded);
        newModList.AddRange(modsNotInImport);
        newModList.AddRange(modsForOtherGames);

        // Backup old config
        string mainDir = _launcherDataInfo.Directory!.FullName;
        string backupDir = string.Concat(mainDir, "/ImportBackups");
        Directory.CreateDirectory(backupDir);

        string date = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        string backupName = string.Concat(backupDir, _launcherDataInfo.Name, ".backup-", date);
        File.Copy(_launcherData, backupName);

        // Write to file
        using (var newFileStream = File.Open(_launcherData, FileMode.Create))
        {
            string serializedData = JsonConvert.SerializeObject(newModList);
            using var writer = new StreamWriter(newFileStream);
            writer.Write(serializedData);
            writer.Close();
        }
    }

    private static void DisplayMessage(string message)
    {
        // TODO
    }
}

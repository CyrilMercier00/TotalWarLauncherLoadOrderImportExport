using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Serilog;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;
using WarhammerLauncherTool.Commands.Implementations.Steam_related.GetModFromStream;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string ExportFileName = @"\ExportedLoadOrder.json";

    private readonly GameName SelectedGame = GameName.Warhammer3; // TODO : Implement game selection
    private readonly FileInfo _launcherDataInfo;

    private readonly string _desktopFolder;
    private readonly string _launcherData;

    private readonly ISelectFile _selectFile;
    private readonly ISelectFolder _selectFolder;
    private readonly IGetModFromStream _getModsFromStream;
    private readonly ISubscribeToWorkshopItems _subscribeToMods;
    private readonly IFindLauncherDataPath _findLauncherDataPath;
    private readonly IGetModsFromLauncherData _getModsFromLauncherData;

    private readonly ILogger _logger;

    public MainWindow(
        ILogger logger,
        ISelectFile selectFile,
        ISelectFolder selectFolder,
        IGetModFromStream getModsFromStream,
        IFindLauncherDataPath findLauncherDataPath,
        IGetModsFromLauncherData getModsFromLauncherData,
        ISubscribeToWorkshopItems subscribeToWorkshopItems)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initalize all commands
        _selectFile = selectFile ?? throw new ArgumentNullException(nameof(selectFile));
        _selectFolder = selectFolder ?? throw new ArgumentNullException(nameof(selectFolder));
        _getModsFromStream = getModsFromStream ?? throw new ArgumentNullException(nameof(getModsFromStream));
        _findLauncherDataPath = findLauncherDataPath ?? throw new ArgumentNullException(nameof(findLauncherDataPath));
        _getModsFromLauncherData = getModsFromLauncherData ?? throw new ArgumentNullException(nameof(getModsFromLauncherData));
        _subscribeToMods = subscribeToWorkshopItems ?? throw new ArgumentNullException(nameof(subscribeToWorkshopItems));

        InitializeComponent();

        _launcherData = _findLauncherDataPath.Execute();
        _desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Ask to manually select the launcher data folder if it was not found
        if (string.IsNullOrEmpty(_launcherData))
        {
            DisplayMessage("Select the file containing the data for the launcher. The file you are looking for is named loadOrder.json");

            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var parameters = new SelectFileParameters { ModalTitle = string.Empty, StartingDirectory = roamingFolder };
            string path = _selectFile.Execute(parameters);

            _launcherData = path;
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
        DisplayMessage("Select the folder where the file will be saved");

        var parameters = new SelectFolderParameters { ModalTitle = string.Empty, StartingDirectory = _desktopFolder };
        string savePath = _selectFolder.Execute(parameters);

        if (string.IsNullOrEmpty(savePath)) return;

        var param = new GetModsFromLauncherDataParameter { GameName = SelectedGame, FilePath = _launcherData };
        var stream = _getModsFromLauncherData.Execute(param);
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
        DisplayMessage("Select the exported load order to import");

        var parameters = new SelectFileParameters { ModalTitle = string.Empty, StartingDirectory = _desktopFolder };
        string savePath = _selectFile.Execute(parameters);

        if (string.IsNullOrEmpty(savePath)) return;

        // Extract mods from file
        var importedMods = new List<Mod>();
        using (var stream = File.Open(savePath, FileMode.Open))
        {
            var mods = _getModsFromStream.Execute(stream);
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
        var launcherModsConfig = _getModsFromStream.Execute(fileStream);

        // Filter out other games
        var isForCurrentGame = launcherModsConfig.ToLookup(mod => mod.Game == SelectedGame);
        var modsForCurrentGame = isForCurrentGame[true].ToList();
        var modsForOtherGames = isForCurrentGame[false].ToList();

        // Separate mods that are in the import but are not downloaded
        var alreadyDownloaded = modsForCurrentGame.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid == existingMod.Uuid)).ToList();
        var notDownloaded = modsForCurrentGame.Where(existingMod => importedMods.Any(importedMod => importedMod.Uuid != existingMod.Uuid)).ToList();

        // Subscribe to missing mods
        ulong[] modUuids = notDownloaded.Select(m => m.workshopUid).ToArray();
        _subscribeToMods.ExecuteAsync(modUuids); // TODO : Pass non subbed items instead

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

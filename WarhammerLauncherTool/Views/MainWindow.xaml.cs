using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Serilog;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;
using WarhammerLauncherTool.Commands.Implementations.Steam_related.GetSteamWorkshopItems;
using WarhammerLauncherTool.Commands.Implementations.Steam_related.SubscribeToWorkshopItems;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Views;

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
    private readonly ISaveModsToFile _saveModsToFile;
    private readonly IGetModFromStream _getModsFromStream;
    private readonly IFindLauncherDataPath _findLauncherDataPath;
    private readonly IGetSteamWorkshopItems _getSteamWorkshopItems;
    private readonly IGetModsFromLauncherData _getModsFromLauncherData;
    private readonly ISubscribeToWorkshopItems _subscribeToWorkshopItems;

    private readonly ILogger _logger;

    public MainWindow(
        ILogger logger,
        ISelectFile selectFile,
        ISelectFolder selectFolder,
        ISaveModsToFile saveModsToFile,
        IGetModFromStream getModsFromStream,
        IFindLauncherDataPath findLauncherDataPath,
        IGetSteamWorkshopItems getSteamWorkshopItems,
        IGetModsFromLauncherData getModsFromLauncherData,
        ISubscribeToWorkshopItems subscribeToWorkshopItems)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initalize all commands
        _selectFile = selectFile ?? throw new ArgumentNullException(nameof(selectFile));
        _selectFolder = selectFolder ?? throw new ArgumentNullException(nameof(selectFolder));
        _saveModsToFile = saveModsToFile ?? throw new ArgumentNullException(nameof(saveModsToFile));
        _getModsFromStream = getModsFromStream ?? throw new ArgumentNullException(nameof(getModsFromStream));
        _findLauncherDataPath = findLauncherDataPath ?? throw new ArgumentNullException(nameof(findLauncherDataPath));
        _getSteamWorkshopItems = getSteamWorkshopItems ?? throw new ArgumentNullException(nameof(getSteamWorkshopItems));
        _getModsFromLauncherData = getModsFromLauncherData ?? throw new ArgumentNullException(nameof(getModsFromLauncherData));
        _subscribeToWorkshopItems = subscribeToWorkshopItems ?? throw new ArgumentNullException(nameof(subscribeToWorkshopItems));

        InitializeComponent();

        _launcherData = _findLauncherDataPath.Execute();
        _desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Ask to manually select the launcher data folder if it was not found
        if (string.IsNullOrEmpty(_launcherData))
        {
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
    private async void ButtonImport_Click(object sender, RoutedEventArgs e)
    {
        // Select import file
        var parameters = new SelectFileParameters { ModalTitle = string.Empty, StartingDirectory = _desktopFolder };
        string savePath = _selectFile.Execute(parameters);

        if (string.IsNullOrEmpty(savePath)) return;

        // Extract mods from file
        var importedMods = new List<Mod>();
        await using (var stream = File.Open(savePath, FileMode.Open))
        {
            var mods = _getModsFromStream.Execute(stream);
            if (!mods.Any()) return;

            importedMods.AddRange(mods);
        }

        // Reorder mods that were already downloaded to fill gaps in the order
        importedMods = importedMods.OrderBy(mod => mod.Order).ToList();
        for (int i = 0; i < importedMods.Count; i++) importedMods[i].Order = i;

        // Retrieve existing mods
        await using var fileStream = File.Open(_launcherData, FileMode.Open);
        var launcherModsConfig = _getModsFromStream.Execute(fileStream);

        // Filter out other games
        var isForCurrentGame = launcherModsConfig.ToLookup(mod => mod.Game == SelectedGame);
        var modsForCurrentGame = isForCurrentGame[true].ToList();
        var modsForOtherGames = isForCurrentGame[false].ToList();

        // Login to steam account
        // TODO

        // Get mods that the user is not subscribed to
        var uids = modsForCurrentGame.Select(mod => mod.workshopUid).ToList();
        var workshopItems = await _getSteamWorkshopItems.ExecuteAsync(uids).ConfigureAwait(false);

        // Subscribe to missing mods
        await _subscribeToWorkshopItems.ExecuteAsync(workshopItems).ConfigureAwait(false);

        // Filter out mods that are installed but not present in the import
        var modsNotInImport = modsForCurrentGame.Where(mod => importedMods.Exists(existingMod => existingMod.Uuid == mod.Uuid)).OrderBy(mods => mods.Order).ToList();
        for (int i = 0; i < modsNotInImport.Count; i++)
        {
            modsNotInImport[i].Active = false;
            modsNotInImport[i].Order = importedMods.Count + i;
        }

        // Build new modList
        var newModList = new List<Mod>();
        newModList.AddRange(modsForCurrentGame);
        newModList.AddRange(modsForOtherGames);
        newModList.AddRange(modsNotInImport);

        // Backup old config
        string mainDir = _launcherDataInfo.Directory!.FullName;
        string backupDir = string.Concat(mainDir, "/ImportBackups");
        Directory.CreateDirectory(backupDir);

        string date = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        string backupName = string.Concat(backupDir, _launcherDataInfo.Name, ".backup-", date);
        File.Copy(_launcherData, backupName);

        // Save to file
        var saveToFileParameters = new SaveModsToFileParameters { mods = newModList, savePath = _launcherData };
        _saveModsToFile.Execute(saveToFileParameters);
    }
}

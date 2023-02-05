using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Newtonsoft.Json;
using Serilog;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string ExportFileName = "\\loadOrder.json";
    private const GameName SelectedGame = GameName.Warhammer3; // TODO : Implement game selection
    private readonly string _launcherData;
    private readonly FileInfo _launcherDataInfo;

    private readonly ILogger _logger;

    private readonly ISelectFile _selectFile;
    private readonly ISelectFolder _selectFolder;
    private readonly IGetModFromStream _getModsFromStream;
    private readonly IFindLauncherDataPath _findLauncherDataPath;
    private readonly IGetModsFromLauncherData _getModsFromLauncherData;

    public MainWindow(
        ILogger logger,
        ISelectFile selectFile,
        ISelectFolder selectFolder,
        IGetModFromStream getModsFromStream,
        IFindLauncherDataPath findLauncherDataPath,
        IGetModsFromLauncherData getModsFromLauncherData)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initalize all commands
        _selectFile = selectFile ?? throw new ArgumentNullException(nameof(selectFile));
        _selectFolder = selectFolder ?? throw new ArgumentNullException(nameof(selectFolder));
        _getModsFromStream = getModsFromStream ?? throw new ArgumentNullException(nameof(getModsFromStream));
        _findLauncherDataPath = findLauncherDataPath ?? throw new ArgumentNullException(nameof(findLauncherDataPath));
        _getModsFromLauncherData = getModsFromLauncherData ?? throw new ArgumentNullException(nameof(getModsFromLauncherData));

        InitializeComponent();

        // Get launcher data file
        _launcherData = _findLauncherDataPath.Execute();

        // Ask to select the launcher data folder if it was not found
        if (string.IsNullOrEmpty(_launcherData))
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DisplayMessage("Select the folder containing the data for the launcher");

            var parameters = new SelectFolderParameters { ModalTitle = string.Empty, StartingDirectory = roaming };
            string folder = _selectFolder.Execute(parameters);
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
        DisplayMessage("Select the folder where the file will be saved");

        string dlFolder = SHGetKnownFolderPath(Constants.RoamingFolderGuid, 0);
        var parameters = new SelectFolderParameters { ModalTitle = string.Empty, StartingDirectory = dlFolder };
        string savePath = _selectFolder.Execute(parameters);

        if (string.IsNullOrEmpty(savePath)) return;

        var param = new GetModsFromLauncherDataParameter { Name = SelectedGame, FilePath = _launcherData };
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

        string dlFolder = SHGetKnownFolderPath(Constants.RoamingFolderGuid, 0);
        var parameters = new SelectFileParameters { ModalTitle = string.Empty, StartingDirectory = dlFolder };
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

    [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    public static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = 0);
}

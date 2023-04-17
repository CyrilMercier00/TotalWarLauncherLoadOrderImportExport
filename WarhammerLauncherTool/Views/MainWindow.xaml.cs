using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsForGame;
using WarhammerLauncherTool.Models;

namespace WarhammerLauncherTool.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string ExportFileName = @"\ExportedLoadOrder.json";

    private const GameName SelectedGame = GameName.Warhammer3;
    private readonly FileInfo _launcherDataInfo;

    private readonly string _desktopFolder;
    private readonly string _launcherData;

    private readonly ISelectFile _selectFile;
    private readonly ISelectFolder _selectFolder;
    private readonly ISaveModsToFile _saveModsToFile;
    private readonly IGetModFromStream _getModsFromStream;
    private readonly IGetModsForGame _getModsForGame;

    public MainWindow(
        ISelectFile selectFile,
        ISelectFolder selectFolder,
        ISaveModsToFile saveModsToFile,
        IGetModFromStream getModsFromStream,
        IFindLauncherDataPath findLauncherDataPath,
        IGetModsForGame getModsForGame)
    {
        // Initalize all commands
        _selectFile = selectFile ?? throw new ArgumentNullException(nameof(selectFile));
        _selectFolder = selectFolder ?? throw new ArgumentNullException(nameof(selectFolder));
        _saveModsToFile = saveModsToFile ?? throw new ArgumentNullException(nameof(saveModsToFile));
        _getModsFromStream = getModsFromStream ?? throw new ArgumentNullException(nameof(getModsFromStream));
        _getModsForGame = getModsForGame ?? throw new ArgumentNullException(nameof(getModsForGame));

        if (findLauncherDataPath is null) throw new ArgumentNullException(nameof(findLauncherDataPath));

        InitializeComponent();

        // Try to read the 
        _launcherData = findLauncherDataPath.Execute();
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

        var param = new GetModsForGameParameter { GameName = SelectedGame, FilePath = _launcherData };
        using (var stream = _getModsForGame.Execute(param))
        {
            using (var fileStream = File.Create(savePath + ExportFileName))
            {
                stream.Position = 0;
                fileStream.Position = 0;

                stream.CopyTo(fileStream);
                fileStream.Close();
            }
        }
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WarhammerLauncherTool.Commands.Implementations.File_related.BackupFile;
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

    private readonly string _desktopFolder;
    private readonly string _launcherData;

    private readonly ISelectFile _selectFile;
    private readonly IBackupFile _backupFile;
    private readonly ISelectFolder _selectFolder;
    private readonly ISaveModsToFile _saveModsToFile;
    private readonly IGetModFromStream _getModsFromStream;
    private readonly IGetModsForGame _getModsForGame;

    public MainWindow(
        ISelectFile selectFile,
        IBackupFile backupFile,
        ISelectFolder selectFolder,
        ISaveModsToFile saveModsToFile,
        IGetModsForGame getModsForGame,
        IGetModFromStream getModsFromStream,
        IFindLauncherDataPath findLauncherDataPath)
    {
        // Initalize all commands
        _selectFile = selectFile ?? throw new ArgumentNullException(nameof(selectFile));
        _selectFolder = selectFolder ?? throw new ArgumentNullException(nameof(selectFolder));
        _saveModsToFile = saveModsToFile ?? throw new ArgumentNullException(nameof(saveModsToFile));
        _getModsFromStream = getModsFromStream ?? throw new ArgumentNullException(nameof(getModsFromStream));
        _getModsForGame = getModsForGame ?? throw new ArgumentNullException(nameof(getModsForGame));
        _backupFile = backupFile ?? throw new ArgumentNullException(nameof(backupFile));

        if (findLauncherDataPath is null) throw new ArgumentNullException(nameof(findLauncherDataPath));

        InitializeComponent();

        // Try to read the 
        _launcherData = findLauncherDataPath.Execute();
        _desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Ask to manually select the launcher data folder if it was not found
        if (!string.IsNullOrEmpty(_launcherData)) return;

        string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var parameters = new SelectFileParameters { ModalTitle = string.Empty, StartingDirectory = roamingFolder };
        string path = _selectFile.Execute(parameters);

        _launcherData = path;
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

        // Retrieve existing mods
        await using var fileStream = File.Open(_launcherData, FileMode.Open);
        var launcherModsConfig = _getModsFromStream.Execute(fileStream);

        // Filter out other games
        var isForCurrentGame = launcherModsConfig.ToLookup(mod => mod.Game == SelectedGame);
        var modsForCurrentGame = isForCurrentGame[true].ToList();
        var modsForOtherGames = isForCurrentGame[false].ToList();

        // Filter out mods that are installed but not present in the import, and turn them off
        var modsNotInImport = modsForCurrentGame.Where(mod => !importedMods.Exists(existingMod => existingMod.Uuid == mod.Uuid)).ToList();
        for (int i = 0; i < modsNotInImport.Count; i++)
        {
            modsNotInImport[i].Active = false;
            modsNotInImport[i].Order = importedMods.Count + i;
        }

        // Remove from og list mods that are already in import to avoid duplicates
        foreach (var importedMod in importedMods)
        {
            foreach (var launcherMod in modsForCurrentGame)
            {
                if (launcherMod.Uuid != importedMod.Uuid) continue;

                launcherMod.Active = importedMod.Active;
                launcherMod.Order = importedMod.Order;
            }
        }

        // Build new modList
        var newModList = new List<Mod>();
        newModList.AddRange(modsForCurrentGame);
        newModList.AddRange(modsForOtherGames);
        newModList.AddRange(modsNotInImport);

        // Backup old config
        _backupFile.Execute(_launcherData);

        // Replace old launcher data
        var saveToFileParameters = new SaveModsToFileParameters { Mods = newModList, SavePath = _launcherData };
        _saveModsToFile.Execute(saveToFileParameters);
    }
}

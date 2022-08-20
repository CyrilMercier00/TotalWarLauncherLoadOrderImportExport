using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImportExportInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ExportFileName = "\\ExportedLoadOrder.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            var savePath = SelectFolder("Save exported file");
            var stream = LauncherMiddleware.Export.ExportMods(LauncherMiddleware.GameName.warhammer3);
            var fileStream = File.Create(savePath + ExportFileName);
            
            stream.Position = 0;
            stream.CopyTo(fileStream);
            fileStream.Close();
        }


        private static string SelectFolder(string modalTitle)
        {
            var dlFolder = SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0);

            var dlg = new CommonOpenFileDialog
            {
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = dlFolder,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                InitialDirectory = dlFolder,
                IsFolderPicker = true,
                Multiselect = false,
                ShowPlacesList = true,
                Title = modalTitle,
            };

            var folder = string.Empty;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folder = dlg.FileName;
            }

            return folder;
        }


        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = 0);
    }
}
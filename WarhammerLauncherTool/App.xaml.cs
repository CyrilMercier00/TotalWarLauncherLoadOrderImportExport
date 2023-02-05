using System.Windows;
using Autofac;

namespace WarhammerLauncherTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var container = AutofacContainer.Initialize();
        using (var scope = container.BeginLifetimeScope())
        {
            var mainWindow = scope.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}

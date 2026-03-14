using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SlayTheSpire2ModManager.App.ViewModels;
using SlayTheSpire2ModManager.App.Views;

namespace SlayTheSpire2ModManager.App;

public partial class AllModsList : UserControl
{
    public AllModsList()
    {
        InitializeComponent();
    }

    private async void OnInstallModClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window mainWindow)
        {
            return;
        }

        var installWindow = new InstallModWindow();
        var sourcePath = await installWindow.ShowDialog<string?>(mainWindow);
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return;
        }

        var installResult = await viewModel.InstallMod(sourcePath);
        if (installResult?.HasConflict == true)
        {
            var conflictWindow = new ModVersionConflictWindow(installResult);
            var shouldForceInstall = await conflictWindow.ShowDialog<bool>(mainWindow);
            if (shouldForceInstall)
            {
                await viewModel.InstallMod(sourcePath, true);
            }
        }
    }
}
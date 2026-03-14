using Avalonia.Controls;
using SlayTheSpire2ModManager.App.ViewModels;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.App.Views
{
    public partial class ModVersionConflictWindow : Window
    {
        public ModVersionConflictWindow(ModInstallResult installResult)
        {
            InitializeComponent();
            DataContext = new ModVersionConflictWindowViewModel(installResult, result => Close(result));
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SlayTheSpire2ModManager.App.ViewModels;

namespace SlayTheSpire2ModManager.App.Views
{
    public partial class InstallModWindow : Window
    {
        public InstallModWindow()
        {
            InitializeComponent();
            DataContext = new InstallModWindowViewModel(PickZipAsync, PickFolderAsync, Close);
        }

        private async Task<string?> PickZipAsync()
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择模组压缩包",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Zip 文件")
                    {
                        Patterns = ["*.zip"]
                    }
                ]
            });

            return files.FirstOrDefault()?.TryGetLocalPath();
        }

        private async Task<string?> PickFolderAsync()
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "选择模组文件夹",
                AllowMultiple = false
            });

            return folders.FirstOrDefault()?.TryGetLocalPath();
        }
    }
}

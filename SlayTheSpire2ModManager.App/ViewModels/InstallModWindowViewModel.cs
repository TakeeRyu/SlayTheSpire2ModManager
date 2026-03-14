using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlayTheSpire2ModManager.App.ViewModels
{
    public partial class InstallModWindowViewModel : ViewModelBase
    {
        private readonly Func<Task<string?>> _pickZipAsync;
        private readonly Func<Task<string?>> _pickFolderAsync;
        private readonly Action<string?> _closeAction;

        [ObservableProperty]
        private string? _sourcePath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public InstallModWindowViewModel(
            Func<Task<string?>> pickZipAsync,
            Func<Task<string?>> pickFolderAsync,
            Action<string?> closeAction)
        {
            _pickZipAsync = pickZipAsync;
            _pickFolderAsync = pickFolderAsync;
            _closeAction = closeAction;
        }

        [RelayCommand]
        private async Task PickZipAsync()
        {
            var selectedPath = await _pickZipAsync();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            SourcePath = selectedPath;
            ErrorMessage = null;
        }

        [RelayCommand]
        private async Task PickFolderAsync()
        {
            var selectedPath = await _pickFolderAsync();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            SourcePath = selectedPath;
            ErrorMessage = null;
        }

        [RelayCommand]
        private void Install()
        {
            var sourcePath = SourcePath?.Trim();
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                ErrorMessage = "请先选择模组路径。";
                return;
            }

            if (Directory.Exists(sourcePath) || 
                (File.Exists(sourcePath) && 
                 string.Equals(Path.GetExtension(sourcePath), ".zip", StringComparison.OrdinalIgnoreCase)))
            {
                _closeAction(sourcePath);
                return;
            }

            ErrorMessage = "仅支持 .zip 文件或文件夹路径。";
        }

        [RelayCommand]
        private void Cancel()
        {
            _closeAction(null);
        }
    }
}

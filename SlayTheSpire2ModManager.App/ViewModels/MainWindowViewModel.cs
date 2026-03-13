using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Utils;
using Path = System.IO.Path;


namespace SlayTheSpire2ModManager.App.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const string EmptyDirectoryString = "尚未发现游戏路径...";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGameDirectoryAvailable))]
        public string? _gameDirectory = EmptyDirectoryString;

        public bool IsGameDirectoryAvailable => Directory.Exists(GameDirectory) && !GameDirectory.Equals(EmptyDirectoryString);

        [ObservableProperty]
        public bool? _isLoadingGameDirectory = false;

        [RelayCommand]
        public async Task SetGameDirectory()
        {
            IsLoadingGameDirectory = true;
            var result = await Task.Run(() => GameDirectoryUtils.FindGameDirAsync(GameConstants.GameDirectoryPath, GameConstants.GameExecutableFileName));
            if (string.IsNullOrEmpty(result))
            {
                var msgBox = MessageBoxManager.GetMessageBoxStandard("获取游戏路径失败", "无法获取游戏路径。", ButtonEnum.Ok);
                await msgBox.ShowAsync();
            }
            else
            {
                GameDirectory = result;
            }
            IsLoadingGameDirectory = false;
        }
    }
}

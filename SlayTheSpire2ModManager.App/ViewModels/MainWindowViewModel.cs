using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Avalonia.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SlayTheSpire2ModManager.App.Models;
using SlayTheSpire2ModManager.Infrastructure.Configuration;
using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Utils;


namespace SlayTheSpire2ModManager.App.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const string EmptyDirectoryString = "尚未发现游戏路径...";
        private readonly AppConfigStore _appConfigStore;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGameDirectoryAvailable))]
        public string? _gameDirectory = EmptyDirectoryString;

        public bool IsGameDirectoryAvailable => Directory.Exists(GameDirectory) && !GameDirectory.Equals(EmptyDirectoryString);

        [ObservableProperty]
        public bool? _isLoadingGameDirectory = false;

        [ObservableProperty] 
        public ObservableCollection<GameModMetadata> _modMetadataListCollection;

        public MainWindowViewModel() : this(new AppConfigStore())
        {
        }

        public MainWindowViewModel(AppConfigStore appConfigStore)
        {
            _appConfigStore = appConfigStore;

            var mods = new List<GameModMetadata>
            {
                new()
                {
                    Author = "皮一下就很凡@Bilibili",
                    Description = "谁尽力?谁犯罪?谁的打法不团队?伤害、格挡、助攻、卡牌效率……17种战斗统计全覆盖，数据仪表盘一键复盘。开黑必装，全平台通用。Who carried? Who slacked? 17 combat stat categories, dashboard, co-op ready. Cross-platform.",
                    Name = "Skada: Damage Meter",
                    PckName = "DamageMeter",
                    Version = "1.7.3"
                }
            };

            ModMetadataListCollection = new ObservableCollection<GameModMetadata>(mods);

            _ = LoadGameDirectoryFromConfigAsync();
        }

        [RelayCommand]
        public async Task SetGameDirectory()
        {
            IsLoadingGameDirectory = true;
            string? result = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result = await Task.Run(() => GameDirectoryUtils.FindGameDirWindowsAsync(GameConstants.GameDirectoryPath, GameConstants.GameExecutableFileName_Windows));
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // result = await Task.Run(() => GameDirectoryUtils.FindGameDirMacAsync(GameConstants.GameDirectoryPath, GameConstants.GameExecutableFileName_Windows));
                var msgBox = MessageBoxManager.GetMessageBoxStandard("不支持的平台", "无法在macOS上自动发现游戏路径。", ButtonEnum.Ok);
                await msgBox.ShowAsync();
            }
            
            if (string.IsNullOrEmpty(result))
            {
                var msgBox = MessageBoxManager.GetMessageBoxStandard("发现游戏路径失败", "无法发现游戏路径。", ButtonEnum.Ok);
                await msgBox.ShowAsync();
            }
            else
            {
                GameDirectory = result;
                await _appConfigStore.SetGameDirectoryAsync(result);
            }

            IsLoadingGameDirectory = false;
        }

        private async Task LoadGameDirectoryFromConfigAsync()
        {
            var gameDirectory = await _appConfigStore.GetGameDirectoryAsync();
            if (!string.IsNullOrWhiteSpace(gameDirectory) && Directory.Exists(gameDirectory))
            {
                GameDirectory = gameDirectory;
            }
        }
    }
}

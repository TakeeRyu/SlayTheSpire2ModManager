using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        private bool _isApplyingModState;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGameDirectoryAvailable))]
        public string? _gameDirectory = EmptyDirectoryString;

        public bool IsGameDirectoryAvailable => Directory.Exists(GameDirectory) && !GameDirectory.Equals(EmptyDirectoryString);

        [ObservableProperty]
        public bool? _isLoadingGameDirectory = false;

        [ObservableProperty] 
        public ObservableCollection<GameModMetadata> _modMetadataListCollection;

        private GameModMetadata? _selectedMod;
        public GameModMetadata? SelectedMod
        {
            get => _selectedMod;
            set => SetProperty(ref _selectedMod, value);
        }

        public MainWindowViewModel() : this(new AppConfigStore())
        {
        }

        public MainWindowViewModel(AppConfigStore appConfigStore)
        {
            _appConfigStore = appConfigStore;

            ModMetadataListCollection = [];

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
                await LoadLocalModsFromGameDirectoryAsync();
            }

            IsLoadingGameDirectory = false;
        }

        [RelayCommand]
        public async Task DisableAllMods()
        {
            await SetAllModsEnabledAsync(false);
        }

        [RelayCommand]
        public async Task EnableAllMods()
        {
            await SetAllModsEnabledAsync(true);
        }

        private async Task LoadGameDirectoryFromConfigAsync()
        {
            var gameDirectory = await _appConfigStore.GetGameDirectoryAsync();
            if (!string.IsNullOrWhiteSpace(gameDirectory) && Directory.Exists(gameDirectory))
            {
                GameDirectory = gameDirectory;
                await LoadLocalModsFromGameDirectoryAsync();
            }
        }

        private async Task LoadLocalModsFromGameDirectoryAsync()
        {
            UnsubscribeModEvents(ModMetadataListCollection);

            if (!IsGameDirectoryAvailable)
            {
                ModMetadataListCollection = [];
                SelectedMod = null;
                return;
            }

            var localMods = await ModMetadataUtils.ReadLocalModsAsync(GameDirectory!);

            ModMetadataListCollection = new ObservableCollection<GameModMetadata>(
                localMods.Select(mod => new GameModMetadata
                {
                    Name = mod.Name,
                    PckName = mod.PckName,
                    Author = mod.Author,
                    Description = mod.Description,
                    Version = mod.Version,
                    IsEnabled = mod.IsEnabled,
                    DirectoryPath = mod.DirectoryPath
                }));

            SubscribeModEvents(ModMetadataListCollection);
            SelectedMod = ModMetadataListCollection.FirstOrDefault();
        }

        private void SubscribeModEvents(IEnumerable<GameModMetadata> mods)
        {
            foreach (var mod in mods)
            {
                mod.PropertyChanged += OnModPropertyChanged;
            }
        }

        private void UnsubscribeModEvents(IEnumerable<GameModMetadata> mods)
        {
            foreach (var mod in mods)
            {
                mod.PropertyChanged -= OnModPropertyChanged;
            }
        }

        private async void OnModPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isApplyingModState || e.PropertyName != nameof(GameModMetadata.IsEnabled) || sender is not GameModMetadata mod)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(mod.DirectoryPath))
            {
                return;
            }

            try
            {
                _isApplyingModState = true;
                mod.DirectoryPath = await ModMetadataUtils.SetModEnabledAsync(mod.DirectoryPath, mod.IsEnabled);
            }
            catch
            {
                mod.IsEnabled = !mod.IsEnabled;
                var msgBox = MessageBoxManager.GetMessageBoxStandard("更新模组状态失败", "无法修改模组启用状态。", ButtonEnum.Ok);
                await msgBox.ShowAsync();
            }
            finally
            {
                _isApplyingModState = false;
            }
        }

        private async Task SetAllModsEnabledAsync(bool isEnabled)
        {
            if (!IsGameDirectoryAvailable || ModMetadataListCollection.Count == 0)
            {
                return;
            }

            try
            {
                _isApplyingModState = true;

                foreach (var mod in ModMetadataListCollection)
                {
                    if (string.IsNullOrWhiteSpace(mod.DirectoryPath) || mod.IsEnabled == isEnabled)
                    {
                        continue;
                    }

                    mod.DirectoryPath = await ModMetadataUtils.SetModEnabledAsync(mod.DirectoryPath, isEnabled);
                    mod.IsEnabled = isEnabled;
                }
            }
            catch
            {
                var msgBox = MessageBoxManager.GetMessageBoxStandard("更新模组状态失败", "批量修改模组启用状态失败。", ButtonEnum.Ok);
                await msgBox.ShowAsync();
                await LoadLocalModsFromGameDirectoryAsync();
            }
            finally
            {
                _isApplyingModState = false;
            }
        }
    }
}

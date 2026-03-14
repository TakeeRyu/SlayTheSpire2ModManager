using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SlayTheSpire2ModManager.App.Models;
using SlayTheSpire2ModManager.Infrastructure.Models;
using SlayTheSpire2ModManager.Infrastructure.Utils;

namespace SlayTheSpire2ModManager.App.ViewModels;

public partial class SavesSlotViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<SaveSlotInfo> _saveSlots = [];

    [ObservableProperty]
    private ObservableCollection<SaveSlotInfo> _normalSaveSlots = [];

    [ObservableProperty]
    private ObservableCollection<SaveSlotInfo> _moddedSaveSlots = [];

    [ObservableProperty]
    private SaveSlotInfo? _selectedSlot;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "等待加载存档槽位...";

    public SavesSlotViewModel()
    {
        Task.Run(RefreshSlots);
    }

    [RelayCommand]
    public async Task RefreshSlots()
    {
        IsLoading = true;

        var readResult = await SaveSlotUtils.ReadSaveSlotsAsync();
        var allSlots = readResult.Slots
            .OrderBy(x => x.SaveType)
            .ThenBy(x => x.SlotId)
            .Select(x => new SaveSlotInfo
            {
                SaveType = x.SaveType,
                SlotId = x.SlotId,
                DisplayName = $"{(x.SaveType == SaveSlotType.Normal ? "原版" : "模组")}槽位 {x.SlotId}",
                LastWriteTime = x.LastModified,
                PlayTime = null,
                DirectoryPath = x.DirectoryPath,
                IsValid = x.HasData
            })
            .ToList();

        SaveSlots = new ObservableCollection<SaveSlotInfo>(allSlots);
        NormalSaveSlots = new ObservableCollection<SaveSlotInfo>(allSlots.Where(x => x.SaveType == SaveSlotType.Normal));
        ModdedSaveSlots = new ObservableCollection<SaveSlotInfo>(allSlots.Where(x => x.SaveType == SaveSlotType.Modded));
        SelectedSlot = SaveSlots.FirstOrDefault();

        if (readResult.SelectedSteamId is null)
        {
            StatusMessage = "未找到存档目录。请先运行一次游戏。";
        }
        else if (readResult.HasMultipleSteamIds)
        {
            StatusMessage = $"已读取 Steam 账号 {readResult.SelectedSteamId} 的存档（检测到多个账号）。";
        }
        else
        {
            StatusMessage = $"已读取 Steam 账号 {readResult.SelectedSteamId} 的存档。";
        }

        IsLoading = false;
    }

    [RelayCommand]
    public Task BackupSelectedSlot()
    {
        StatusMessage = SelectedSlot is null
            ? "请先选择一个存档槽位。"
            : "本功能施工中！";

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task RestoreToSelectedSlot()
    {
        StatusMessage = SelectedSlot is null
            ? "请先选择一个存档槽位。"
            : "本功能施工中！";

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task DeleteSelectedSlot()
    {
        StatusMessage = SelectedSlot is null
            ? "请先选择一个存档槽位。"
            : "本功能施工中！";

        return Task.CompletedTask;
    }
}

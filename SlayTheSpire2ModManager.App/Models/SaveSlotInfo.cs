using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.App.Models;

public partial class SaveSlotInfo : ObservableObject
{
    public SaveSlotType SaveType { get; set; }

    public string SaveTypeLabel => SaveType == SaveSlotType.Normal ? "普通存档" : "模组存档";

    public string SlotCode => SaveType == SaveSlotType.Normal ? $"A{SlotId}" : $"B{SlotId}";

    public int SlotId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public DateTime? LastWriteTime { get; set; }

    public string? PlayTime { get; set; }

    public string? DirectoryPath { get; set; }

    [ObservableProperty]
    private bool _isValid = true;
}

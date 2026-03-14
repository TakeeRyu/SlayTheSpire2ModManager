namespace SlayTheSpire2ModManager.Infrastructure.Models
{
    public class SaveSlotsReadResult
    {
        public string? SelectedSteamId { get; set; }

        public bool HasMultipleSteamIds { get; set; }

        public IReadOnlyList<string> DetectedSteamIds { get; set; } = [];

        public IReadOnlyList<SaveSlotSnapshot> Slots { get; set; } = [];
    }
}

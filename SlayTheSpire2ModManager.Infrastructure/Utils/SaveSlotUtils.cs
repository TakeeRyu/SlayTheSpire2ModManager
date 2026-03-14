using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    public static class SaveSlotUtils
    {
        public static Task<SaveSlotsReadResult> ReadSaveSlotsAsync(CancellationToken cancellationToken = default)
        {
            var result = new SaveSlotsReadResult();
            var saveRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                SaveConstants.SaveRootFolderName,
                SaveConstants.SaveSteamFolderName);

            if (!Directory.Exists(saveRoot))
            {
                return Task.FromResult(result);
            }

            var steamIds = Directory.GetDirectories(saveRoot)
                .Select(Path.GetFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Cast<string>()
                .ToList();

            if (steamIds.Count == 0)
            {
                return Task.FromResult(result);
            }

            result.DetectedSteamIds = steamIds;
            result.HasMultipleSteamIds = steamIds.Count > 1;

            var selectedSteamId = SelectSteamId(saveRoot, steamIds);
            result.SelectedSteamId = selectedSteamId;

            if (string.IsNullOrWhiteSpace(selectedSteamId))
            {
                return Task.FromResult(result);
            }

            var snapshots = new List<SaveSlotSnapshot>(SaveConstants.MaxProfileSlotCount * 2);

            for (var slotId = 1; slotId <= SaveConstants.MaxProfileSlotCount; slotId++)
            {
                snapshots.Add(BuildSnapshot(saveRoot, selectedSteamId, SaveSlotType.Normal, slotId));
                snapshots.Add(BuildSnapshot(saveRoot, selectedSteamId, SaveSlotType.Modded, slotId));
            }

            result.Slots = snapshots;
            return Task.FromResult(result);
        }

        private static SaveSlotSnapshot BuildSnapshot(string saveRoot, string steamId, SaveSlotType saveType, int slotId)
        {
            var directoryPath = saveType == SaveSlotType.Normal
                ? Path.Combine(saveRoot, steamId, $"profile{slotId}", "saves")
                : Path.Combine(saveRoot, steamId, "modded", $"profile{slotId}", "saves");

            var progressSavePath = Path.Combine(directoryPath, SaveConstants.ProgressSaveFileName);
            var hasData = File.Exists(progressSavePath);
            DateTime? lastModified = null;

            if (hasData)
            {
                lastModified = File.GetLastWriteTime(progressSavePath);
            }

            return new SaveSlotSnapshot
            {
                SteamId = steamId,
                SaveType = saveType,
                SlotId = slotId,
                DirectoryPath = directoryPath,
                HasData = hasData,
                LastModified = lastModified
            };
        }

        private static string? SelectSteamId(string saveRoot, IReadOnlyCollection<string> steamIds)
        {
            if (steamIds.Count == 1)
            {
                return steamIds.FirstOrDefault();
            }

            return steamIds
                .OrderByDescending(id =>
                {
                    var idPath = Path.Combine(saveRoot, id);
                    return Directory.GetLastWriteTime(idPath);
                })
                .FirstOrDefault();
        }
    }
}

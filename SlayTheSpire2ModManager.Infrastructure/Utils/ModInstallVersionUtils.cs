using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    internal static class ModInstallVersionUtils
    {
        public static bool IsSameMod(ModMetadata existingMod, ModMetadata candidateMod)
        {
            if (!string.IsNullOrWhiteSpace(existingMod.PckName) && !string.IsNullOrWhiteSpace(candidateMod.PckName))
            {
                return string.Equals(existingMod.PckName, candidateMod.PckName, StringComparison.OrdinalIgnoreCase);
            }

            return !string.IsNullOrWhiteSpace(existingMod.Name)
                && !string.IsNullOrWhiteSpace(candidateMod.Name)
                && string.Equals(existingMod.Name, candidateMod.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static int CompareVersion(string? candidateVersion, string? existingVersion)
        {
            var candidateParts = ParseVersionParts(candidateVersion);
            var existingParts = ParseVersionParts(existingVersion);

            if (candidateParts.Count > 0 && existingParts.Count > 0)
            {
                var maxLength = Math.Max(candidateParts.Count, existingParts.Count);
                for (var i = 0; i < maxLength; i++)
                {
                    var candidatePart = i < candidateParts.Count ? candidateParts[i] : 0;
                    var existingPart = i < existingParts.Count ? existingParts[i] : 0;

                    if (candidatePart > existingPart)
                    {
                        return 1;
                    }

                    if (candidatePart < existingPart)
                    {
                        return -1;
                    }
                }

                return 0;
            }

            return string.Compare(candidateVersion?.Trim(), existingVersion?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static List<int> ParseVersionParts(string? version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return [];
            }

            return version
                .Split(['.', '-', '_', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var value) ? value : -1)
                .TakeWhile(part => part >= 0)
                .ToList();
        }
    }
}

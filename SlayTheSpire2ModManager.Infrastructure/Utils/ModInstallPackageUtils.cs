using System.Text.Json;
using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    internal static class ModInstallPackageUtils
    {
        public static string ResolveCandidateModDirectory(string rootDirectory)
        {
            if (IsLikelyModDirectory(rootDirectory))
            {
                return rootDirectory;
            }

            var allCandidates = Directory
                .EnumerateDirectories(rootDirectory, "*", SearchOption.AllDirectories)
                .Where(IsLikelyModDirectory)
                .ToList();

            if (allCandidates.Count == 1)
            {
                return allCandidates[0];
            }

            if (allCandidates.Count > 1)
            {
                throw new InvalidOperationException("Found multiple candidate mod directories. Please select the exact mod folder.");
            }

            throw new InvalidOperationException("Cannot locate mod directory. Please ensure selected zip/folder contains .dll, .pck and .json files.");
        }

        public static async Task<ModMetadata> ReadModMetadataFromDirectoryAsync(string modDirectory, CancellationToken cancellationToken)
        {
            if (!IsLikelyModDirectory(modDirectory))
            {
                throw new InvalidOperationException("Invalid mod directory.");
            }

            var jsonFile = Directory.EnumerateFiles(modDirectory, "*.json", SearchOption.TopDirectoryOnly).FirstOrDefault()
                           ?? Directory.EnumerateFiles(modDirectory, $"*.json{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(jsonFile))
            {
                throw new InvalidOperationException("Cannot find mod metadata json.");
            }

            await using var stream = File.OpenRead(jsonFile);
            var metadata = await JsonSerializer.DeserializeAsync<ModMetadata>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            if (metadata is null)
            {
                throw new InvalidOperationException("Cannot read mod metadata.");
            }

            metadata.Name ??= ModInstallPathUtils.RemoveDisabledPrefix(Path.GetFileName(modDirectory));
            return metadata;
        }

        public static bool IsLikelyModDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            var hasDll = Directory.EnumerateFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly).Any() ||
                         Directory.EnumerateFiles(directoryPath, $"*.dll{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).Any();
            var hasPck = Directory.EnumerateFiles(directoryPath, "*.pck", SearchOption.TopDirectoryOnly).Any() ||
                         Directory.EnumerateFiles(directoryPath, $"*.pck{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).Any();
            var hasJson = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly).Any() ||
                          Directory.EnumerateFiles(directoryPath, $"*.json{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).Any();

            return hasDll && hasPck && hasJson;
        }
    }
}

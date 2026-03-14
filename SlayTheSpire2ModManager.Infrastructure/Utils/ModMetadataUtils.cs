using System.Text.Json;
using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    public static class ModMetadataUtils
    {
        public static async Task<IReadOnlyList<ModMetadata>> ReadLocalModsAsync(string gameDirectory, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                return [];
            }

            var modsDirectory = Path.Combine(gameDirectory, ModConstants.ModsDirectoryName);
            if (!Directory.Exists(modsDirectory))
            {
                return [];
            }

            var result = new List<ModMetadata>();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var modDirectory in Directory.GetDirectories(modsDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var modDirectoryName = Path.GetFileName(modDirectory);
                var isDisabled = modDirectoryName.StartsWith(ModConstants.DisabledPrefix, StringComparison.OrdinalIgnoreCase);

                var hasDll = isDisabled
                    ? Directory.EnumerateFiles(modDirectory, $"*.dll{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).Any()
                    : Directory.EnumerateFiles(modDirectory, "*.dll", SearchOption.TopDirectoryOnly).Any();

                var hasPck = isDisabled
                    ? Directory.EnumerateFiles(modDirectory, $"*.pck{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).Any()
                    : Directory.EnumerateFiles(modDirectory, "*.pck", SearchOption.TopDirectoryOnly).Any();

                var jsonFile = isDisabled
                    ? Directory.EnumerateFiles(modDirectory, $"*.json{ModConstants.DisabledSuffix}", SearchOption.TopDirectoryOnly).FirstOrDefault()
                    : Directory.EnumerateFiles(modDirectory, "*.json", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (!hasDll || !hasPck || string.IsNullOrWhiteSpace(jsonFile))
                {
                    continue;
                }

                try
                {
                    await using var stream = File.OpenRead(jsonFile);
                    var metadata = await JsonSerializer.DeserializeAsync<ModMetadata>(stream, jsonOptions, cancellationToken);
                    if (metadata is null)
                    {
                        continue;
                    }

                    metadata.Name ??= isDisabled
                        ? modDirectoryName[ModConstants.DisabledPrefix.Length..]
                        : modDirectoryName;
                    metadata.IsEnabled = !isDisabled;
                    metadata.DirectoryPath = modDirectory;
                    result.Add(metadata);
                }
                catch
                {
                }
            }

            return result;
        }

        public static Task<string> SetModEnabledAsync(string modDirectoryPath, bool isEnabled, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(modDirectoryPath))
            {
                throw new ArgumentException("Mod directory path is required.", nameof(modDirectoryPath));
            }

            if (!Directory.Exists(modDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Mod directory not found: {modDirectoryPath}");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var currentPath = modDirectoryPath;

            if (isEnabled)
            {
                var disabledFiles = Directory
                    .EnumerateFiles(currentPath, $"*{ModConstants.DisabledSuffix}", SearchOption.AllDirectories)
                    .ToList();

                foreach (var file in disabledFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var enabledPath = file[..^ModConstants.DisabledSuffix.Length];
                    File.Move(file, enabledPath, true);
                }

                var directoryName = Path.GetFileName(currentPath);
                if (directoryName.StartsWith(ModConstants.DisabledPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var parent = Path.GetDirectoryName(currentPath)!;
                    var enabledDirectoryName = directoryName[ModConstants.DisabledPrefix.Length..];
                    var enabledDirectoryPath = Path.Combine(parent, enabledDirectoryName);
                    Directory.Move(currentPath, enabledDirectoryPath);
                    currentPath = enabledDirectoryPath;
                }
            }
            else
            {
                var allFiles = Directory
                    .EnumerateFiles(currentPath, "*", SearchOption.AllDirectories)
                    .ToList();

                foreach (var file in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!file.EndsWith(ModConstants.DisabledSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(file, file + ModConstants.DisabledSuffix, true);
                    }
                }

                var directoryName = Path.GetFileName(currentPath);
                if (!directoryName.StartsWith(ModConstants.DisabledPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var parent = Path.GetDirectoryName(currentPath)!;
                    var disabledDirectoryPath = Path.Combine(parent, ModConstants.DisabledPrefix + directoryName);
                    Directory.Move(currentPath, disabledDirectoryPath);
                    currentPath = disabledDirectoryPath;
                }
            }

            return Task.FromResult(currentPath);
        }
    }
}

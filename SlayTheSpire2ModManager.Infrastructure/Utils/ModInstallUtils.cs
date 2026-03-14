using System.IO.Compression;
using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    public static class ModInstallUtils
    {
        public static async Task<ModInstallResult> InstallModAsync(string gameDirectory, string sourcePath, bool forceInstall = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory) || !Directory.Exists(gameDirectory))
            {
                throw new DirectoryNotFoundException("Game directory was not found.");
            }

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException("Mod source path is required.", nameof(sourcePath));
            }

            var modsDirectory = Path.Combine(gameDirectory, ModConstants.ModsDirectoryName);
            Directory.CreateDirectory(modsDirectory);

            string? tempDirectory = null;
            string candidateDirectory;

            if (Directory.Exists(sourcePath))
            {
                candidateDirectory = ModInstallPackageUtils.ResolveCandidateModDirectory(sourcePath);
            }
            else
            {
                if (!File.Exists(sourcePath) || !string.Equals(Path.GetExtension(sourcePath), ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Only .zip file or directory is supported.");
                }

                tempDirectory = Path.Combine(Path.GetTempPath(), $"{ModConstants.TempExtractDirectoryPrefix}{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDirectory);

                cancellationToken.ThrowIfCancellationRequested();
                ZipFile.ExtractToDirectory(sourcePath, tempDirectory, true);
                candidateDirectory = ModInstallPackageUtils.ResolveCandidateModDirectory(tempDirectory);
            }

            try
            {
                var candidateMetadata = await ModInstallPackageUtils.ReadModMetadataFromDirectoryAsync(candidateDirectory, cancellationToken);
                var localMods = await ModMetadataUtils.ReadLocalModsAsync(gameDirectory, cancellationToken);
                var existingMod = localMods.FirstOrDefault(m => ModInstallVersionUtils.IsSameMod(m, candidateMetadata));

                if (existingMod is not null)
                {
                    var versionCompare = ModInstallVersionUtils.CompareVersion(candidateMetadata.Version, existingMod.Version);
                    if (versionCompare == 0 && !forceInstall)
                    {
                        return new ModInstallResult(false, "已安装同名同版本模组。", candidateMetadata, existingMod, ModInstallConflictType.SameVersion);
                    }

                    if (versionCompare < 0 && !forceInstall)
                    {
                        return new ModInstallResult(false, "已安装更新版本模组，当前安装包版本更旧。", candidateMetadata, existingMod, ModInstallConflictType.IncomingOlderVersion);
                    }

                    if (!string.IsNullOrWhiteSpace(existingMod.DirectoryPath) && Directory.Exists(existingMod.DirectoryPath))
                    {
                        Directory.Delete(existingMod.DirectoryPath, true);
                    }
                }

                var destinationDirectoryName = ModInstallPathUtils.ResolveDestinationDirectoryName(sourcePath, candidateDirectory, candidateMetadata, existingMod);
                ModInstallPathUtils.CopyDirectory(candidateDirectory, Path.Combine(modsDirectory, destinationDirectoryName), true);

                return new ModInstallResult(true, existingMod is null ? "模组安装成功。" : "模组升级成功。", candidateMetadata, existingMod);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempDirectory) && Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }
    }
}

using SlayTheSpire2ModManager.Infrastructure.Constants;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    internal static class ModInstallPathUtils
    {
        public static string ResolveDestinationDirectoryName(string sourcePath, string candidateDirectory, ModMetadata candidateMetadata, ModMetadata? existingMod)
        {
            if (!string.IsNullOrWhiteSpace(existingMod?.DirectoryPath))
            {
                var existingName = Path.GetFileName(existingMod.DirectoryPath);
                if (!string.IsNullOrWhiteSpace(existingName))
                {
                    return SanitizeDirectoryName(RemoveDisabledPrefix(existingName));
                }
            }

            if (!string.IsNullOrWhiteSpace(candidateMetadata.Name))
            {
                return SanitizeDirectoryName(RemoveDisabledPrefix(candidateMetadata.Name.Trim()));
            }

            if (string.Equals(Path.GetExtension(sourcePath), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                return SanitizeDirectoryName(Path.GetFileNameWithoutExtension(sourcePath));
            }

            return SanitizeDirectoryName(RemoveDisabledPrefix(Path.GetFileName(candidateDirectory)));
        }

        public static string RemoveDisabledPrefix(string directoryName)
        {
            return directoryName.StartsWith(ModConstants.DisabledPrefix, StringComparison.OrdinalIgnoreCase)
                ? directoryName[ModConstants.DisabledPrefix.Length..]
                : directoryName;
        }

        public static string SanitizeDirectoryName(string? directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                return ModConstants.DefaultModDirectoryName;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(directoryName
                .Trim()
                .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
                .ToArray())
                .TrimEnd('.', ' ');

            return string.IsNullOrWhiteSpace(sanitized) ? ModConstants.DefaultModDirectoryName : sanitized;
        }

        public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite)
        {
            if (Directory.Exists(destinationDirectory))
            {
                Directory.Delete(destinationDirectory, true);
            }

            Directory.CreateDirectory(destinationDirectory);

            foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, file);
                var destinationFile = Path.Combine(destinationDirectory, relativePath);
                var destinationFileDirectory = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrWhiteSpace(destinationFileDirectory))
                {
                    Directory.CreateDirectory(destinationFileDirectory);
                }

                File.Copy(file, destinationFile, overwrite);
            }
        }
    }
}

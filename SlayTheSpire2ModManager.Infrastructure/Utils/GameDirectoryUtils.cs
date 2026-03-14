using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SlayTheSpire2ModManager.Infrastructure.Utils
{
    public static class GameDirectoryUtils
    {
        public static async Task<string?> FindGameDirWindowsAsync(string gameDirName, string gameExe)
        {
            var steamPath = GetSteamPath_Windows();

            if (!string.IsNullOrEmpty(steamPath))
            {
                steamPath = steamPath.Replace('/', '\\');

                var candidate = Path.Combine(steamPath, "steamapps", "common", gameDirName);
                if (File.Exists(Path.Combine(candidate, gameExe)))
                    return candidate;

                var vdfPaths = new[]
                {
                    Path.Combine(steamPath, "steamapps", "libraryfolders.vdf"),
                    Path.Combine(steamPath, "config", "libraryfolders.vdf")
                };

                foreach (var vdf in vdfPaths)
                {
                    if (!File.Exists(vdf))
                        continue;

                    var content = await File.ReadAllTextAsync(vdf);

                    var matches = Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\"");

                    foreach (Match m in matches)
                    {
                        try
                        {
                            var libPath = m.Groups[1].Value.Replace("\\\\", "\\");
                            candidate = Path.Combine(libPath, "steamapps", "common", gameDirName);

                            if (File.Exists(Path.Combine(candidate, gameExe)))
                                return candidate;
                        }
                        catch { }
                    }

                    break;
                }
            }

            var drives = DriveInfo.GetDrives()
                .Where(d => d is { DriveType: DriveType.Fixed, IsReady: true })
                .Select(d => d.RootDirectory.FullName);

            var subPaths = new[]
            {
                $"SteamLibrary\\steamapps\\common\\{gameDirName}",
                $"Steam\\steamapps\\common\\{gameDirName}",
                $"Program Files (x86)\\Steam\\steamapps\\common\\{gameDirName}",
                $"Program Files\\Steam\\steamapps\\common\\{gameDirName}",
                $"Games\\Steam\\steamapps\\common\\{gameDirName}",
                $"Games\\SteamLibrary\\steamapps\\common\\{gameDirName}",
                $"Game\\Steam\\steamapps\\common\\{gameDirName}",
                $"Game\\SteamLibrary\\steamapps\\common\\{gameDirName}"
            };

            foreach (var drive in drives)
            {
                foreach (var sub in subPaths)
                {
                    try
                    {
                        var candidate = Path.Combine(drive, sub);

                        if (File.Exists(Path.Combine(candidate, gameExe)))
                            return candidate;
                    }
                    catch { }
                }

                await Task.Yield();
            }

            return null;
        }

        public static async Task<string?> FindGameDirMacAsync(string gameDirName, string gameExe)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var steamPath = Path.Combine(
                home,
                "Library",
                "Application Support",
                "Steam"
            );

            if (!Directory.Exists(steamPath))
                return null;

            var candidate = Path.Combine(
                steamPath,
                "steamapps",
                "common",
                gameDirName
            );

            if (File.Exists(Path.Combine(candidate, gameExe)))
                return candidate;

            var vdf = Path.Combine(
                steamPath,
                "steamapps",
                "libraryfolders.vdf"
            );

            if (!File.Exists(vdf))
                return null;

            var content = await File.ReadAllTextAsync(vdf);

            var matches = Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\"");

            foreach (Match m in matches)
            {
                try
                {
                    var libPath = m.Groups[1].Value.Replace("\\\\", "\\");

                    candidate = Path.Combine(
                        libPath,
                        "steamapps",
                        "common",
                        gameDirName
                    );

                    if (File.Exists(Path.Combine(candidate, gameExe)))
                        return candidate;
                }
                catch { }
            }

            return null;
        }

        private static string? GetSteamPath_Windows()
        {
            foreach (var reg in new[]
            {
                (RegistryHive.CurrentUser, @"SOFTWARE\Valve\Steam"),
                (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam")
            })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(reg.Item1, RegistryView.Registry64);
                    using var key = baseKey.OpenSubKey(reg.Item2);

                    if (key == null)
                        continue;

                    var steamPath = key.GetValue("SteamPath") as string ??
                                    key.GetValue("InstallPath") as string;

                    if (!string.IsNullOrEmpty(steamPath))
                        return steamPath;
                }
                catch { }
            }

            return null;
        }
    }
}

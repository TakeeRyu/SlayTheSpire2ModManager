namespace SlayTheSpire2ModManager.Infrastructure.Models
{
    public sealed record ModInstallResult(
        bool IsInstalled,
        string Message,
        ModMetadata Candidate,
        ModMetadata? Existing,
        ModInstallConflictType ConflictType = ModInstallConflictType.None)
    {
        public bool HasConflict => ConflictType != ModInstallConflictType.None;
    }

    public enum ModInstallConflictType
    {
        None = 0,
        SameVersion = 1,
        IncomingOlderVersion = 2
    }
}

namespace SlayTheSpire2ModManager.Infrastructure.Models
{
    public class SaveSlotSnapshot
    {
        public required string SteamId { get; set; }

        public SaveSlotType SaveType { get; set; }

        public int SlotId { get; set; }

        public required string DirectoryPath { get; set; }

        public bool HasData { get; set; }

        public DateTime? LastModified { get; set; }
    }
}

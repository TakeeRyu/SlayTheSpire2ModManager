using CommunityToolkit.Mvvm.ComponentModel;

namespace SlayTheSpire2ModManager.App.Models
{
    public partial class GameModMetadata : ObservableObject
    {
        public string? Name { get; set; }
        public string? PckName { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? DirectoryPath { get; set; }

        [ObservableProperty]
        private bool _isEnabled = true;
    }
}

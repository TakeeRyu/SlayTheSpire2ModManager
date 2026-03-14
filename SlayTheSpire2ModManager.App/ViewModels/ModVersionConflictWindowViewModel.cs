using System;
using CommunityToolkit.Mvvm.Input;
using SlayTheSpire2ModManager.Infrastructure.Models;

namespace SlayTheSpire2ModManager.App.ViewModels
{
    public partial class ModVersionConflictWindowViewModel : ViewModelBase
    {
        private readonly Action<bool> _closeAction;

        public string ConflictMessage { get; }

        public string ExistingName { get; }
        public string ExistingVersion { get; }
        public string ExistingAuthor { get; }

        public string IncomingName { get; }
        public string IncomingVersion { get; }
        public string IncomingAuthor { get; }

        public ModVersionConflictWindowViewModel(ModInstallResult installResult, Action<bool> closeAction)
        {
            _closeAction = closeAction;

            ConflictMessage = installResult.Message;

            ExistingName = installResult.Existing?.Name ?? "-";
            ExistingVersion = installResult.Existing?.Version ?? "-";
            ExistingAuthor = installResult.Existing?.Author ?? "-";

            IncomingName = installResult.Candidate?.Name ?? "-";
            IncomingVersion = installResult.Candidate?.Version ?? "-";
            IncomingAuthor = installResult.Candidate?.Author ?? "-";
        }

        [RelayCommand]
        private void KeepExisting()
        {
            _closeAction(false);
        }

        [RelayCommand]
        private void InstallAnyway()
        {
            _closeAction(true);
        }
    }
}

using System.Diagnostics;
using System.Windows.Input;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.Commands;

namespace FolderDeepSearchV2.Core.Searching.Results {
    public abstract class SizeCalculatableResultViewModel : BaseResultViewModel {
        private long calculatedSize;
        public long CalculatedSize {
            get => this.calculatedSize;
            set => this.RaisePropertyChanged(ref this.calculatedSize, value);
        }

        private bool hasCalculatedSize;
        public bool HasCalculatedSize {
            get => this.hasCalculatedSize;
            set => this.RaisePropertyChanged(ref this.hasCalculatedSize, value);
        }

        public ICommand CalculateSizeCommand { get; }

        public ICommand OpenContainingFolderCommand { get; }
        public ICommand CopyPathToClipboard { get; }

        protected SizeCalculatableResultViewModel(string filePath) : base(filePath) {
            this.CalculateSizeCommand = new RelayCommand(this.CalculateSizeAction);

            this.OpenContainingFolderCommand = new RelayCommand(this.OpenContainingFolderAction);

            this.CopyPathToClipboard = new RelayCommand(() => {
                ServiceManager.Clipboard.SetText(this.FilePath);
            });
        }

        public void OpenContainingFolderAction() {
            Process.Start("explorer.exe", "/select, \"" + this.FilePath + "\"");
        }

        public abstract void CalculateSizeAction();
    }
}
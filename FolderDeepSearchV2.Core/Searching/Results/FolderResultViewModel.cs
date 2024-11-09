using System.IO;
using System.Windows.Input;
using REghZy.MVVM.Commands;

namespace FolderDeepSearchV2.Core.Searching.Results {
    public class FolderResultViewModel : SizeCalculatableResultViewModel {
        public ICommand OpenFolderCommand { get; }

        public FolderResultViewModel(string filePath) : base(filePath) {
            this.OpenFolderCommand = new RelayCommand(this.OpenFolderAction);
            DirectoryInfo info = new DirectoryInfo(filePath);
            this.TimeCreated = info.CreationTime;
            this.TimeLastModified = info.LastWriteTime;
            this.TimeLastAccessed = info.LastAccessTime;
        }

        public void OpenFolderAction() {
            Opener.OpenFile(this.FilePath);
        }

        public override void CalculateSizeAction() {
            this.CalculatedSize = 1;
            this.HasCalculatedSize = true;
        }
    }
}
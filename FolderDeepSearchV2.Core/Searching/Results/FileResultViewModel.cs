using System.IO;
using System.Windows.Input;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.Commands;

namespace FolderDeepSearchV2.Core.Searching.Results {
    public class FileResultViewModel : SizeCalculatableResultViewModel {
        public ICommand OpenFileCommand { get; }

        public FileResultViewModel(string filePath) : base(filePath) {
            this.OpenFileCommand = new RelayCommand(this.OpenFileAction);

            if (File.Exists(filePath)) {
                this.CalculatedSize = new FileInfo(filePath).Length;
            }
        }

        public void OpenFileAction() {
            Opener.OpenFile(this.FilePath);
        }

        public override void CalculateSizeAction() {
            if (File.Exists(this.FilePath)) {
                this.CalculatedSize = new FileInfo(this.FilePath).Length;
                this.HasCalculatedSize = true;
            }
            else {
                this.HasCalculatedSize = false;
                this.CalculatedSize = 0;
                ServiceManager.Messages.Show("No such file", "The file no longer exists: " + this.FilePath);
            }
        }
    }
}
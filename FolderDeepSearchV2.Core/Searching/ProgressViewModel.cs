using System.Threading.Tasks;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching {
    public class ProgressViewModel : BaseViewModel {
        private bool isRunning;
        public bool IsRunning {
            get => this.isRunning;
            set => this.RaisePropertyChanged(ref this.isRunning, value);
        }

        private int foundFiles;
        public int FoundFiles {
            get => this.foundFiles;
            set => this.RaisePropertyChanged(ref this.foundFiles, value);
        }

        private int foundFolders;
        public int FoundFolders {
            get => this.foundFolders;
            set => this.RaisePropertyChanged(ref this.foundFolders, value);
        }

        private string currentDirectory;
        public string CurrentPath {
            get => this.currentDirectory;
            set => this.RaisePropertyChangedCheckEqual(ref this.currentDirectory, value);
        }

        public ProgressViewModel(FolderSearchViewModel search) {
            Task.Factory.StartNew(async () => {
                while (ServiceManager.App.IsRunning()) {
                    if (search.IsSearching) {
                        this.CurrentPath = search.CurrentPath;
                    }
                    else if (this.CurrentPath != null) {
                        this.CurrentPath = null;
                    }

                    await Task.Delay(25);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
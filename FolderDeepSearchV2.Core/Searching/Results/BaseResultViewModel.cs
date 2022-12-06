using System.IO;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching.Results {
    public abstract class BaseResultViewModel : BaseViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public string FileName { get => Path.GetFileName(this.FilePath) ?? this.FilePath; }

        public BaseResultViewModel(string filePath) {
            this.FilePath = filePath;
        }
    }
}
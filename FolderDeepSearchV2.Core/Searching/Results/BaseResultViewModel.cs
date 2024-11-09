using System;
using System.IO;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching.Results {
    public abstract class BaseResultViewModel : BaseViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }
        
        private DateTime timeCreated;
        public DateTime TimeCreated {
            get => this.timeCreated;
            set => this.RaisePropertyChanged(ref this.timeCreated, value);
        }
        
        private DateTime timeLastModified;
        public DateTime TimeLastModified {
            get => this.timeLastModified;
            set => this.RaisePropertyChanged(ref this.timeLastModified, value);
        }
        
        private DateTime timeLastAccessed;
        public DateTime TimeLastAccessed {
            get => this.timeLastAccessed;
            set => this.RaisePropertyChanged(ref this.timeLastAccessed, value);
        }

        public string FileName { get => Path.GetFileName(this.FilePath) ?? this.FilePath; }

        public BaseResultViewModel(string filePath) {
            this.FilePath = filePath;
        }

        public static int CompareFolder(BaseResultViewModel a, BaseResultViewModel b) {
            if (a is FolderResultViewModel) {
                if (b is FolderResultViewModel) {
                    return 0;
                }
                else {
                    return -1;
                }
            }
            else if (b is FolderResultViewModel) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public static int CompareFolderAndName(BaseResultViewModel a, BaseResultViewModel b) {
            int compared = CompareFolder(a, b);
            return compared != 0 ? compared : StringComparer.Ordinal.Compare(a.FileName, b.FileName);
        }

        public static int CompareFolderAndFileExtension(BaseResultViewModel a, BaseResultViewModel b) {
            int compared = CompareFolder(a, b);
            if (compared != 0) {
                return compared;
            }

            return StringComparer.Ordinal.Compare(Path.GetExtension(a.FileName) ?? "", Path.GetExtension(b.FileName) ?? "");
        }

        public static int CompareFileName(BaseResultViewModel a, BaseResultViewModel b) {
            return StringComparer.Ordinal.Compare(a.FileName ?? "", b.FileName ?? "");
        }
        
        public static int CompareTimeCreated(BaseResultViewModel a, BaseResultViewModel b) {
            return DateTime.Compare(a.timeCreated, b.timeCreated);
        }
        
        public static int CompareTimeModified(BaseResultViewModel a, BaseResultViewModel b) {
            return DateTime.Compare(a.timeLastModified, b.timeLastModified);
        }
        
        public static int CompareTimeAccessed(BaseResultViewModel a, BaseResultViewModel b) {
            return DateTime.Compare(a.timeLastAccessed, b.timeLastAccessed);
        }
    }
}
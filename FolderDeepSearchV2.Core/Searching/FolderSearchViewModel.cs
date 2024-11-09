using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderDeepSearchV2.Core.Searching.Results;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.Commands;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching {
    public class FolderSearchViewModel : BaseViewModel {
        public static FolderSearchViewModel MockViewModel { get; }

        static FolderSearchViewModel() {
            MockViewModel = new FolderSearchViewModel();
            MockViewModel.ResultList.Results.Add(new FolderResultViewModel("C:\\Okay\\Then"));
            MockViewModel.ResultList.Results.Add(new FolderResultViewModel("C:\\Okay\\Then\\HHHH"));
            MockViewModel.ResultList.Results.Add(new FileResultViewModel("C:\\Okay\\no_u.png"));
            MockViewModel.ResultList.Results.Add(new FileResultViewModel("C:\\Okay\\Then\\HHHH\\fef\\s.txt"));
            MockViewModel.ResultList.Results.Add(new FileResultViewModel("C:\\Okay\\Then\\HHHH\\gre.txt"));
        }

        private string targetDirectory;
        public string TargetDirectory {
            get => this.targetDirectory;
            set => this.RaisePropertyChanged(ref this.targetDirectory, value);
        }

        private string searchTerm;
        public string SearchTerm {
            get => this.searchTerm;
            set => this.RaisePropertyChanged(ref this.searchTerm, value);
        }

        private string searchTermExtension;
        public string SearchTermExtension {
            get => this.searchTermExtension;
            set {
                if (!string.IsNullOrEmpty(value) && value[0] == '.') {
                    value = value.Substring(1);
                }

                this.RaisePropertyChanged(ref this.searchTermExtension, value);

                if (this.ignoreFileExtension && !string.IsNullOrEmpty(value)) {
                    this.IgnoreFileExtension = false;
                }
            }
        }

        private bool searchFolderNames;
        public bool SearchFolderNames {
            get => this.searchFolderNames;
            set => this.RaisePropertyChanged(ref this.searchFolderNames, value);
        }

        private bool searchFileNames;
        public bool SearchFileNames {
            get => this.searchFileNames;
            set => this.RaisePropertyChanged(ref this.searchFileNames, value);
        }

        private bool searchFileContents;
        public bool SearchFileContents {
            get => this.searchFileContents;
            set => this.RaisePropertyChanged(ref this.searchFileContents, value);
        }

        private bool searchRecursively;
        public bool SearchRecursively {
            get => this.searchRecursively;
            set => this.RaisePropertyChanged(ref this.searchRecursively, value);
        }

        private bool caseSensitive;
        public bool CaseSensitive {
            get => this.caseSensitive;
            set => this.RaisePropertyChanged(ref this.caseSensitive, value);
        }

        private bool ignoreFileExtension;
        public bool IgnoreFileExtension {
            get => this.ignoreFileExtension;
            set => this.RaisePropertyChanged(ref this.ignoreFileExtension, value);
        }

        private bool isSearching;
        public bool IsSearching {
            get => this.isSearching;
            set {
                this.RaisePropertyChanged(ref this.isSearching, value);
                if (value) {
                    this.TotalFileCount = 0;
                    this.TotalFolderCount = 0;
                }

                // Cannot do this; a deadlock is risked
                // ServiceManager.App.Invoke(() => {
                //     this.SelectDirectoryCommand.RaiseCanExecuteChanged();
                //     this.ToggleCaseSensitivityCommand.RaiseCanExecuteChanged();
                //     this.ToggleResursiveSearchCommand.RaiseCanExecuteChanged();
                //     this.ToggleNameStartsWithCommand.RaiseCanExecuteChanged();
                // });
            }
        }

        private bool useNameStartsWithTerm;
        public bool UseNameStartsWithTerm {
            get => this.useNameStartsWithTerm;
            set {
                if (value == this.useNameStartsWithTerm)
                    return;
                
                this.searchTypeInternal = SearchType.StartsWith;
                if (value && this.useExactSearchTerm)
                    this.RaisePropertyChanged(ref this.useExactSearchTerm, false, nameof(this.UseExactSearchTerm));
                this.RaisePropertyChanged(ref this.useNameStartsWithTerm, value);
                if (!value && !this.useExactSearchTerm)
                    this.searchTypeInternal = SearchType.Contains;
            }
        }

        private bool useExactSearchTerm;
        public bool UseExactSearchTerm {
            get => this.useExactSearchTerm;
            set {
                if (value == this.useExactSearchTerm)
                    return;
                
                this.searchTypeInternal = SearchType.Exact;
                if (value && this.useNameStartsWithTerm)
                    this.RaisePropertyChanged(ref this.useNameStartsWithTerm, false, nameof(this.UseNameStartsWithTerm));
                this.RaisePropertyChanged(ref this.useExactSearchTerm, value);
                if (!value && !this.useNameStartsWithTerm)
                    this.searchTypeInternal = SearchType.Contains;
            }
        }

        private bool ignoreSecurityErrors;
        public bool IgnoreSecurityErrors {
            get => this.ignoreSecurityErrors;
            set => this.RaisePropertyChanged(ref this.ignoreSecurityErrors, value);
        }
        
        private SearchType searchTypeInternal;

        public ResultListViewModel ResultList { get; }

        public ProgressViewModel Progress { get; }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SelectDirectoryCommand { get; }

        public ICommand ToggleCaseSensitivityCommand { get; }
        public ICommand ToggleResursiveSearchCommand { get; }
        public ICommand ToggleNameStartsWithCommand { get; }

        private string currentPath;
        public string CurrentPath {
            get => this.currentPath;
            set => this.RaisePropertyChanged(ref this.currentPath, value);
        }

        private int totalFileCount;
        public int TotalFileCount {
            get => this.totalFileCount;
            set => this.RaisePropertyChanged(ref this.totalFileCount, value);
        }

        private int totalFolderCount;
        public int TotalFolderCount {
            get => this.totalFolderCount;
            set => this.RaisePropertyChanged(ref this.totalFolderCount, value);
        }

        private volatile CancellationTokenSource cancellation;

        private volatile Task searchTask;

        public FolderSearchViewModel() {
            this.ToggleCaseSensitivityCommand = new RelayCommand(() => {
                if (!this.IsSearching)
                    this.CaseSensitive = !this.CaseSensitive;
            });
            this.ToggleResursiveSearchCommand = new RelayCommand(() => {
                if (!this.IsSearching)
                    this.SearchRecursively = !this.SearchRecursively;
            });
            this.ToggleNameStartsWithCommand = new RelayCommand(() => {
                if (!this.IsSearching)
                    this.UseNameStartsWithTerm = !this.UseNameStartsWithTerm;
            });

            this.StartCommand = new RelayCommand(this.StartSearchAction);
            this.StopCommand = new RelayCommand(this.StopSearchAction);
            this.SelectDirectoryCommand = new RelayCommand(() => {
                if (!this.IsSearching && ServiceManager.IoDialogs.SelectFolder(this.TargetDirectory, out string folder)) {
                    this.TargetDirectory = folder;
                }
            });
            this.SearchFileNames = true;
            this.SearchFolderNames = true;

            this.SearchRecursively = true;
            this.IgnoreFileExtension = true;

            this.IgnoreSecurityErrors = true;

            this.Progress = new ProgressViewModel(this);
            this.ResultList = new ResultListViewModel(this);
            this.searchTypeInternal = SearchType.Contains;
        }

        public void CancelAndWait() {
            if (this.IsSearching) {
                this.cancellation?.Cancel(false);
            }
        }

        public void StopSearchAction() {
            this.CancelAndWait();
        }

        public void StartSearchAction() {
            this.CancelAndWait();
            this.RunSearch();
        }

        private void RunSearch() {
            this.ResultList.Clear();
            if (string.IsNullOrEmpty(this.SearchTerm)) {
                ServiceManager.Messages.Show("Empty search term", "Empty search term");
                return;
            }

            if (!Directory.Exists(this.TargetDirectory)) {
                ServiceManager.Messages.Show("No such directory", "The directory does not exist: " + this.targetDirectory);
                return;
            }

            this.cancellation = new CancellationTokenSource();
            this.IsSearching = true;
            this.searchTask = Task.Run(async () => {
                try {
                    await this.RunSearchAsync(this.TargetDirectory, this.SearchTerm);
                }
                catch (SearchCancelledException) {
                    // ignored; but throw for any other type
                }
                catch (Exception e) {
                    // Crash in debug mode
                    // Display error at runtime
#if DEBUG
                    ServiceManager.App.Invoke(() => throw new Exception("Failed to search", e));
#else
                    ServiceManager.Messages.Show("Failed to search", 
                        "An error occoured while searching. \n" +
                        e.Message + "\n" +
                        e.StackTrace);
#endif
                }
            }).ContinueWith(x => {
                this.IsSearching = false;
                this.cancellation = null;
                this.ResultList.InsertQueueIntoResultList();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private Task RunSearchAsync(string directory, string search) {
            try {
                this.SearchDirectory(directory, search, this.SearchRecursively, this.cancellation.Token);
            }
            finally {
                this.CurrentPath = null;
            }

            return Task.CompletedTask;
        }

        private void SearchDirectory(string directory, string term, bool deep, in CancellationToken token) {
            if (token.IsCancellationRequested) {
                throw new SearchCancelledException();
            }

            this.CurrentPath = directory;
            bool names = this.searchFolderNames;

            IEnumerable<string> folders;
            try {
                folders = Directory.EnumerateDirectories(directory);
            }
            catch (PathTooLongException e) {
                ServiceManager.Messages.Show("Path is too long", "Failed to get directory enumerator for " + directory + "\n" + e.Message);
                throw new SearchCancelledException();
            }
            catch (UnauthorizedAccessException e) {
                if (this.IgnoreSecurityErrors) {
                    // simple way of bypassing the folder search
                    // cba to implement a better way :----)
                    folders = Enumerable.Empty<string>();
                }
                else {
                    ServiceManager.Messages.Show("Folder inaccessible", "Failed to get directory enumerator for " + directory + "\n" + e.Message);
                    throw new SearchCancelledException();
                }
            }

            if (!string.IsNullOrWhiteSpace(this.searchTermExtension)) {
                names = false;
            }

            foreach (string path in folders) {
                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                if (names) {
                    if (this.MatchName(path, term, FileType.Directory, true)) {
                        this.ResultList.AddQueuedItem(new FolderResultViewModel(path));
                    }
                }

                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                if (deep) {
                    this.SearchDirectory(path, term, true, in token);
                }

                this.TotalFolderCount++;
            }

            if (token.IsCancellationRequested) {
                throw new SearchCancelledException();
            }

            // search files last
            this.SearchFiles(directory, term, token);
        }

        private void SearchFiles(string directory, string term, in CancellationToken token) {
            if (token.IsCancellationRequested) {
                throw new SearchCancelledException();
            }

            bool names = this.searchFileNames;
            bool contents = this.searchFileContents;
            bool ignoreType = this.ignoreFileExtension;
            bool ignoreCase = !this.caseSensitive;

            IEnumerable<string> files;
            try {
                files = Directory.EnumerateFiles(directory);
            }
            catch (PathTooLongException e) {
                ServiceManager.Messages.Show("Path is too long", "Failed to get file enumerator for " + directory + "\n" + e.Message);
                throw new SearchCancelledException();
            }
            catch (UnauthorizedAccessException e) {
                if (this.IgnoreSecurityErrors) {
                    // simple way of bypassing the folder search
                    // cba to implement a better way :----)
                    files = Enumerable.Empty<string>();
                }
                else {
                    ServiceManager.Messages.Show("Folder inaccessible", "Failed to get file enumerator for " + directory + "\n" + e.Message);
                    throw new SearchCancelledException();
                }
            }

            foreach (string file in files) {
                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                if (names) {
                    if (this.MatchName(file, term, FileType.File, ignoreType)) {
                        this.ResultList.AddQueuedItem(new FileResultViewModel(file));
                        continue;
                    }
                }

                if (contents) {
                    this.CurrentPath = file;
                    const int size = 16384;
                    int offset = term.Length;
                    char[] buffer = new char[size + offset]; // 9 with "hello"
                    int length = buffer.Length;
                    using (StreamReader reader = new StreamReader(File.OpenRead(file))) {
                        // size = 4, term = "89", buffer = 6 long
                        // iteration0: 0,1,2,3,4,5
                        // iteration1: 4,5,6,7,8,9
                        // iteration1: 8,9,a,b,c,?

                        // reads iteration0
                        int read = reader.Read(buffer, 0, buffer.Length);
                        if (read < 1) {
                            continue;
                        }

                        if (IndexOf(buffer, term, 0, ignoreCase) != -1) {
                            this.ResultList.AddQueuedItem(new FileResultViewModel(file));
                            continue;
                        }

                        // copy 4,5 to start of buffer
                        CopyEndToStart(buffer, offset);
                        // reads iteration1, starting at offset (2), reading size (4) chars
                        while ((read = reader.Read(buffer, offset, size)) > 0) {
                            if (IndexOf(buffer, 0, read == size ? length : (read + offset), term, 0, term.Length, 0, ignoreCase) != -1) {
                                this.ResultList.AddQueuedItem(new FileResultViewModel(file));
                                goto fileLoopEnd;
                            }

                            CopyEndToStart(buffer, offset);
                        }


                        // int read;
                        // bool first = true;
                        // while ((read = reader.Read(buffer, first ? 0 : offset, first ? buffer.Length : size)) > 0) {
                        //     // file contains "helloXD12345678"
                        //     // iteration0: reads h,e,l,l,o,X,D,1,2
                        //     // iteration1: reads o,X,D,1,2,3,4,5,6
                        //     // iteration2: reads 2,3,4,5,6,7,8,-,-
                        //     if (first) {
                        //         first = false;
                        //     }
                        //     else {
                        //         // copy offset amount of chars from end to start
                        //         Shift(buffer, offset, read);
                        //         // for (int i = 0, j = offset; i < read; i++) {
                        //         //     buffer[i] = buffer[j + i];
                        //         // }
                        //     }
                        //     if (IndexOf(buffer, 0, buffer.Length, term, 0, term.Length, 0, ignoreCase) != -1) {
                        //         this.ResultList.AppendResult(new FileResultViewModel(file));
                        //         goto end;
                        //     }
                        // }
                    }
                }

                this.TotalFileCount++;
                fileLoopEnd: ;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyEndToStart(char[] array, int count) {
            for (int i = 0, len = array.Length, j = len - count; i < count; i++) {
                array[i] = array[j + i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool MatchChar(char a, char b, bool ic) {
            if (a == b) {
                return true;
            }

            if (ic) {
                char a1 = char.ToLower(a);
                char b1 = char.ToLower(b);
                if (a1 == b1) {
                    return true;
                }

                if (char.ToUpper(a1) == char.ToUpper(b1)) {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexOf(char[] src, string tar, int fromIndex, bool ignoreCase) {
            return IndexOf(src, 0, src.Length, tar, 0, tar.Length, fromIndex, ignoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexOf(char[] src, int srcOff, int srcLen, string tar, int tarOff, int tarLen, int fromIndex, bool ignoreCase) {
            if (fromIndex >= srcLen) {
                return (tarLen == 0 ? srcLen : -1);
            }

            if (fromIndex < 0) {
                fromIndex = 0;
            }

            if (tarLen == 0) {
                return fromIndex;
            }

            char first = tar[tarOff];
            int max = srcOff + (srcLen - tarLen);

            for (int i = srcOff + fromIndex; i <= max; i++) {
                if (!MatchChar(src[i], first, ignoreCase)) {
                    while (++i <= max && !MatchChar(src[i], first, ignoreCase)) { }
                }

                if (i <= max) {
                    int j = i + 1;
                    int end = j + tarLen - 1;
                    for (int k = tarOff + 1; j < end && MatchChar(src[j], tar[k], ignoreCase); j++, k++) { }

                    if (j == end) {
                        return i - srcOff;
                    }
                }
            }

            return -1;
        }

        private enum SearchType {
            StartsWith,
            Contains,
            Exact
        }
        
        private static bool MatchString(string search, string term, bool caseSensitive, SearchType type) {
            StringComparison comparison = caseSensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            
            switch (type) {
                case SearchType.StartsWith: return search.StartsWith(term, comparison);
                case SearchType.Contains: return search.IndexOf(term, 0, comparison) != -1;
                case SearchType.Exact: return search.Equals(term, comparison);
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private bool MatchName(string path, string term, FileType type, bool ignoreExtension) {
            if (ignoreExtension) {
                return MatchString(Path.GetFileNameWithoutExtension(path), term, this.caseSensitive, this.searchTypeInternal);
            }
            else {
                string fileName = Path.GetFileName(path);
                if (string.IsNullOrEmpty(fileName)) {
                    return term == "";
                }

                if (MatchString(fileName, term, this.caseSensitive, this.searchTypeInternal)) {
                    string searchExtension = this.searchTermExtension;
                    if (string.IsNullOrWhiteSpace(searchExtension)) {
                        return true;
                    }

                    searchExtension = "." + searchExtension;

                    if (type != FileType.File) {
                        return false;
                    }

                    string extension = Path.GetExtension(fileName);
                    if (string.IsNullOrEmpty(extension)) {
                        return true;
                    }
                    
                    return extension.Equals(searchExtension, this.caseSensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
                }
                else {
                    return false;
                }
            }
        }
    }
}

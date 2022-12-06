using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
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
            set => this.RaisePropertyChanged(ref this.isSearching, value);
        }

        private bool nameStartsWithTerm;
        public bool NameStartsWithTerm {
            get => this.nameStartsWithTerm;
            set => this.RaisePropertyChanged(ref this.nameStartsWithTerm, value);
        }

        public ResultListViewModel ResultList { get; }

        public ProgressViewModel Progress { get; }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SelectDirectoryCommand { get; }

        private string currentPath;
        public string CurrentPath {
            get => this.currentPath;
            set => this.RaisePropertyChanged(ref this.currentPath, value);
        }

        private volatile CancellationTokenSource cancellation;

        private volatile Task searchTask;

        public FolderSearchViewModel() {
            this.StartCommand = new RelayCommand(this.StartSearchAction);
            this.StopCommand = new RelayCommand(this.StopSearchAction);
            this.SelectDirectoryCommand = new RelayCommand(() => {
                if (ServiceManager.IoDialogs.SelectFolder(this.TargetDirectory, out string folder)) {
                    this.TargetDirectory = folder;
                }
            });
            this.SearchFileNames = true;
            this.SearchFolderNames = true;

            this.SearchRecursively = false;
            this.IgnoreFileExtension = true;

            this.Progress = new ProgressViewModel(this);
            this.ResultList = new ResultListViewModel(this);
        }

        public void CancelAndWait() {
            if (this.IsSearching) {
                this.cancellation?.Cancel(false);
                this.searchTask?.Wait();
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

            this.searchTask = Task.Run(async () => {
                try {
                    this.IsSearching = true;
                    await this.RunSearchAsync(this.TargetDirectory, this.SearchTerm);
                }
                catch (SearchCancelledException) {
                    // ignored; but throw for any other type
                }
                finally {
                    this.IsSearching = false;
                }
            });
        }

        private Task RunSearchAsync(string directory, string search) {
            this.cancellation = new CancellationTokenSource();
            try {
                this.SearchDirectory(directory, search, this.SearchRecursively, this.cancellation.Token);
            }
            finally {
                this.cancellation = null;
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
            foreach (string path in Directory.EnumerateDirectories(directory)) {
                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                string name = Path.GetFileName(path) ?? path; // just in case
                if (names) {
                    if (this.MatchName(name, term, FileType.Directory)) {
                        this.ResultList.AppendResult(new FolderResultViewModel(path));
                    }
                }

                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                if (deep) {
                    this.SearchDirectory(path, term, true, in token);
                }
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

            foreach (string file in Directory.EnumerateFiles(directory)) {
                if (token.IsCancellationRequested) {
                    throw new SearchCancelledException();
                }

                if (names) {
                    string target = (ignoreType ? Path.GetFileNameWithoutExtension(file) : Path.GetFileName(file)) ?? file; // just in case
                    if (this.MatchName(target, term, FileType.File)) {
                        this.ResultList.AppendResult(new FileResultViewModel(file));
                        continue;
                    }
                }

                if (contents) {
                    this.CurrentPath = file;
                    const int size = 1024;
                    int offset = term.Length;
                    char[] buffer = new char[size + offset]; // 9 with "hello"
                    int length = buffer.Length;
                    using (StreamReader reader = new StreamReader(new BufferedStream(File.OpenRead(file), 2048))) {
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
                            this.ResultList.AppendResult(new FileResultViewModel(file));
                            continue;
                        }

                        // copy 4,5 to start of buffer
                        CopyEndToStart(buffer, offset);
                        // reads iteration1, starting at offset (2), reading size (4) chars
                        while ((read = reader.Read(buffer, offset, size)) > 0) {
                            if (IndexOf(buffer, 0, read == size ? length : (read + offset), term, 0, term.Length, 0, ignoreCase) != -1) {
                                this.ResultList.AppendResult(new FileResultViewModel(file));
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

        private bool MatchName(string source, string search, FileType type) {
            if (this.caseSensitive) {
                return this.nameStartsWithTerm ? source.StartsWith(search) : source.Contains(search);
            }
            else if (this.nameStartsWithTerm) {
                return source.StartsWith(search, StringComparison.CurrentCultureIgnoreCase);
            }
            else {
                return source.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1;
            }
        }
    }
}

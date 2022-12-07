using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderDeepSearchV2.Core.Searching.Results;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.Commands;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching {
    public class ResultListViewModel : BaseViewModel {
        public FolderSearchViewModel Searcher { get; }

        public ObservableCollection<BaseResultViewModel> Results { get; }

        public ICommand ClearCommand { get; }
        public ICommand SortByNameACommand { get; }
        public ICommand SortByNameBCommand { get; }
        public ICommand SortByTypeCommand { get; }
        public ICommand SortByExtensionCommand { get; }
        public ICommand SortByNameCommand { get; }

        private readonly Queue<BaseResultViewModel> resultQueue;

        /// <summary>
        /// A lock used to synchronise access over the collections of items
        /// </summary>
        private readonly object collectionLock = new object();

        public ResultListViewModel(FolderSearchViewModel folder) {
            this.Searcher = folder;
            this.Results = new ObservableCollection<BaseResultViewModel>();
            this.resultQueue = new Queue<BaseResultViewModel>(512);
            this.ClearCommand = new RelayCommand(this.Clear);
            this.SortByNameACommand = new RelayCommand(() => this.Sort(true));
            this.SortByNameBCommand = new RelayCommand(() => this.Sort(false));

            this.SortByTypeCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFolder));
            this.SortByExtensionCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFolderAndFileExtension));
            this.SortByNameCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFileName));

            Task.Run(async () => {
                IAppProxy app = ServiceManager.App;
                while (app.IsRunning()) {
                    ServiceManager.App.Invoke(this.TickAddResults);
                    await Task.Delay(100);
                }
            });
        }

        private void Sort(Comparison<BaseResultViewModel> comparer) {
            lock (this.collectionLock) {
                this.TickAddResults(this.resultQueue.Count);
                List<BaseResultViewModel> list = new List<BaseResultViewModel>(this.Results);
                list.Sort(comparer);
                this.Results.Clear();
                foreach (BaseResultViewModel result in list) {
                    this.Results.Add(result);
                }
            }
        }

        private void TickAddResults() {
            this.TickAddResults(20);
        }

        private void TickAddResults(int additionLimit) {
            lock (this.collectionLock) {
                if (this.resultQueue.Count == 0) {
                    return;
                }

                for (int i = 0, count = Math.Min(additionLimit, this.resultQueue.Count); i < count; i++) {
                    this.Results.Add(this.resultQueue.Dequeue());
                }

                this.resultQueue.Clear();
            }
        }

        public void AppendResult(BaseResultViewModel result) {
            lock (this.collectionLock) {
                this.resultQueue.Enqueue(result);
            }
        }

        private void Sort(bool ascending) {
            lock (this.collectionLock) {
                List<BaseResultViewModel> list = (ascending ? this.Results.OrderBy(a => a.FileName) : this.Results.OrderByDescending(a => a.FileName)).ToList();
                this.Results.Clear();
                foreach (BaseResultViewModel result in list) {
                    this.Results.Add(result);
                }
            }
        }

        public void Clear() {
            // do this first, in case lock is acquired after this.Clear() but before this.Results.Clear()
            lock (this.collectionLock) {
                this.resultQueue.Clear();
                this.Results.Clear();
            }
        }
    }
}
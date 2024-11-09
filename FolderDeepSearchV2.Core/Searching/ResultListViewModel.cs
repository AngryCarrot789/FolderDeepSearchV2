using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderDeepSearchV2.Core.Searching.Results;
using FolderDeepSearchV2.Core.Services;
using REghZy.MVVM.Commands;
using REghZy.MVVM.IoC;
using REghZy.MVVM.ViewModels;

namespace FolderDeepSearchV2.Core.Searching {
    public class ResultListViewModel : BaseViewModel {
        public FolderSearchViewModel Searcher { get; }

        public ObservableCollection<BaseResultViewModel> Results { get; }

        public ICommand ClearCommand { get; }
        public ICommand ReverseListCommand { get; }
        public ICommand SortByNameACommand { get; }
        public ICommand SortByNameBCommand { get; }
        public ICommand SortByTypeCommand { get; }
        public ICommand SortByExtensionCommand { get; }
        public ICommand SortByTimeCreatedCommand { get; }
        public ICommand SortByTimeModifiedCommand { get; }
        public ICommand SortByTimeAccessedCommand { get; }
        public ICommand SortByNameCommand { get; }

        private readonly ConcurrentQueue<BaseResultViewModel> resultQueue;
        private volatile int IsInsertQueueCallbackScheduled;
        private readonly object locker = new object();

        public ResultListViewModel(FolderSearchViewModel folder) {
            this.Searcher = folder;
            this.Results = new ObservableCollection<BaseResultViewModel>();
            this.resultQueue = new ConcurrentQueue<BaseResultViewModel>();
            this.ClearCommand = new RelayCommand(this.Clear);
            this.SortByNameACommand = new RelayCommand(() => this.Sort(true));
            this.SortByNameBCommand = new RelayCommand(() => this.Sort(false));
            this.ReverseListCommand = new RelayCommand(this.ReverseList);

            this.SortByTypeCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFolder));
            this.SortByExtensionCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFolderAndFileExtension));
            this.SortByNameCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareFileName));
            this.SortByTimeCreatedCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareTimeCreated));
            this.SortByTimeModifiedCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareTimeModified));
            this.SortByTimeAccessedCommand = new RelayCommand(() => this.Sort(BaseResultViewModel.CompareTimeAccessed));
        }

        private void Sort(Comparison<BaseResultViewModel> comparer) {
            this.InsertQueueIntoResultList();
            List<BaseResultViewModel> list = new List<BaseResultViewModel>(this.Results);
            list.Sort(comparer);
            this.Results.Clear();
            foreach (BaseResultViewModel result in list) {
                this.Results.Add(result);
            }
        }

        public void AddQueuedItem(BaseResultViewModel item) {
            if (item != null) {
                this.resultQueue.Enqueue(item);
            }

            lock (this.locker) {
                if (Interlocked.CompareExchange(ref this.IsInsertQueueCallbackScheduled, 1, 0) == 0) {
                    Task.Run(async () => {
                        await Task.Delay(50);
                        ServiceManager.App.InvokeAsync(() => this.InsertQueueIntoResultList(25));
                    });
                }
            }
        }

        public void InsertQueueIntoResultList(int max = int.MaxValue) {
            lock (this.locker) {
                int i = 0;
                while (++i <= max && this.resultQueue.TryDequeue(out BaseResultViewModel value))
                    this.Results.Add(value);
                this.IsInsertQueueCallbackScheduled = 0;
            }
        }

        private void Sort(bool ascending) {
            List<BaseResultViewModel> list = (ascending ? this.Results.OrderBy(a => a.FileName) : this.Results.OrderByDescending(a => a.FileName)).ToList();
            this.Results.Clear();
            foreach (BaseResultViewModel result in list) {
                this.Results.Add(result);
            }
        }
        
        private void ReverseList() {
            List<BaseResultViewModel> list = this.Results.Reverse().ToList();
            this.Results.Clear();
            foreach (BaseResultViewModel result in list) {
                this.Results.Add(result);
            }
        }

        public void Clear() {
            // do this first, in case lock is acquired after this.Clear() but before this.Results.Clear()
            while (this.resultQueue.TryDequeue(out _)) {
            }

            this.Results.Clear();
        }
    }
}
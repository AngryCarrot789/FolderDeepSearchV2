using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FolderDeepSearchV2.Imaging {
    /// <summary>
    /// Async icon resolver - made by REghZy :)
    ///
    /// Probably very inefficienly made but it's better than the entire app being
    /// halted while an icon is fetched
    ///
    /// It works by the AsyncImage class enqueueing an icon fetch
    /// </summary>
    public class FileIconService {
        private volatile bool canFileThreadRun;
        public bool CanFileThreadRun {
            get => this.canFileThreadRun;
            set => this.canFileThreadRun = value;
        }

        private volatile bool canDirectoryThreadRun;
        public bool CanDirectoryThreadRun {
            get => this.canDirectoryThreadRun;
            set => this.canDirectoryThreadRun = value;
        }

        private volatile bool canUpdateTaskRun;
        public bool CanUpdateTaskRun {
            get => this.canUpdateTaskRun;
            set => this.canUpdateTaskRun = value;
        }

        public static FileIconService Instance { get; } = new FileIconService();

        private readonly Thread fileThread;
        private readonly Thread directoryThread;
        private readonly Thread updateThread;
        private readonly ConcurrentQueue<QueuedIconResolution> fileQueue;
        private readonly ConcurrentQueue<QueuedIconResolution> directoryQueue;
        private readonly ConcurrentQueue<PendingIconDelivery> updateQueue;

        public FileIconService() {
            this.fileQueue = new ConcurrentQueue<QueuedIconResolution>();
            this.directoryQueue = new ConcurrentQueue<QueuedIconResolution>();
            this.updateQueue = new ConcurrentQueue<PendingIconDelivery>();

            this.canFileThreadRun = true;
            this.canDirectoryThreadRun = true;
            this.canUpdateTaskRun = true;

            this.fileThread = new Thread(this.FileQueueThreadMain);
            this.directoryThread = new Thread(this.DirectoryQueueThreadMain);
            this.updateThread = new Thread(this.UpdateMain);

            this.fileThread.Start();
            this.directoryThread.Start();
            this.updateThread.Start();
        }

        public void Shutdown() {
            this.canUpdateTaskRun = false;
            this.canFileThreadRun = false;
            this.canDirectoryThreadRun = false;
        }

        public void ClearAll() {
            while (this.fileQueue.TryDequeue(out _)) { }
            while (this.directoryQueue.TryDequeue(out _)) { }
            while (this.updateQueue.TryDequeue(out _)) { }
        }

        private void UpdateMain() {
            while (this.canUpdateTaskRun) {
                int size = Math.Min(this.updateQueue.Count, 20);
                if (size > 0) { // Checking here saves constantly invoking a new task every 50ms; slight performance help
                    Application.Current.Dispatcher.Invoke(() => {
                        for (int i = 0; i < size; i++) {
                            if (this.updateQueue.TryDequeue(out PendingIconDelivery pair) && pair.imageable.IsValid) {
                                pair.Deliver();
                            }
                            else {
                                break;
                            }
                        }
                    });
                }

                Thread.Sleep(50);
            }
        }

        private void FileQueueThreadMain() {
            while (this.canFileThreadRun) {
                int count = Math.Min(this.fileQueue.Count, 5);
                if (count > 0) {
                    Application.Current.Dispatcher.Invoke(() => {
                        for (int i = 0; i < count; i++) {
                            if (this.fileQueue.TryDequeue(out QueuedIconResolution control) && control.imageable.IsValid) {
                                string path = control.path;
                                if (File.Exists(path)) {
                                    this.updateQueue.Enqueue(new PendingIconDelivery(control.imageable, ShellUtils.GetFileIconAsBitmapSource(path, control.iconType, false)));
                                }
                            }
                        }
                    });
                }

                Thread.Sleep(10);
            }
        }

        private void DirectoryQueueThreadMain() {
            while (this.canDirectoryThreadRun) {
                int count = Math.Min(this.directoryQueue.Count, 5);
                if (count > 0) {
                    // due to reasons that i have yet to confirm, this has to be done on the UI thread
                    Application.Current.Dispatcher.Invoke(() => {
                        for (int i = 0; i < count; i++) {
                            if (this.directoryQueue.TryDequeue(out QueuedIconResolution control) && control.imageable.IsValid) {
                                string path = control.path;
                                if (Directory.Exists(path)) {
                                    BitmapSource source;
                                    if (control.iconType == IconType.Large) {
                                        source = ShellEx.GetBitmapSourceForPath(path, false, true);
                                    }
                                    else {
                                        source = ShellUtils.GetFileIconAsBitmapSource(path, control.iconType, true);
                                    }

                                    this.updateQueue.Enqueue(new PendingIconDelivery(control.imageable, source));
                                }
                            }
                        }
                    });
                }

                Thread.Sleep(10);
            }
        }

        public void EnqueueForIconResolution(string path, IImageable control, bool forceFile = false, bool forceDirectory = false, IconType iconType = IconType.Normal) {
            if (forceFile || forceDirectory) {
                if (forceFile) {
                    this.fileQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
                else {
                    this.directoryQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
            }
            else if (!string.IsNullOrEmpty(path)) {
                if (File.Exists(path)) {
                    this.fileQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
                else if (Directory.Exists(path)) {
                    this.directoryQueue.Enqueue(new QueuedIconResolution(path, control, iconType));
                }
            }
        }

        private readonly struct QueuedIconResolution {
            public readonly string path;
            public readonly IImageable imageable;
            public readonly IconType iconType;

            public QueuedIconResolution(string path, IImageable imageable, IconType iconType) {
                this.path = path;
                this.imageable = imageable;
                this.iconType = iconType;
            }
        }

        private readonly struct PendingIconDelivery {
            public readonly IImageable imageable;
            public readonly ImageSource image;

            public PendingIconDelivery(IImageable imageable, ImageSource image) {
                this.imageable = imageable;
                this.image = image;
            }

            public void Deliver() {
                this.imageable.Source = this.image;
            }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using FolderDeepSearchV2.Imaging;

namespace FolderDeepSearchV2.Controls {
    public class AsyncImage : Image, IImageable {
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register(
                "FilePath",
                typeof(string),
                typeof(AsyncImage),
                new PropertyMetadata(null, (d,e) => ((AsyncImage) d).OnFilePathChanged((string) e.OldValue, (string) e.NewValue)));

        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register(
                "IconType",
                typeof(IconType),
                typeof(AsyncImage),
                new PropertyMetadata(IconType.Normal));

        public string FilePath {
            get => (string) this.GetValue(FilePathProperty);
            set => this.SetValue(FilePathProperty, value);
        }

        public IconType IconType {
            get => (IconType) this.GetValue(IconTypeProperty);
            set => this.SetValue(IconTypeProperty, value);
        }

        public bool IsValid => true;

        private bool triggerUpdateOnLoad;

        public AsyncImage() {
            this.Loaded += this.OnControlLoaded;
        }

        public void FetchIcon() {
            if (this.IsLoaded) {
                FileIconService.Instance.EnqueueForIconResolution(this.FilePath, this, false, false, this.IconType);
                this.triggerUpdateOnLoad = false;
            }
            else {
                this.triggerUpdateOnLoad = true;
            }
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e) {
            if (this.triggerUpdateOnLoad) {
                this.triggerUpdateOnLoad = false;
                this.FetchIcon();
            }
        }

        private void OnFilePathChanged(string oldPath, string newPath) {
            if (oldPath == newPath || newPath == null) {
                return;
            }

            this.FetchIcon();
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using FolderDeepSearchV2.Core.Searching;
using FolderDeepSearchV2.Core.Searching.Results;
using FolderDeepSearchV2.Windows;

namespace FolderDeepSearchV2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase {
        public MainWindow() {
            InitializeComponent();

            // CancellationTokenSource source = new CancellationTokenSource();
            // Task.Run(async () => {
            //     int count = 0;
            //     while (true) {
            //         if (source.IsCancellationRequested) {
            //             Debug.WriteLine("Cancel requested! " + count);
            //             return;
            //         }
            //
            //         Debug.WriteLine("hello! " + count++);
            //         await Task.Delay(1000);
            //     }
            // }, source.Token);
            //
            // Task.Run(async () => {
            //     await Task.Delay(2500);
            //     source.Cancel(true);
            // });

            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            this.SearchTermBox.Focus();
            this.SearchTermBox.SelectAll();
        }

        private void WindowBase_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Escape) {
                if (this.DataContext is FolderSearchViewModel search) {
                    search.CancelAndWait();
                }

                Application.Current.Shutdown();
            }
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (sender is ListBox list) {
                if (list.SelectedItem is FileResultViewModel file) {
                    file.OpenFileAction();
                }
                else if (list.SelectedItem is FolderResultViewModel folder) {
                    folder.OpenFolderAction();
                }
            }
        }
    }
}

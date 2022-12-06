using System;
using System.IO;
using System.Windows;
using FolderDeepSearchV2.Core.Searching;
using FolderDeepSearchV2.Core.Services;
using FolderDeepSearchV2.Imaging;
using FolderDeepSearchV2.Services;

namespace FolderDeepSearchV2 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static volatile bool IS_RUNNING = false;

        private void Application_Startup(object sender, StartupEventArgs e) {
            // init app
            IS_RUNNING = true;

            ServiceManager.Set<IDialogMessages>(new WPFDialogMessages());
            ServiceManager.Set<IAppProxy>(new WPFAppProxy());
            ServiceManager.Set<IIODialogs>(new WPFIODialogs());
            ServiceManager.Set<IClipboard>(new CSClipboard());

            string directory = null;
            string cmdLine = string.Join(" ", e.Args);
            if (!string.IsNullOrWhiteSpace(cmdLine)) {
                if (Directory.Exists(cmdLine)) {
                    directory = cmdLine;
                }
                else if (File.Exists(cmdLine)) {
                    directory = Path.GetDirectoryName(cmdLine);
                }
                else {
                    ServiceManager.Get<IDialogMessages>().Show("Cannot open non-existent directory", "No such directory: " + cmdLine);
                }

                // set target file
            }

            // run
            this.MainWindow = new MainWindow();

            FolderSearchViewModel vm = new FolderSearchViewModel();
            vm.TargetDirectory = string.IsNullOrWhiteSpace(directory) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : directory;
            this.MainWindow.DataContext = vm;
            this.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            IS_RUNNING = false;
            FileIconService.Instance.Shutdown();
            base.OnExit(e);
        }
    }
}

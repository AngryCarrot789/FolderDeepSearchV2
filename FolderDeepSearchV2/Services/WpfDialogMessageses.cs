using System;
using System.Threading.Tasks;
using System.Windows;
using FolderDeepSearchV2.Core.Services;

namespace FolderDeepSearchV2.Services {
    public class WPFDialogMessages : IDialogMessages {
        public void Show(string caption, string message) {
            RunOnThreadUI(() => MessageBox.Show(message, caption));
        }

        public bool ShowYesNo(string caption, string message, bool defaultResult = false) {
            return MessageBoxResult.Yes == RunOnThreadUI(() => MessageBox.Show(message, caption, MessageBoxButton.YesNo,MessageBoxImage.Information, defaultResult ? MessageBoxResult.Yes : MessageBoxResult.No));
        }

        public bool? ShowYesNoCancel(string caption, string message, bool? defaultResult = false) {
            MessageBoxResult result = RunOnThreadUI(() => MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Information, defaultResult == null ? MessageBoxResult.Cancel : ((defaultResult == true) ? MessageBoxResult.Yes : MessageBoxResult.No)));
            switch (result) {
                case MessageBoxResult.Yes: return true;
                case MessageBoxResult.No: return false;
                default: return null;
            }
        }

        public Task ShowAsync(string caption, string message) {
            return Application.Current.Dispatcher.InvokeAsync(() => MessageBox.Show(message, caption)).Task;
        }

        private static void RunOnThreadUI(Action action) {
            TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();
            Application.Current.Dispatcher.Invoke(() => {
                try {
                    action();
                }
                finally {
                    result.TrySetResult(true);
                }
            });

            result.Task.Wait();
        }

        private static T RunOnThreadUI<T>(Func<T> action) {
            return Application.Current.Dispatcher.Invoke(action);
        }
    }
}
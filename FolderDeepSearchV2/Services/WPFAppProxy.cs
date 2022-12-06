using System;
using System.Windows;
using System.Windows.Threading;
using FolderDeepSearchV2.Core.Services;

namespace FolderDeepSearchV2.Services {
    public class WPFAppProxy : IAppProxy {
        public void Invoke(Action action) {
            Application.Current.Dispatcher.Invoke(action);
        }

        public bool IsRunning() {
            return App.IS_RUNNING;

            // This does work... somehow. Too wasteful though; most likely case is true
            // requiring an exception instance every time
            // try {
            //     Application app = Application.Current;
            //     if (app != null) {
            //         app.ShutdownMode = app.ShutdownMode;
            //     }
            //
            //     return false;
            // }
            // catch (Exception) {
            //     return true;
            // }
        }
    }
}

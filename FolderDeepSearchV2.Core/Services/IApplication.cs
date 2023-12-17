using System;

namespace FolderDeepSearchV2.Core.Services {
    public interface IApplication {
        void Invoke(Action action);
        void InvokeAsync(Action action);
        bool IsRunning();
    }
}

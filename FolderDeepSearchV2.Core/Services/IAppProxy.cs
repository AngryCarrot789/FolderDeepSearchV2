using System;

namespace FolderDeepSearchV2.Core.Services {
    public interface IAppProxy {
        void Invoke(Action action);
        bool IsRunning();
    }
}

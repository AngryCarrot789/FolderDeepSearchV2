using System.Windows;
using FolderDeepSearchV2.Core.Services;

namespace FolderDeepSearchV2.Services {
    public class CSClipboard : IClipboard {
        public void SetText(string value) {
            Clipboard.SetText(value);
        }
    }
}
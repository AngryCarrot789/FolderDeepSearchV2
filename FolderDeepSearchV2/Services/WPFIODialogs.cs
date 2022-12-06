using FolderDeepSearchV2.Core.Services;
using Ookii.Dialogs.Wpf;

namespace FolderDeepSearchV2.Services {
    public class WPFIODialogs : IIODialogs {
        public bool SelectFolder(string defaultFolder, out string target) {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            if (defaultFolder != null) {
                ofd.SelectedPath = defaultFolder;
            }

            ofd.Multiselect = false;
            ofd.Description = "Select a directory to open";
            ofd.UseDescriptionForTitle = true;
            if (ofd.ShowDialog() == true) {
                target = ofd.SelectedPath;
                return true;
            }
            else {
                target = null;
                return false;
            }
        }
    }
}
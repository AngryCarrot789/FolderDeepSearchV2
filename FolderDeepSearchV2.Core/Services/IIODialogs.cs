namespace FolderDeepSearchV2.Core.Services {
    public interface IIODialogs {
        bool SelectFolder(string defaultFolder, out string target);
    }
}
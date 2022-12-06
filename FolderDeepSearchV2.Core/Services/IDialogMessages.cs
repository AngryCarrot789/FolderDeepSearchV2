namespace FolderDeepSearchV2.Core.Services {
    public interface IDialogMessages {
        void Show(string caption, string message);
        bool ShowYesNo(string caption, string message, bool defaultResult = false);
        bool? ShowYesNoCancel(string caption, string message, bool? defaultResult = false);
    }
}
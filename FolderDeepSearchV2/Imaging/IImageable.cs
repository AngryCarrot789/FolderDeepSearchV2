using System.Windows.Media;

namespace FolderDeepSearchV2.Imaging {
    public interface IImageable {
        /// <summary>
        /// This <see cref="IImageable"/>'s image source
        /// </summary>
        ImageSource Source { get; set; }

        /// <summary>
        /// Whether this <see cref="IImageable"/> is valid and setting <see cref="Source"/> would have a meaningful effect
        /// </summary>
        bool IsValid { get; }
    }
}
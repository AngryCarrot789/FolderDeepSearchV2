using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace FolderDeepSearchV2.Windows {
    public class WindowBase : Window {
        public static readonly DependencyProperty TitlebarColourProperty =
            DependencyProperty.Register(
                "TitlebarColour",
                typeof(Brush),
                typeof(WindowBase),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Gray)));

        [Category("Brush")]
        public Brush TitlebarColour {
            get => (Brush) GetValue(TitlebarColourProperty);
            set => SetValue(TitlebarColourProperty, value);
        }

        protected WindowBase() {
            // TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        }
    }
}

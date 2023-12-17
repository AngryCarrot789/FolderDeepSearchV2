using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace FolderDeepSearchV2.Converters {
    public class PathToExtensionConverter : IValueConverter {
        public static PathToExtensionConverter Instance { get; } = new PathToExtensionConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str) {
                return Path.GetExtension(str);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

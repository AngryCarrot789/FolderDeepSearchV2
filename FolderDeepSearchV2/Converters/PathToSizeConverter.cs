using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace FolderDeepSearchV2.Converters {
    public class PathToSizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string path) {
                if (File.Exists(path)) {
                    return new FileInfo(path).Length;
                }
                else {
                    return -1;
                }
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

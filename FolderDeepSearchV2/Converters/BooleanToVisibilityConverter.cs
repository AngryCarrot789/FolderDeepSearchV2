using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FolderDeepSearchV2.Converters {
    public class BooleanToVisibilityConverter : IValueConverter {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }
        public Visibility NonBooleanValue { get; set; }
        public Visibility UnsetValue { get; set; }

        public BooleanToVisibilityConverter() {
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Collapsed;
            this.NonBooleanValue = Visibility.Hidden;
            this.UnsetValue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue) {
                return this.UnsetValue;
            }
            else if (value is bool state) {
                return state ? this.TrueValue : this.FalseValue;
            }
            else {
                return this.NonBooleanValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == DependencyProperty.UnsetValue) {
                return DependencyProperty.UnsetValue;
            }
            else if (value is Visibility visibility) {
                if (visibility == this.TrueValue) {
                    return true;
                }
                else if (visibility == this.FalseValue) {
                    return false;
                }
                else if (visibility == this.NonBooleanValue) {
                    return null;
                }
                else {
                    return DependencyProperty.UnsetValue;
                }
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}

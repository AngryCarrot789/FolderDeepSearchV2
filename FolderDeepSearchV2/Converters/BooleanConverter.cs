using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FolderDeepSearchV2.Converters {
    public class BooleanConverter : IValueConverter {
        public static BooleanConverter InvertBool => new BooleanConverter {TrueValue = false, FalseValue = true};

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object NonBooleanValue { get; set; }
        public object UnsetValue { get; set; }

        public BooleanConverter() {

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
            else if (value == this.TrueValue) {
                return true;
            }
            else if (value == this.FalseValue) {
                return false;
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace FolderDeepSearchV2.Converters {
    public class CollectionTypeCountConverter : IValueConverter {
        public Type TargetType { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
             if (value is ICollection collection) {
                int count = 0;
                foreach (object obj in collection) {
                    if (this.TargetType.IsInstanceOfType(obj)) {
                        count++;
                    }
                }

                return count;
            }
            else {
                throw new Exception("Value is not a collection: " + value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

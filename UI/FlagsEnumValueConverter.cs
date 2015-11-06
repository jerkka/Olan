using System;
using System.Globalization;
using System.Windows.Data;

namespace Olan.UI {
    public class FlagsEnumValueConverter : IValueConverter {
        #region Fields

        private int _targetValue;

        #endregion
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var mask = (int)Enum.Parse(value.GetType(), (string)parameter);
            _targetValue = (int)value;
            return ((mask & _targetValue) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            _targetValue ^= (int)Enum.Parse(targetType, (string)parameter);
            return Enum.Parse(targetType, _targetValue.ToString());
        }

        #endregion
    }
}
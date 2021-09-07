using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class IsEqualIntegerConverter : IValueConverter
    {
        private int _defaultIntegerToCompareTo = 0;
        public string DefaultIntegerToCompareTo 
        {
            get { return _defaultIntegerToCompareTo.ToString(); }
            set { _defaultIntegerToCompareTo = int.Parse(value); }
        } 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int integerValue;
            if (!int.TryParse(value.ToString(), out integerValue))
                return false;

            int compareTo;
            if (parameter == null || !int.TryParse(parameter.ToString(), out compareTo))
                compareTo = _defaultIntegerToCompareTo;

            return integerValue == compareTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

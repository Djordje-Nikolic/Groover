using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{

    public class TimeSpanConverter : IValueConverter
    {
        public const string FormatLessThanHour = "mm':'ss";
        public const string FormatMoreThanHour = "hh':'mm':'ss";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan? timeSpan = (TimeSpan?)value;
            if (timeSpan == null)
                return "";

            if (timeSpan.Value.Hours > 0)
                return timeSpan.Value.ToString(FormatMoreThanHour);
            else
                return timeSpan.Value.ToString(FormatLessThanHour);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? timeSpan = (string?)value;
            if (timeSpan == null)
                return TimeSpan.FromSeconds(0);

            TimeSpan ts;
            if (!TimeSpan.TryParseExact(timeSpan, FormatMoreThanHour, CultureInfo.CurrentCulture, out ts))
            {
                TimeSpan.TryParseExact(timeSpan, FormatLessThanHour, CultureInfo.CurrentCulture, out ts);
            }

            return ts;
        }
    }
}

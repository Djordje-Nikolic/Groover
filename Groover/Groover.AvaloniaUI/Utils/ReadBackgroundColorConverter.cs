using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Groover.AvaloniaUI.Utils
{
    public class ReadBackgroundColorConverter : IValueConverter
    {
        public SolidColorBrush ReadColor { get; set; }
        public SolidColorBrush UnreadColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool isRead))
            {
                if (isRead)
                    return ReadColor;
                else
                    return UnreadColor;
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

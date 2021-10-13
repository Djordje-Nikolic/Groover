using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class MessageHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object isSender, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)isSender)
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Left;
        }

        public object ConvertBack(object horizontalAlignment, Type targetType, object parameter, CultureInfo culture)
        {
            return (HorizontalAlignment)horizontalAlignment == HorizontalAlignment.Right;
        }
    }
}

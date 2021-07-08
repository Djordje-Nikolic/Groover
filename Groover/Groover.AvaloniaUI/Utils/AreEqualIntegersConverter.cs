using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class AreEqualIntegersConverter : IMultiValueConverter
    {
        //If parameter is true, the end value will be inverted
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            bool.TryParse(parameter?.ToString(), out invert);

            int count = values.Count();
            if (count >= 2)
            {
                int firstVal = int.Parse(values[0].ToString());
                for (int i = 1; i < count; i++)
                {
                    if (firstVal != int.Parse(values[i].ToString()))
                        return false ^ invert;
                }
                return true ^ invert;
            }
            else
                return true ^ invert;
        }
    }
}

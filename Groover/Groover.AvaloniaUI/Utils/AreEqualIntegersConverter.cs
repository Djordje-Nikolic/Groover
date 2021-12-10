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

            try
            {
                int count = values.Count();
                if (count >= 2)
                {
                    if (!int.TryParse(values[0].ToString(), out int firstVal))
                        return false;

                    for (int i = 1; i < count; i++)
                    {
                        if (!int.TryParse(values[i].ToString(), out int secondVal))
                            return false;

                        if (firstVal != secondVal)
                            return false ^ invert;
                    }
                    return true ^ invert;
                }
                else
                    return true ^ invert;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

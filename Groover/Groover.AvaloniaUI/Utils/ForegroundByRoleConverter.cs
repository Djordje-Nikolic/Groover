using Avalonia.Data.Converters;
using Avalonia.Media;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class ForegroundByRoleConverter : IValueConverter
    {
        public const string MemberForegroundName = "DarkGray";
        public const string AdminForegroundName = "LightGray";

        public SolidColorBrush MemberForeground { get; }
        public SolidColorBrush AdminForeground { get; }

        public ForegroundByRoleConverter()
        {
            MemberForeground = SolidColorBrush.Parse(MemberForegroundName);
            AdminForeground = SolidColorBrush.Parse(AdminForegroundName);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GrooverGroupRole? type = (GrooverGroupRole?)value;

            switch (type)
            {
                case GrooverGroupRole.Admin:
                    return AdminForeground;
                case GrooverGroupRole.Member:
                    return MemberForeground;
                case null:
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

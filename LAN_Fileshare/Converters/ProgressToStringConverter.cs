using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LAN_Fileshare.Converters
{
    public class ProgressToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int progress)
            {
                return $"{progress}%";
            }
            return "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

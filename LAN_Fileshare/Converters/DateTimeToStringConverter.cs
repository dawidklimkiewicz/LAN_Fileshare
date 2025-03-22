using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LAN_Fileshare.Converters
{
    internal class DateTimeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            DateTime? time = value as DateTime?;
            if (time != null)
            {
                return time.Value.ToString("dd/MM/yyyy HH:mm:ss");
            }
            return "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

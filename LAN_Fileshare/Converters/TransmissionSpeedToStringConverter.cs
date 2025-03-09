using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LAN_Fileshare.Converters
{
    internal class TransmissionSpeedToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string[] Suffixes = ["B", "KB", "MB", "GB"];
            int Count = 0;

            if (value != null)
            {
                double Size = System.Convert.ToDouble(value);
                while (Size >= 1024)
                {
                    Size = Size / 1024;
                    Count++;
                }
                return $"{Size.ToString("0.00")} {Suffixes[Count]}/s";
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

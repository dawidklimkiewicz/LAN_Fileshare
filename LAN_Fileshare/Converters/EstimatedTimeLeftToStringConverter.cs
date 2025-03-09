using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LAN_Fileshare.Converters
{
    internal class EstimatedTimeLeftToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan timeRemaining = (TimeSpan)value;

                if (timeRemaining.Hours > 0)
                {
                    return $"{timeRemaining.Hours}h {timeRemaining.Minutes}m";
                }
                else if (timeRemaining.Minutes > 0)
                {
                    return $"{timeRemaining.Minutes}m {timeRemaining.Seconds}s";
                }
                else if (timeRemaining.Seconds > 0)
                {
                    return $"{timeRemaining.Seconds}s";
                }
                else
                {
                    return "";
                }
            }

            return "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

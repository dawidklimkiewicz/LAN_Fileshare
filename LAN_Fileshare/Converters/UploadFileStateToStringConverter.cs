using Avalonia.Data.Converters;
using LAN_Fileshare.Models;
using System;
using System.Globalization;

namespace LAN_Fileshare.Converters
{
    public class UploadFileStateToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FileState state)
            {
                return state switch
                {
                    FileState.Paused => "Paused",
                    FileState.Transmitting => "Uploading",
                    FileState.Finished => "Finished",
                    _ => ""
                };
            }
            return "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace OneCleaner
{
    [ValueConversion(typeof(Status), typeof(string))]
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Status)value == Status.Uninstalling)
                return "Отменить";
            else
                return "Удалить";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


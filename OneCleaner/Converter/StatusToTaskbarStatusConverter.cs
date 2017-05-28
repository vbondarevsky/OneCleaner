using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace OneCleaner
{
    [ValueConversion(typeof(Status), typeof(TaskbarItemProgressState))]
    public class StatusToTaskbarStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Status)value == Status.Uninstalling)
                return TaskbarItemProgressState.Normal;
            else
                return TaskbarItemProgressState.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


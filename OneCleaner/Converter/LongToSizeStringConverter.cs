using System;
using System.Globalization;
using System.Windows.Data;

namespace OneCleaner
{
    [ValueConversion(typeof(long), typeof(string))]
    public class LongToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sizeString = "";

            double sizeInKb = (long)value / 1024d;

            if ((long)value == 0)
                sizeString = string.Format("{0:0.0} B", (long)value);
            else if(sizeInKb < 1000)
                sizeString = string.Format("{0:0.0} KB", sizeInKb);
            else if (sizeInKb < 1000000)
                sizeString = string.Format("{0:0.0} MB", sizeInKb / 1024d);
            else
                sizeString = string.Format("{0:0.0} GB", sizeInKb / (1024d * 1024d));

            return sizeString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
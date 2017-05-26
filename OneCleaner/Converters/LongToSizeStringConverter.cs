using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OneCleaner.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class LongToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sizeString = "";

            long sizeInKb = (long)value / 1024;

            if ((long)value == 0)
                sizeString = string.Format("{0} B", (long)value);
            else if(sizeInKb < 1000)
                sizeString = string.Format("{0} KB", sizeInKb);
            else
                sizeString = string.Format("{0} MB", sizeInKb / 1024);

            return sizeString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
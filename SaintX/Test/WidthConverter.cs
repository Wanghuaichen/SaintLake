using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Test
{
    [ValueConversion(typeof(Int32), typeof(Int32))]
    class WidthConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            int width = 0;
            int.TryParse(value.ToString(), out width);
            width = Math.Max(width, 120);
            return width / 6;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace kinect2_nidaq.Views.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) { return true; }
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) { return false; }
            return !((bool)value);
        }
    }
}

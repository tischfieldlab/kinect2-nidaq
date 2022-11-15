using Sensor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace kinect2_nidaq.Views.Converters
{
    [ValueConversion(typeof(DepthFrameEventArgs), typeof(ImageSource))]
    public class DepthFrameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DepthFrameEventArgs depthFrame = (DepthFrameEventArgs)value;
            if (depthFrame != null)
                return depthFrame.ToBitmap();
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kinect2_nidaq.Views
{
    // https://stackoverflow.com/questions/14584662/enable-a-child-control-when-the-parent-is-disabled
    public class ResetIsEnabled : ContentControl
    {
        static ResetIsEnabled()
        {
            IsEnabledProperty.OverrideMetadata(
                typeof(ResetIsEnabled),
                new UIPropertyMetadata(
                    defaultValue: true,
                    propertyChangedCallback: (_, __) => { },
                    coerceValueCallback: (_, x) => x));
        }
    }
}

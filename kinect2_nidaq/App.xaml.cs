using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace kinect2_nidaq
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App() : base()
        {
            GlobalFFOptions.Configure(new FFOptions {
                BinaryFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "External", "ffmpeg-4.2-win-64")
            });
        }
    }
}

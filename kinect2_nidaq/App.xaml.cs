using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "C:\\Users\\thackray\\Downloads\\ffmpeg-4.2-win-64" });
        }
    }
}

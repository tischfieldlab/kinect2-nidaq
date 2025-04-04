using Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace kinect2_nidaq.Models
{
    public interface IKinectDevice
    {
        event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        event EventHandler<IRFrameEventArgs> IRFrameProduced;
        event EventHandler ColorFrameDropped;
        event EventHandler DepthFrameDropped;
        event EventHandler IRFrameDropped;


        void Initialize();
        void Start();
        void Stop();


        bool IsInitialized { get; }
        bool IsColorStreamEnabled { get; set; }
        ColorInfo ColorInfo { get; }
        bool IsDepthStreamEnabled { get; set; }
        DepthInfo DepthInfo { get; }
        bool IsIRStreamEnabled { get; set; }
        bool IsIRStreamSupported { get; }
        IRInfo IRInfo { get; }
    }

    public class ColorInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BytesPerPixel { get { return (Format.BitsPerPixel + 7) / 8; } }
        public PixelFormat Format { get; set; }
        public int FPS { get; set; }
        public int Size { get { return Width * Height * BytesPerPixel; } }
    }

    public class DepthInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int FPS { get; set; }
        public int Size { get { return Width * Height * 2; } }
    }

    public class IRInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int FPS { get; set; }
        public int Size { get { return Width * Height * 2; } }
    }

    public class DeviceNotFoundException : ApplicationException { }
}

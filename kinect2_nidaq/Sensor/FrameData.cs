using System;
using kinect2_nidaq.Models;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Kinect;

namespace Sensor
{

    public class ColorFrameEventArgs// : FrameEventArgs
    {
        public ColorInfo ColorInfo { get; set; }
        public DepthInfo DepthInfo { get; set; }
        public byte[] ColorData { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public double TimeStamp { get; set; }
    }

    public class K2ColorFrameEventArgs : ColorFrameEventArgs
    {
        public ColorSpacePoint[] ColorSpacepoints { get; set; }
    }

    public class K4AColorFrameEventArgs : ColorFrameEventArgs
    {
        public Transformation Transform { get; set; }
    }

    public class DepthFrameEventArgs //: FrameEventArgs
    {
        public DepthInfo DepthInfo { get; set; }
        public ushort[] DepthData { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public double TimeStamp { get; set; }

        public ushort DepthMinReliableDistance { get; set; }
        public ushort DepthMaxReliableDistance { get; set; }
    }

    public class IRFrameEventArgs //: FrameEventArgs
    {
        public IRInfo IRInfo { get; set; }
        public ushort[] IRData { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public double TimeStamp { get; set; }
    }
}


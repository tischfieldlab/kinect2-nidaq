using Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public interface IKinectDevice
    {
        event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        event EventHandler ColorFrameDropped;
        event EventHandler DepthFrameDropped;


        void Initialize();
        void Start();
        void Stop();


        bool IsInitialized { get; }
        bool IsColorStreamEnabled { get; set; }
        bool IsDepthStreamEnabled { get; set; }
    }

    public class DeviceNotFoundException : ApplicationException { }
}

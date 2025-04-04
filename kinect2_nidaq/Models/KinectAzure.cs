using Microsoft.Azure.Kinect.Sensor;
using Sensor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class KinectAzure : IKinectDevice
    {
        private Device _sensor;
        private bool _isInitialized;
        private bool _isOpen;

        private bool _isColorStreamEnabled;
        private bool _isDepthStreamEnabled;
        private bool _isIRStreamEnabled;

        private Transformation _transform;

        public KinectAzure()
        {
        }

        public event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        public event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        public event EventHandler<IRFrameEventArgs> IRFrameProduced;
        public event EventHandler ColorFrameDropped;
        public event EventHandler DepthFrameDropped;
        public event EventHandler IRFrameDropped;

        public ColorInfo ColorInfo { get; } = new ColorInfo()
        {
            Width = 2048,
            Height = 1536,
            Format = System.Windows.Media.PixelFormats.Bgra32,
            FPS = 30
        };
        public DepthInfo DepthInfo { get; } = new DepthInfo()
        {
            Width = 640,
            Height = 576,
            FPS = 30
        };
        public IRInfo IRInfo { get; } = new IRInfo()
        {
            Width = 640,
            Height = 576,
            FPS = 30
        };



        public bool IsInitialized { get => this._isInitialized; }
        public bool IsColorStreamEnabled
        {
            get => this._isColorStreamEnabled;
            set
            {
                if (value != this._isColorStreamEnabled)
                {
                    if (this._isOpen)
                    {
                        throw new ApplicationException("Cannot enable/disable color stream while device is open!");
                    }
                    this._isColorStreamEnabled = value;
                }
            }
        }
        public bool IsDepthStreamEnabled {
            get => this._isDepthStreamEnabled;
            set
            {
                if (value != this._isDepthStreamEnabled)
                {
                    if (this._isOpen)
                    {
                        throw new ApplicationException("Cannot enable/disable depth stream while device is open!");
                    }
                    this._isDepthStreamEnabled = value;
                }
            }
        }
        public bool IsIRStreamSupported { get { return true; } }
        public bool IsIRStreamEnabled
        {
            get => this._isIRStreamEnabled;
            set
            {
                if (value != this._isIRStreamEnabled)
                {
                    if (this._isOpen)
                    {
                        throw new ApplicationException("Cannot enable/disable IR stream while device is open!");
                    }
                    this._isIRStreamEnabled = value;
                }
            }
        }

        public void Initialize()
        {
            int numDevices = Device.GetInstalledCount();
            if (Device.GetInstalledCount() <= 0)
            {
                throw new DeviceNotFoundException();
            }
            this._sensor = Device.Open(0);
            
            this._isInitialized = true;
        }

        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            this._sensor.StartCameras(new DeviceConfiguration
            {
                CameraFPS = FPS.FPS30,
                DisableStreamingIndicator = true,
                ColorFormat = Microsoft.Azure.Kinect.Sensor.ImageFormat.ColorBGRA32,
                ColorResolution = this.IsColorStreamEnabled ? ColorResolution.R1536p : ColorResolution.Off,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = (this.IsColorStreamEnabled && this.IsDepthStreamEnabled) ? true : false,
            });
            this._transform = this._sensor.GetCalibration().CreateTransformation();
            this._isOpen = true;
            this.Run_Capture();
        }

        public void Stop()
        {
            this._isOpen = false;
            this._sensor.StopCameras();
            this._sensor.Dispose();
            this._isInitialized = false;
        }


        private void Run_Capture()
        {
            Task.Run(() =>
            {
                while (this._isOpen)
                {
                    using (Capture capture = this._sensor.GetCapture())
                    {
                        this.ColorFrameArrived(capture);
                        this.DepthFrameArrived(capture);
                        this.IRFrameArrived(capture);
                    }
                }
            });
        }


        private void ColorFrameArrived(Capture data)
        {
            if (data.Color == null)
            {
                this.ColorFrameDropped?.Invoke(this, new EventArgs());
            }
            else
            {
                var colorEventArgs = new K4AColorFrameEventArgs
                {
                    //ColorData = data.Color.Memory.ToArray().copy,
                    //colorEventArgs.TimeStamp = CurrentNITimeStamp;
                    RelativeTime = data.Color.DeviceTimestamp,
                    //colorEventArgs.Transform = 
                    //colorEventArgs.Image = this.transform.ColorImageToDepthCamera(data);
                    ColorInfo = this.ColorInfo,
                    DepthInfo = this.DepthInfo,
                    Transform = this._transform
                };
                colorEventArgs.ColorData = new byte[this.ColorInfo.Size];
                data.Color.Memory.ToArray().CopyTo(colorEventArgs.ColorData, 0);

                this.ColorFrameProduced?.Invoke(this, colorEventArgs);
            }
        }

        private void DepthFrameArrived(Capture data)
        {
            if (data == null)
            {
                this.DepthFrameDropped?.Invoke(this, new EventArgs());
            }
            else
            {
                var depthEventArgs = new DepthFrameEventArgs
                {
                    RelativeTime = data.Depth.DeviceTimestamp,
                    //DepthData = data.Depth.GetPixels<ushort>().ToArray(),
                    //depthEventArgs.TimeStamp = CurrentNITimeStamp;
                    DepthInfo = this.DepthInfo
                };
                depthEventArgs.DepthData = new ushort[this.DepthInfo.Width * this.DepthInfo.Height];
                data.Depth.GetPixels<ushort>().ToArray().CopyTo(depthEventArgs.DepthData, 0);

                this.DepthFrameProduced?.Invoke(this, depthEventArgs);
            }
        }

        private void IRFrameArrived(Capture data)
        {
            if (data == null)
            {
                this.IRFrameDropped?.Invoke(this, new EventArgs());
            }
            else
            {
                var IREventArgs = new IRFrameEventArgs
                {
                    RelativeTime = data.IR.DeviceTimestamp,
                    //IRData = data.IR.GetPixels<ushort>().ToArray(),
                    //depthEventArgs.TimeStamp = CurrentNITimeStamp;
                    IRInfo = this.IRInfo
                };
                IREventArgs.IRData = new ushort[this.IRInfo.Size];
                data.IR.GetPixels<ushort>().ToArray().CopyTo(IREventArgs.IRData, 0);

                this.IRFrameProduced?.Invoke(this, IREventArgs);
            }
        }
    }
}

using Microsoft.Kinect;
using Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class KinectV2 : IKinectDevice
    {
        private KinectSensor _sensor;
        private bool _isInitialized;
        private bool _isOpen;

        private bool _isColorStreamEnabled;
        private ColorFrameReader _colorFrameReader;
        private ColorSpacePoint[] _colorSpacePoints;

        private bool _isDepthStreamEnabled;
        private DepthFrameReader _depthFrameReader;

        public KinectV2()
        {

        }

        public event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        public event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        public event EventHandler ColorFrameDropped;
        public event EventHandler DepthFrameDropped;
        public event EventHandler<IRFrameEventArgs> IRFrameProduced;
        public event EventHandler IRFrameDropped;

        public ColorInfo ColorInfo {
            get
            {
                return new ColorInfo() {
                    Width = 1920,
                    Height = 1080,
                    Format = System.Windows.Media.PixelFormats.Bgra32,
                    FPS = 30
                };
            }
        }
        public DepthInfo DepthInfo
        {
            get
            {
                return new DepthInfo()
                {
                    Width = 512,
                    Height = 424,
                    //Format = System.Windows.Media.PixelFormats.Bgr32,
                    FPS = 30
                };
            }
        }


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

        public bool IsIRStreamEnabled { get => false; set {/* do nothing */} }

        public bool IsIRStreamSupported => false;

        public IRInfo IRInfo => throw new NotImplementedException();

        public void Initialize()
        {
            bool sensorAvailable = false;
            this._sensor = KinectSensor.GetDefault();
            if (this._sensor != null)
            {
                //this._sensor.Open();
                //Thread.Sleep(100);
                sensorAvailable = true; //this._sensor.IsAvailable;
                //this._sensor.Close();                
            } 

            if (sensorAvailable)
            {
                this._isInitialized = true;
            }
            else
            {
                throw new DeviceNotFoundException();
            }
        }

        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            if (this.IsColorStreamEnabled)
            {
                this._colorFrameReader = this._sensor.ColorFrameSource.OpenReader();
                this._colorFrameReader.FrameArrived += ColorReader_FrameArrived;
            }

            if (this.IsDepthStreamEnabled)
            {
                this._depthFrameReader = this._sensor.DepthFrameSource.OpenReader();
                this._depthFrameReader.FrameArrived += DepthReader_FrameArrived;
            }

            this._sensor.Open();
            this._isOpen = true;
        }

        public void Stop()
        {
            this._sensor.Close();
            this._isOpen = false;
            this._isInitialized = false;

            if (this.IsColorStreamEnabled)
            {
                this._colorFrameReader.Dispose();
                this._colorFrameReader = null;
            }
            if (this.IsDepthStreamEnabled)
            {
                this._depthFrameReader.Dispose();
                this._depthFrameReader = null;
            }
        }


        private void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // grab and dispatch
            using (ColorFrame frame = e.FrameReference.AcquireFrame())
            {

                if (frame == null)
                {
                    this.ColorFrameDropped?.Invoke(this, new EventArgs());
                }
                else
                {
                    var colorEventArgs = new K2ColorFrameEventArgs
                    {
                        RelativeTime = frame.RelativeTime,
                        ColorData = new byte[this.ColorInfo.Size],
                        ColorInfo = this.ColorInfo,
                        DepthInfo = this.DepthInfo,
                        ColorSpacepoints = this._colorSpacePoints,
                        // colorEventArgs.TimeStamp = CurrentNITimeStamp; // TODO
                    };

                    if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        frame.CopyRawFrameDataToArray(colorEventArgs.ColorData);
                    }
                    else
                    {
                        frame.CopyConvertedFrameDataToArray(colorEventArgs.ColorData, ColorImageFormat.Bgra);
                    }

                    this.ColorFrameProduced?.Invoke(this, colorEventArgs);
                }
            }
        }

        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame frame = e.FrameReference.AcquireFrame())
            {
                var depthEventArgs = new DepthFrameEventArgs();

                if (frame == null)
                {
                    this.DepthFrameDropped?.Invoke(this, new EventArgs());
                }
                else
                {
                    depthEventArgs.RelativeTime = frame.RelativeTime;
                    depthEventArgs.DepthData = new ushort[this.DepthInfo.Width * this.DepthInfo.Height];
                    frame.CopyFrameDataToArray(depthEventArgs.DepthData);

                    // Only map if we're also recording RGB, otherwise not reason to care...
                    if (IsColorStreamEnabled == true)
                    {
                        this._colorSpacePoints = new ColorSpacePoint[depthEventArgs.DepthData.Length];
                        this._sensor.CoordinateMapper.MapDepthFrameToColorSpace(depthEventArgs.DepthData, this._colorSpacePoints);
                    }

                    // depthEventArgs.TimeStamp = CurrentNITimeStamp; // TODO
                    depthEventArgs.DepthInfo = this.DepthInfo;
                    depthEventArgs.DepthMinReliableDistance = frame.DepthMinReliableDistance;
                    depthEventArgs.DepthMaxReliableDistance = frame.DepthMaxReliableDistance;

                    this.DepthFrameProduced?.Invoke(this, depthEventArgs);
                }
            }
        }
    }
}

using Microsoft.Kinect;
using Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private byte[] _colorData;
        private ColorSpacePoint[] _colorSpacePoints;

        private bool _isDepthStreamEnabled;
        private DepthFrameReader _depthFrameReader;
        private ushort[] _depthData;

        public KinectV2()
        {
            this._colorData = new byte[Constants.kDefaultColorFrameHeight * Constants.kDefaultColorFrameWidth * Constants.kBytesPerPixel];
            this._depthData = new ushort[Constants.kDefaultFrameHeight * Constants.kDefaultFrameWidth];
        }

        public event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        public event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        public event EventHandler ColorFrameDropped;
        public event EventHandler DepthFrameDropped;


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

        public void Initialize()
        {
            this._sensor = KinectSensor.GetDefault();
            if (this._sensor != null && this._sensor.IsAvailable)
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

                var colorEventArgs = new ColorFrameEventArgs();
                colorEventArgs.RelativeTime = e.FrameReference.RelativeTime;
                // colorEventArgs.TimeStamp = CurrentNITimeStamp; // TODO

                if (frame == null)
                {
                    this.ColorFrameDropped?.Invoke(this, new EventArgs());
                }
                else
                {

                    if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        frame.CopyRawFrameDataToArray(this._colorData);
                    }
                    else
                    {
                        frame.CopyConvertedFrameDataToArray(this._colorData, ColorImageFormat.Bgra);
                    }

                    colorEventArgs.RelativeTime = frame.RelativeTime;
                    colorEventArgs.ColorData = this._colorData;
                    colorEventArgs.ColorSpacepoints = this._colorSpacePoints;
                    colorEventArgs.Height = frame.FrameDescription.Height;
                    colorEventArgs.Width = frame.FrameDescription.Width;
                    colorEventArgs.DepthWidth = Constants.kDefaultFrameWidth;
                    colorEventArgs.DepthHeight = Constants.kDefaultFrameHeight;

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
                    frame.CopyFrameDataToArray(this._depthData);

                    // Only map if we're also recording RGB, otherwise not reason to care...
                    if (IsColorStreamEnabled == true)
                    {
                        this._colorSpacePoints = new ColorSpacePoint[this._depthData.Length];
                        this._sensor.CoordinateMapper.MapDepthFrameToColorSpace(this._depthData, this._colorSpacePoints);
                    }

                    // depthEventArgs.TimeStamp = CurrentNITimeStamp; // TODO
                    depthEventArgs.DepthData = this._depthData;
                    depthEventArgs.Height = frame.FrameDescription.Height;
                    depthEventArgs.Width = frame.FrameDescription.Width;
                    depthEventArgs.DepthMinReliableDistance = frame.DepthMinReliableDistance;
                    depthEventArgs.DepthMaxReliableDistance = frame.DepthMaxReliableDistance;

                    this.DepthFrameProduced?.Invoke(this, depthEventArgs);
                }
            }
        }
    }
}

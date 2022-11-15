using Microsoft.Kinect;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kinect2_nidaq.ViewModels.KinectV2
{
    public class KinectViewModel : ObservableObject
    {
        private KinectSensor _sensor;
        private bool _isInitialized;

        private bool _isColorStreamEnabled;
        private ColorFrameReader _colorFrameReader;
        private BlockingCollection<ColorFrameEventArgs> _colorFrameCollection;
        private int _colorFramesDropped;
        private byte[] _colorData = new byte[Constants.kDefaultColorFrameHeight * Constants.kDefaultColorFrameWidth * Constants.kBytesPerPixel];
        private ColorFrameEventArgs _lastColorFrame;

        private bool _isDepthStreamEnabled;
        private DepthFrameReader _depthFrameReader;
        private BlockingCollection<DepthFrameEventArgs> _depthFrameCollection;
        private int _depthFramesDropped;
        private ushort[] _depthData = new ushort[Constants.kDefaultFrameHeight * Constants.kDefaultFrameWidth];
        private DepthFrameEventArgs _lastDepthFrame;

        private ColorSpacePoint[] _colorSpacePoints;

        private bool _flipFrameDisplay;
        private ushort _depthMinDisplay;
        private ushort _depthMaxDisplay;


        public KinectViewModel()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            this._sensor = KinectSensor.GetDefault();
            if (this._sensor != null)
            {
                this._isInitialized = true;
            }
            else
            {
                MessageBox.Show("No Kinect found");
            }
        }

        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            if (this.IsColorStreamEnabled)
            {
                this._colorFrameCollection = new BlockingCollection<ColorFrameEventArgs>(Constants.kMaxFrames);
                this._colorFrameReader = this._sensor.ColorFrameSource.OpenReader();
                this._colorFrameReader.FrameArrived += ColorReader_FrameArrived;
            }

            if (this.IsDepthStreamEnabled)
            {
                this._depthFrameCollection = new BlockingCollection<DepthFrameEventArgs>(Constants.kMaxFrames);
                this._depthFrameReader = this._sensor.DepthFrameSource.OpenReader();
                this._depthFrameReader.FrameArrived += DepthReader_FrameArrived;
            }

            this._sensor.Open();
        }

        

        public void Stop()
        {
            this._sensor.Close();

            if (this.IsColorStreamEnabled)
            {
                this._colorFrameCollection.CompleteAdding();
                this._colorFrameCollection.Dispose();
                this._colorFrameCollection = null;

                this._colorFrameReader.Dispose();
                this._colorFrameReader = null;
            }
            if (this.IsDepthStreamEnabled)
            {
                this._depthFrameCollection.CompleteAdding();
                this._depthFrameCollection.Dispose();
                this._depthFrameCollection = null;

                this._depthFrameReader.Dispose();
                this._depthFrameReader = null;
            }

        }

        
        public bool IsColorStreamEnabled
        {
            get => this._isColorStreamEnabled;
            set => this.SetField(ref this._isColorStreamEnabled, value);
        }
        public BlockingCollection<ColorFrameEventArgs> ColorStream
        {
            get => this._colorFrameCollection;
        }
        public int ColorFramesDropped
        {
            get => this._colorFramesDropped;
            set => this.SetField(ref this._colorFramesDropped, value);
        }
        public ColorFrameEventArgs LastColorFrame
        {
            get => this._lastColorFrame;
            set => this.SetField(ref this._lastColorFrame, value);
        }


        public bool IsDepthStreamEnabled
        {
            get => this._isDepthStreamEnabled;
            set => this.SetField(ref this._isDepthStreamEnabled, value);
        }
        public BlockingCollection<DepthFrameEventArgs> DepthStream
        {
            get => this._depthFrameCollection;
        }
        public int DepthFramesDropped
        {
            get => this._depthFramesDropped;
            set => this.SetField(ref this._depthFramesDropped, value);
        }
        public DepthFrameEventArgs LastDepthFrame
        {
            get => this._lastDepthFrame;
            set => this.SetField(ref this._lastDepthFrame, value);
        }


        public bool FlipFrameDisplay
        {
            get => this._flipFrameDisplay;
            set => this.SetField(ref this._flipFrameDisplay, value);
        }
        public ushort DepthMinValue
        {
            get => this._depthMinDisplay;
            set => this.SetField(ref this._depthMinDisplay, value, () => Properties.Settings.Default.DepthMinValue = value);
        }
        public ushort DepthMaxValue
        {
            get => this._depthMaxDisplay;
            set => this.SetField(ref this._depthMaxDisplay, value, () => Properties.Settings.Default.DepthMaxValue = value);
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
                    ColorFramesDropped++;
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

                    // update to include absolute timestamps with hi-rest stopwatch
                    this.LastColorFrame = colorEventArgs;

                    // don't add to the queue if we're in preview mode
                    this._colorFrameCollection.Add(colorEventArgs);

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
                    this.DepthFramesDropped++;
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

                    LastDepthFrame = depthEventArgs;

                    // don't add if we're in preview mode
                    if (/*IsRecordingEnabled && */IsDepthStreamEnabled) // TODO: respect recording
                    {
                        this._depthFrameCollection.Add(depthEventArgs);
                    }
                }
            }
        }
    }
}

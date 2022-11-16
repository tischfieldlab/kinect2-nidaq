using kinect2_nidaq.Models;
using Microsoft.Kinect;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace kinect2_nidaq.ViewModels
{
    public class KinectViewModel : ObservableObject, IDeviceViewModel
    {
        private IKinectDevice _sensor;
        private bool _isInitialized;

        private SettingsViewModel settings;

        private int _colorFramesDropped;
        private ColorFrameEventArgs _lastColorFrame;

        private int _depthFramesDropped;
        private DepthFrameEventArgs _lastDepthFrame;

        private bool _flipFrameDisplay;
        private ushort _depthMinDisplay;
        private ushort _depthMaxDisplay;


        public KinectViewModel(SettingsViewModel settings)
        {
            this.settings = settings;
            
        }


        public void Initialize()
        {
            this._sensor = new KinectV2();
            try
            {
                this._sensor.Initialize();
            }
            catch (DeviceNotFoundException)
            {
                MessageBox.Show("Unable to find Kinect Sensor.\n\nPlease connect a sensor and restart the program!",
                                "Kinect not found!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                App.Current.Shutdown();
            }


            this._sensor.IsColorStreamEnabled = this.settings.IsColorStreamEnabled;
            this._sensor.IsDepthStreamEnabled = this.settings.IsDepthStreamEnabled;

            if (this.settings.IsColorStreamEnabled)
            {
                if (!this.settings.IsPreviewMode)
                    this.ColorStream = new BlockingCollection<ColorFrameEventArgs>(Constants.kMaxFrames);
                this._sensor.ColorFrameDropped += this._sensor_ColorFrameDropped;
                this._sensor.ColorFrameProduced += this._sensor_ColorFrameProduced;
            }

            if (this.settings.IsDepthStreamEnabled)
            {
                if (!this.settings.IsPreviewMode)
                    this.DepthStream = new BlockingCollection<DepthFrameEventArgs>(Constants.kMaxFrames);
                this._sensor.DepthFrameDropped += this._sensor_DepthFrameDropped;
                this._sensor.DepthFrameProduced += this._sensor_DepthFrameProduced;
            }
            this._isInitialized = true;
        }


        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            this._sensor.Start();
        }

        public void Stop()
        {
            this._sensor.Stop();
            this._isInitialized = false;

            if (this.settings.IsColorStreamEnabled)
            {
                if (this.ColorStream != null)
                {
                    this.ColorStream.CompleteAdding();
                    this.ColorStream.Dispose();
                    this.ColorStream = null;
                }

                this._sensor.ColorFrameDropped -= this._sensor_ColorFrameDropped;
                this._sensor.ColorFrameProduced -= this._sensor_ColorFrameProduced;
            }
            if (this.settings.IsDepthStreamEnabled)
            {
                if (this.DepthStream != null)
                {
                    this.DepthStream.CompleteAdding();
                    this.DepthStream.Dispose();
                    this.DepthStream = null;
                }
                
                this._sensor.DepthFrameDropped -= this._sensor_DepthFrameDropped;
                this._sensor.DepthFrameProduced -= this._sensor_DepthFrameProduced;
            }
        }

        public BlockingCollection<ColorFrameEventArgs> ColorStream { get; private set; }
        public int ColorFramesDropped
        {
            get => this._colorFramesDropped;
            set => this.SetField(ref this._colorFramesDropped, value);
        }
        public ColorFrameEventArgs LastColorFrame
        {
            get => this._lastColorFrame;
            set
            {
                this.SetField(ref this._lastColorFrame, value);
                Task.Run(() => this.LastColorFrameBitmap = this._lastColorFrame.ToBitmap());
            }
        }
        public BitmapSource LastColorFrameBitmap
        {
            get => this._lastColorFrameBitmap;
            set => this.SetField(ref this._lastColorFrameBitmap, value);
        }
        protected BitmapSource _lastColorFrameBitmap;


        public BlockingCollection<DepthFrameEventArgs> DepthStream { get; private set; }
        public int DepthFramesDropped
        {
            get => this._depthFramesDropped;
            set => this.SetField(ref this._depthFramesDropped, value);
        }
        public DepthFrameEventArgs LastDepthFrame
        {
            get => this._lastDepthFrame;
            set
            {
                this.SetField(ref this._lastDepthFrame, value);
                Task.Run(() => this.LastDepthFrameBitmap = this._lastDepthFrame.ToBitmap());
            }
        }
        public BitmapSource LastDepthFrameBitmap
        {
            get => this._lastDepthFrameBitmap;
            set => this.SetField(ref this._lastDepthFrameBitmap, value);
        }
        protected BitmapSource _lastDepthFrameBitmap;


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


        private void _sensor_DepthFrameProduced(object sender, DepthFrameEventArgs e)
        {
            this.LastDepthFrame = e;
            if (this.DepthStream != null)
                this.DepthStream.Add(e);
        }

        private void _sensor_DepthFrameDropped(object sender, EventArgs e)
        {
            this.DepthFramesDropped++;
        }

        private void _sensor_ColorFrameProduced(object sender, ColorFrameEventArgs e)
        {
            this.LastColorFrame = e;
            if (this.ColorStream != null)
                this.ColorStream.Add(e);
        }

        private void _sensor_ColorFrameDropped(object sender, EventArgs e)
        {
            this.ColorFramesDropped++;
        }
    }
}

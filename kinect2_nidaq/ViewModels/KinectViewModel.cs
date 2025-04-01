using kinect2_nidaq.Models;
using Microsoft.Kinect;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private FPSMonitor _colorFPSMonitor;
        private double _colorFPS;

        private int _depthFramesDropped;
        private DepthFrameEventArgs _lastDepthFrame;
        private FPSMonitor _depthFPSMonitor;
        private double _depthFPS;

        private int _irFramesDropped;
        private IRFrameEventArgs _lastIRFrame;
        private FPSMonitor _irFPSMonitor;
        private double _irFPS;

        private bool _flipFrameDisplay;
        private ushort _depthMinDisplay;
        private ushort _depthMaxDisplay;

        public event EventHandler<ColorFrameEventArgs> ColorFrameProduced;
        public event EventHandler<DepthFrameEventArgs> DepthFrameProduced;
        public event EventHandler<IRFrameEventArgs> IRFrameProduced;


        public KinectViewModel(SettingsViewModel settings)
        {
            this.settings = settings;
            
        }

        public ColorInfo ColorInfo { get { return this._sensor.ColorInfo; } }
        public DepthInfo DepthInfo { get { return this._sensor.DepthInfo; } }
        public IRInfo IRInfo { get { return this._sensor.IRInfo; } }


        public void Initialize()
        {
            if (Properties.Settings.Default.DeviceType == "K4A")
            {
                this._sensor = new KinectAzure();
            }
            else if (Properties.Settings.Default.DeviceType == "KinectV2")
            {
                this._sensor = new KinectV2();
            }
            else
            {
                MessageBox.Show("Invalid value for DeviceType in application settings: \""+Properties.Settings.Default.DeviceType+"\"!",
                                "Expecting one of \"K4A\" or \"KinectV2\". Please update application settings and restart!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                App.Current.Shutdown();
            }
            
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
            this._sensor.IsIRStreamEnabled = this.settings.IsIRStreamEnabled;

            if (this.settings.IsColorStreamEnabled)
            {
                if (!this.settings.IsPreviewMode)
                    this.ColorStream = new BlockingCollection<ColorFrameEventArgs>(Constants.kMaxFrames);
                this._sensor.ColorFrameDropped += this._sensor_ColorFrameDropped;
                this._sensor.ColorFrameProduced += this._sensor_ColorFrameProduced;
                this._colorFPSMonitor = new FPSMonitor();
            }

            if (this.settings.IsDepthStreamEnabled)
            {
                if (!this.settings.IsPreviewMode)
                    this.DepthStream = new BlockingCollection<DepthFrameEventArgs>(Constants.kMaxFrames);
                this._sensor.DepthFrameDropped += this._sensor_DepthFrameDropped;
                this._sensor.DepthFrameProduced += this._sensor_DepthFrameProduced;
                this._depthFPSMonitor = new FPSMonitor();
            }

            if (this.settings.IsIRStreamEnabled)
            {
                if (!this.settings.IsPreviewMode)
                    this.IRStream = new BlockingCollection<IRFrameEventArgs>(Constants.kMaxFrames);
                this._sensor.IRFrameDropped += this._sensor_IRFrameDropped;
                this._sensor.IRFrameProduced += this._sensor_IRFrameProduced;
                this._irFPSMonitor = new FPSMonitor();
            }
            this._isInitialized = true;
        }


        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            this._sensor.Start();
            this.WatchFPS();
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
                    this.DepthStream = null;
                }
                
                this._sensor.DepthFrameDropped -= this._sensor_DepthFrameDropped;
                this._sensor.DepthFrameProduced -= this._sensor_DepthFrameProduced;
            }
            if (this.settings.IsIRStreamEnabled)
            {
                if (this.IRStream != null)
                {
                    this.IRStream.CompleteAdding();
                    this.IRStream = null;
                }

                this._sensor.IRFrameDropped -= this._sensor_IRFrameDropped;
                this._sensor.IRFrameProduced -= this._sensor_IRFrameProduced;
            }

            this.ColorFPS = 0;
            this.DepthFPS = 0;
            this.IRFPS = 0;
        }

        public BlockingCollection<ColorFrameEventArgs> ColorStream { get; private set; }
        public double ColorFPS
        {
            get => this._colorFPS;
            private set => this.SetField(ref this._colorFPS, value);
        }
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
        public double DepthFPS
        {
            get => this._depthFPS;
            private set => this.SetField(ref this._depthFPS, value);
        }
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


        public BlockingCollection<IRFrameEventArgs> IRStream { get; private set; }
        public double IRFPS
        {
            get => this._irFPS;
            private set => this.SetField(ref this._irFPS, value);
        }
        public int IRFramesDropped
        {
            get => this._irFramesDropped;
            set => this.SetField(ref this._irFramesDropped, value);
        }
        public IRFrameEventArgs LastIRFrame
        {
            get => this._lastIRFrame;
            set
            {
                this.SetField(ref this._lastIRFrame, value);
                Task.Run(() => this.LastIRFrameBitmap = this._lastIRFrame.ToBitmap());
            }
        }
        public BitmapSource LastIRFrameBitmap
        {
            get => this._lastIRFrameBitmap;
            set => this.SetField(ref this._lastIRFrameBitmap, value);
        }
        protected BitmapSource _lastIRFrameBitmap;


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
            this.DepthFrameProduced?.Invoke(this, e);
            this.LastDepthFrame = e;
            if (this.DepthStream != null)
            {
                this.DepthStream.Add(e);
                
            }
            this._depthFPSMonitor.add_sample(e.RelativeTime.TotalSeconds);
        }

        private void _sensor_DepthFrameDropped(object sender, EventArgs e)
        {
            this.DepthFramesDropped++;
        }

        private void _sensor_IRFrameProduced(object sender, IRFrameEventArgs e)
        {
            this.IRFrameProduced?.Invoke(this, e);
            this.LastIRFrame = e;
            if (this.IRStream != null)
            {
                this.IRStream.Add(e);
            }
            this._irFPSMonitor.add_sample(e.RelativeTime.TotalSeconds);
        }

        private void _sensor_IRFrameDropped(object sender, EventArgs e)
        {
            this.IRFramesDropped++;
        }

        private void _sensor_ColorFrameProduced(object sender, ColorFrameEventArgs e)
        {
            this.ColorFrameProduced?.Invoke(this, e);
            this.LastColorFrame = e;
            if (this.ColorStream != null)
            {
                this.ColorStream.Add(e);
            }
            this._colorFPSMonitor.add_sample(e.RelativeTime.TotalSeconds);
        }

        private void _sensor_ColorFrameDropped(object sender, EventArgs e)
        {
            this.ColorFramesDropped++;
        }

        private void WatchFPS()
        {
            Task.Run(() =>
            {
                while (this._isInitialized)
                {
                    if (this._sensor.IsColorStreamEnabled)
                        this.ColorFPS = this._colorFPSMonitor.calc_fps();
                    if (this._sensor.IsDepthStreamEnabled)
                        this.DepthFPS = this._depthFPSMonitor.calc_fps();
                    if (this._sensor.IsIRStreamEnabled)
                        this.IRFPS = this._irFPSMonitor.calc_fps();

                    Thread.Sleep(500);
                }
            });
        }
    }
}

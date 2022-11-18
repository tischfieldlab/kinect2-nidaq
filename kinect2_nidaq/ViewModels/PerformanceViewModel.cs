using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace kinect2_nidaq.ViewModels
{
    public class PerformanceViewModel : ObservableObject
    {
        protected DispatcherTimer _checkTimer;
        protected PerformanceCounter _cpuPerformance;
        protected PerformanceCounter _ramPerformance;
        protected string _diskPathToMonitor;
        protected string _freeDiskSpace;

        protected SettingsViewModel _settings;

        public PerformanceViewModel(SettingsViewModel settings)
        {
            _cpuPerformance = new PerformanceCounter();
            _ramPerformance = new PerformanceCounter();

            _cpuPerformance.CategoryName = "Processor";
            _cpuPerformance.CounterName = "% Processor Time";
            _cpuPerformance.InstanceName = "_Total";

            _ramPerformance.CategoryName = "Memory";
            _ramPerformance.CounterName = "Available MBytes";

            this._progress = 0;

            this._settings = settings;
            this.StartMonitor();
        }

        public bool IsMonitoring { get => this._checkTimer != null && this._checkTimer.IsEnabled; }
        public void StartMonitor()
        {
            this._checkTimer = new DispatcherTimer();
            this._checkTimer.Interval = TimeSpan.FromMilliseconds(1000);
            this._checkTimer.Tick += this.CheckTimerTick;
            this._checkTimer.Start();
        }

        public void StopMonitor()
        {
            this._checkTimer.Stop();
            this._checkTimer.Tick -= this.CheckTimerTick;
            this._checkTimer = null;
        }

        protected void CheckTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(this._settings.FolderName))
                {
                    FileInfo PathInfo = new FileInfo(this._settings.FolderName);
                    DirectoryInfo directory = PathInfo.Directory;
                    if (PathInfo.Directory != null)
                    {
                        DriveInfo SaveDrive = new DriveInfo(PathInfo.Directory.Root.FullName);
                        Double FreeMem = SaveDrive.AvailableFreeSpace / 1e9;
                        Double AllMem = SaveDrive.TotalSize / 1e9;
                        this.FreeDiskSpace = String.Format("{0} {1:0.##} / {2:0.##} GB Free", SaveDrive.RootDirectory, FreeMem, AllMem);
                    }
                    else
                    {
                        this.FreeDiskSpace = "N/A";
                    }
                }
            }
            catch
            {
                this.FreeDiskSpace = "N/A";
            }
            this.NotifyPropertyChanged(null);
        }
        

        public string CPUPerformance { get => (100 - this._cpuPerformance.NextValue()).ToString("F1") + "% Free"; }
        public string RAMPerformance { get => this._ramPerformance.NextValue().ToString("F1") + "MB Free"; }
        public string FreeDiskSpace
        {
            get => this._freeDiskSpace;
            set => this.SetField(ref this._freeDiskSpace, value);
        }

        public string ApplicationStatus
        {
            get => this._applicationStatus;
            set => this.SetField(ref this._applicationStatus, value);
        }
        protected string _applicationStatus;


        public double? Progress
        {
            get => this._progress;
            set => this.SetField(ref this._progress, value);
        }
        protected double? _progress;

        public string ProgressETA
        {
            get => this._progressETA;
            set => this.SetField(ref this._progressETA, value);
        }
        protected string _progressETA;

        public double ColorFrameQueueUtilization
        {
            get => this._colorFrameQueueUtilization;
            set => this.SetField(ref this._colorFrameQueueUtilization, value);
        }
        protected double _colorFrameQueueUtilization;

        public bool IsColorAcquisitionActive
        {
            get => this._isColorAcquisitionActive;
            set => this.SetField(ref this._isColorAcquisitionActive, value);
        }
        protected bool _isColorAcquisitionActive;

        public double DepthFrameQueueUtilization
        {
            get => this._depthFrameQueueUtilization;
            set => this.SetField(ref this._depthFrameQueueUtilization, value);
        }
        protected double _depthFrameQueueUtilization;

        public bool IsDepthAcquisitionActive
        {
            get => this._isDepthAcquisitionActive;
            set => this.SetField(ref this._isDepthAcquisitionActive, value);
        }
        protected bool _isDepthAcquisitionActive;

        public double NidaqFrameQueueUtilization
        {
            get => this._nidaqFrameQueueUtilization;
            set => this.SetField(ref this._nidaqFrameQueueUtilization, value);
        }
        protected double _nidaqFrameQueueUtilization;

        public bool IsNidaqAcquisitionActive
        {
            get => this._isNidaqAcquisitionActive;
            set => this.SetField(ref this._isNidaqAcquisitionActive, value);
        }
        protected bool _isNidaqAcquisitionActive;


    }
}

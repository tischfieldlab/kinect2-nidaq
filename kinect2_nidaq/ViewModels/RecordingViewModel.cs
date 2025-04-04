using kinect2_nidaq.Models;
using kinect2_nidaq.Models.Recording;
using kinect2_nidaq.ViewModels.AnalogDAQ;
using Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels
{
    public class RecordingViewModel : ObservableObject
    {
        protected MainWindowViewModel viewModel;
        protected Recorder recorder;

        public RecordingViewModel(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        protected PerformanceViewModel Performance { get => this.viewModel.Performance; }
        protected SettingsViewModel Settings { get => this.viewModel.Settings; }
        protected KinectViewModel Kinect { get => this.viewModel.Kinect; }
        protected AnalogNIDAQViewModel AnalogDAQ { get => this.viewModel.AnalogDAQ; }

        public bool IsRecording
        {
            get => this._isRecording;
            protected set => this.SetField(ref this._isRecording, value);
        }
        protected bool _isRecording;

        public void StartRecording()
        {
            if (this.IsRecording)
            {
                throw new Exception("A recording session is already in progress!");
            }

            this.IsRecording = true;
            this.Settings.InactivateSettings();
            this.Kinect.Initialize();

            if (!this.Settings.IsPreviewMode)
            {
                var fileHelper = new FilePathHelper(this.Settings.FolderName);

                var recordingMode = this.Settings.IsIndeterminateRecording ? RecordingMode.Indeterminate : RecordingMode.Timed;
                this.recorder = new Recorder(recordingMode, TimeSpan.FromMinutes(this.Settings.RecordingDuration));
                this.recorder.PropertyChanged += this.Recorder_PropertyChanged;
                this.recorder.RecordingStarted += Recorder_RecordingStarted;
                this.recorder.BeforeRecordingEnd += Recorder_BeforeRecordingEnd;
                this.recorder.RecordingFinished += Recorder_RecordingFinished;

                this.recorder.AddDevice(this.Kinect);
                this.recorder.AddWriter(new MetadataWriter(fileHelper.Metadata, this.Settings, this.Kinect));

                if (this.Settings.IsColorStreamEnabled)
                {
                    this.Performance.IsColorAcquisitionActive = true;
                    this.recorder.AddWriter(new KinectColorWriter(this.Kinect.ColorInfo, fileHelper.ColorTS, fileHelper.ColorVid, this.Kinect.ColorStream));
                }

                if (this.Settings.IsDepthStreamEnabled)
                {
                    this.Performance.IsDepthAcquisitionActive = true;
                    this.recorder.AddWriter(new KinectDepthFFV1Writer(this.Kinect.DepthInfo, fileHelper.DepthTS, fileHelper.DepthVid, this.Kinect.DepthStream));
                }

                if (this.Settings.IsIRStreamEnabled)
                {
                    this.Performance.IsIRAcquisitionActive = true;
                    this.recorder.AddWriter(new KinectIRWriter(this.Kinect.IRInfo, fileHelper.IRTS, fileHelper.IRVid, this.Kinect.IRStream));
                }

                if (this.Settings.AnalogNIDAQ.IsEnabled)
                {
                    this.Performance.IsNidaqAcquisitionActive = true;
                    this.AnalogDAQ.Initialize();
                    this.recorder.AddDevice(this.AnalogDAQ);
                    this.recorder.AddWriter(new AnalogDaqWriter(fileHelper.Nidaq, this.AnalogDAQ.AnalogStream));
                }

                if (this.Settings.CompressSession)
                {
                    var compressor = new SessionCompressor(fileHelper);
                    compressor.Progress += Compressor_Progress;
                    this.recorder.AddPostRecordTask(compressor.CompressSession);
                }
                else
                {
                    var relocator = new FileRelocator(fileHelper);
                    this.recorder.AddPostRecordTask(relocator.RelocateFiles);
                }

                this.WatchQueueUtilization();

                this.recorder.Start();
            }
            else
            {
                this.Kinect.Start();
                this.Performance.ApplicationStatus = "Preview";
                this.Performance.ProgressETA = "ETA: Continuous";
                this.Performance.Progress = null;

                if (this.Settings.IsColorStreamEnabled)
                    this.Performance.IsColorAcquisitionActive = true;

                if (this.Settings.IsDepthStreamEnabled)
                    this.Performance.IsDepthAcquisitionActive = true;

                if (this.Settings.IsIRStreamEnabled)
                    this.Performance.IsIRAcquisitionActive = true;
            }
        }

        public void StopRecording()
        {
            if (this.recorder != null)
            {
                this.recorder.Stop();
            }
            else
            {
                this.Kinect.Stop();
                this.Recorder_RecordingFinished(null, null);
            }
            
        }

        private void Recorder_RecordingStarted(object sender, EventArgs e)
        {
            this.Performance.ApplicationStatus = "Recording";
        }

        private void Recorder_BeforeRecordingEnd(object sender, EventArgs e)
        {
            this.Performance.ApplicationStatus = "Finalizing";
            this.Performance.ProgressETA = "Finalizing";
            this.Performance.Progress = 1.0;
        }

        private void Recorder_RecordingFinished(object sender, EventArgs e)
        {
            this.Settings.ActivateSettings();
            this.Performance.ApplicationStatus = "Done";
            this.Performance.ProgressETA = "Complete";
            this.Performance.Progress = 1.0;
            this.Performance.IsColorAcquisitionActive = false;
            this.Performance.IsDepthAcquisitionActive = false;
            this.Performance.IsIRAcquisitionActive = false;
            this.Performance.IsNidaqAcquisitionActive = false;

            this.Performance.ColorFrameQueueUtilization = 0;
            this.Performance.DepthFrameQueueUtilization = 0;
            this.Performance.IRFrameQueueUtilization = 0;
            this.Performance.NidaqFrameQueueUtilization = 0;

            this.IsRecording = false;
        }

        private void Recorder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Performance.Progress = recorder.Progress;
            if (recorder.TimeRemaining.HasValue)
            {
                this.Performance.ProgressETA = String.Format("ETA: ({0} mins, {1:F0} secs)", recorder.TimeRemaining.Value.Minutes, recorder.TimeRemaining.Value.Seconds);
            }
            else
            {
                this.Performance.ProgressETA = "ETA: Continuous";
            }
        }

        private void Compressor_Progress(object sender, CompressionProgressEventArgs e)
        {
            this.Performance.ApplicationStatus = "Compressing";
            this.Performance.ProgressETA = String.Format("ETA: ({0} mins, {1:F0} secs)", Math.Floor(e.SmoothedETA / 60), e.SmoothedETA % 60);
            this.Performance.Progress = e.Progress;
        }

        private void WatchQueueUtilization()
        {
            Task.Run(() =>
            {
                while (this.IsRecording)
                {
                    if (this.Kinect.ColorStream != null)
                        this.Performance.ColorFrameQueueUtilization = ((double)this.Kinect.ColorStream.Count / (double)Constants.kMaxFrames);
                    if (this.Kinect.DepthStream != null)
                        this.Performance.DepthFrameQueueUtilization = ((double)this.Kinect.DepthStream.Count / (double)Constants.kMaxFrames);
                    if (this.Kinect.IRStream != null)
                        this.Performance.IRFrameQueueUtilization = ((double)this.Kinect.IRStream.Count / (double)Constants.kMaxFrames);
                    if (this.AnalogDAQ.AnalogStream != null)
                        this.Performance.NidaqFrameQueueUtilization = ((double)this.AnalogDAQ.AnalogStream.Count / (double)Constants.nMaxBuffer);
                    Thread.Sleep(100);
                }
            });
        }

    }
}

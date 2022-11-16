using kinect2_nidaq.Models;
using kinect2_nidaq.Models.Recording;
using kinect2_nidaq.ViewModels.AnalogDAQ;
using kinect2_nidaq.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kinect2_nidaq.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {

        public MainWindowViewModel()
        {
            this.Settings = new SettingsViewModel();
            this.Performance = new PerformanceViewModel(this.Settings);
            this.Kinect = new KinectViewModel(this.Settings);
            this.Recording = new RecordingViewModel();
            this.AnalogDAQ = new AnalogNIDAQViewModel(this.Settings);

            this.StartRecordingCommand = new StartRecordingCommand(this);
        }

        public SettingsViewModel Settings { get; protected set; }
        public PerformanceViewModel Performance { get; protected set; }
        public KinectViewModel Kinect { get; protected set; }
        public RecordingViewModel Recording { get; protected set; }
        public AnalogNIDAQViewModel AnalogDAQ { get; protected set; }

        public ICommand StartRecordingCommand { get; protected set; }

        public void StartRecording()
        {
            this.Settings.InactivateSettings();
            this.Kinect.Initialize();        

            if (!this.Settings.IsPreviewMode)
            {
                var fileHelper = new FilePathHelper(this.Settings.FolderName);

                var recordingMode = this.Settings.IsIndeterminateRecording ? RecordingMode.Indeterminate : RecordingMode.Timed;
                var recorder = new Recorder(recordingMode, TimeSpan.FromMinutes(this.Settings.RecordingDuration));

                recorder.AddDevice(this.Kinect);

                this.AnalogDAQ.Initialize();
                recorder.AddDevice(this.AnalogDAQ);

                recorder.AddWriter(new MetadataWriter(fileHelper.Metadata, this.Settings));

                if (this.Settings.IsColorStreamEnabled)
                {
                    recorder.AddWriter(new KinectColorWriter(fileHelper.ColorTS, fileHelper.ColorVid, this.Kinect.ColorStream));
                }

                if (this.Settings.IsDepthStreamEnabled)
                {
                    recorder.AddWriter(new KinectDepthWriter(fileHelper.DepthTS, fileHelper.DepthVid, this.Kinect.DepthStream));
                }

                if (this.Settings.AnalogNIDAQ.IsEnabled)
                {
                    recorder.AddWriter(new AnalogDaqWriter(fileHelper.Nidaq, this.AnalogDAQ.AnalogStream));
                }

                if(this.Settings.CompressSession)
                {
                    var compressor = new SessionCompressor(fileHelper);
                    compressor.Progress += Compressor_Progress;
                    recorder.AddPostRecordTask(compressor.CompressSession);
                }
                else
                {
                    recorder.AddPostRecordTask(() =>
                    {
                        foreach (string[] FileName in fileHelper.FileToTarMemberMapping)
                        {
                            Console.WriteLine(String.Format("{0} {1}", FileName[0], FileName[1]));

                            if (File.Exists(FileName[0]))
                            {
                                Console.WriteLine(String.Format("Moving {0} to {1}", FileName[0], Path.Combine(fileHelper.MoveFolder, FileName[1])));
                                File.Move(FileName[0], Path.Combine(fileHelper.MoveFolder, FileName[1]));
                            }
                        }
                    });
                }

                recorder.Start();
            }
        }

        private void Compressor_Progress(object sender, CompressionProgressEventArgs e)
        {
            this.Performance.ApplicationStatus = "Compressing";
            this.Performance.ProgressETA = String.Format("ETA: ({0} mins, {1:F2} secs)", Math.Floor(e.SmoothedETA / 60), e.SmoothedETA % 60);
            this.Performance.Progress = e.Progress;
        }

        public void StopRecording()
        {

        }
    }
}

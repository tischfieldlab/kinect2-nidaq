using kinect2_nidaq.Models;
using kinect2_nidaq.Models.Recording;
using kinect2_nidaq.ViewModels.AnalogDAQ;
using kinect2_nidaq.ViewModels.Commands;
using kinect2_nidaq.ViewModels.DigitalDAQ;
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
            this.Recording = new RecordingViewModel(this);
            this.AnalogDAQ = new AnalogNIDAQViewModel(this.Settings);
            this.DigitalDAQ = new DigitalNIDAQViewModel(this);

            this.StartRecordingCommand = new StartRecordingCommand(this);
            this.StopRecordingCommand = new StopRecordingCommand(this);
        }

        public SettingsViewModel Settings { get; protected set; }
        public PerformanceViewModel Performance { get; protected set; }
        public KinectViewModel Kinect { get; protected set; }
        public RecordingViewModel Recording { get; protected set; }
        public AnalogNIDAQViewModel AnalogDAQ { get; protected set; }
        public DigitalNIDAQViewModel DigitalDAQ { get; protected set; }

        public ICommand StartRecordingCommand { get; protected set; }
        public ICommand StopRecordingCommand { get; protected set; }






    }
}

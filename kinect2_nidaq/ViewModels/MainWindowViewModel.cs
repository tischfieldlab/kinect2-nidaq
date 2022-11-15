using kinect2_nidaq.ViewModels.AnalogDAQ;
using kinect2_nidaq.ViewModels.KinectV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {

        public MainWindowViewModel()
        {
            this.Performance = new PerformanceViewModel();
            this.Kinect = new KinectViewModel();
            this.Recording = new RecordingViewModel();
            this.AnalogDAQ = new AnalogNIDAQViewModel();
        }


        public PerformanceViewModel Performance { get; protected set; }
        public KinectViewModel Kinect { get; protected set; }
        public RecordingViewModel Recording { get; protected set; }
        public AnalogNIDAQViewModel AnalogDAQ { get; protected set; }



        public void StartRecording()
        {
            
            this.Kinect.Start();

            //if (this.Kinect.IsColorStreamEnabled)
            var colorWriter = new KinectV2ColorWriter("", this.Kinect.ColorStream);
            colorWriter.Start();

            //if (this.Kinect.IsDepthStreamEnabled)
            var depthWriter = new KinectV2DepthWriter("", this.Kinect.DepthStream);
            depthWriter.Start();

            //if (this.AnalogDAQ.Settings.IsEnabled)
            var analogDaqWriter = new AnalogDaqWriter("", this.AnalogDAQ.AnalogStream);
            analogDaqWriter.Start();

        }


    }
}

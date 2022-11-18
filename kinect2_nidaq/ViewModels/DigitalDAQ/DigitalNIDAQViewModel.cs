using kinect2_nidaq.Properties;
using NationalInstruments;
using NationalInstruments.DAQmx;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels.DigitalDAQ
{
    public class DigitalNIDAQViewModel: ObservableObject
    {
        protected MainWindowViewModel viewModel;
        protected DigitalTtlListner ttlListner;
        protected DigitalTtlSender ttlSender;


        public DigitalNIDAQViewModel(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.viewModel.Settings.DigitalNIDAQ = new DigitalNIDAQSettingsViewModel();
            this.viewModel.Settings.DigitalNIDAQ.PropertyChanged += DigitalNIDAQ_PropertyChanged;

            this.ttlSender = new DigitalTtlSender();
            this.viewModel.Kinect.DepthFrameProduced += Kinect_DepthFrameProduced;


        }

        protected DigitalNIDAQSettingsViewModel DIOSettings { get => this.viewModel.Settings.DigitalNIDAQ; }

        private void DigitalNIDAQ_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            List<string> listnerProps = new List<string>() { "IsRecieveStartRecordingTTLEnabled", "RecieveStartRecordingTTLDevice" };
            if (listnerProps.Contains(e.PropertyName))
            {
                this.DisposeListner();
                this.RunListner();
            }
        }


        protected void RunListner()
        {
            if (this.DIOSettings.IsRecieveStartRecordingTTLEnabled)
            {
                this.ttlListner = new DigitalTtlListner(this.DIOSettings.RecieveStartRecordingTTLDevice);
                this.ttlListner.TTLRecieved += TtlListner_TTLRecieved;
                this.ttlListner.Start();
            }
        }

        protected void DisposeListner()
        {
            if (this.ttlListner != null)
            {
                this.ttlListner.Stop();
                this.ttlListner.TTLRecieved -= TtlListner_TTLRecieved;
                this.ttlListner = null;
            }
        }

        private void TtlListner_TTLRecieved(object sender, EventArgs e)
        {
            this.viewModel.Recording.StartRecording();
        }

        private void Kinect_DepthFrameProduced(object sender, DepthFrameEventArgs e)
        {
            if (this.DIOSettings.IsSendDepthCaptureTTLEnabled)
            {
                this.ttlSender.SendTTL(this.DIOSettings.SendDepthCaptureTTLDevice);
            }
        }
    }
}

using kinect2_nidaq.Properties;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kinect2_nidaq.ViewModels.DigitalDAQ
{
    public class DigitalNIDAQSettingsViewModel : ObservableObject
    {
        public DigitalNIDAQSettingsViewModel()
        {
            this.AvailableDevices = new ObservableCollection<string>();

            this.PopulateDevices();
        }

        public ObservableCollection<string> AvailableDevices { get; protected set; }
        

        public bool IsRecieveStartRecordingTTLEnabled
        {
            get => this._isRecieveStartRecordingTTLEnabled;
            set => this.SetField(ref this._isRecieveStartRecordingTTLEnabled, value, this.PopulateDevices);
        }
        protected bool _isRecieveStartRecordingTTLEnabled;

        public string RecieveStartRecordingTTLDevice
        {
            get => this._recieveStartRecordingTTLDevice;
            set => this.SetField(ref this._recieveStartRecordingTTLDevice, value);
        }
        protected string _recieveStartRecordingTTLDevice;


        public bool IsSendDepthCaptureTTLEnabled
        {
            get => this._isSendDepthCaptureTTLEnabled;
            set => this.SetField(ref this._isSendDepthCaptureTTLEnabled, value, this.PopulateDevices);
        }
        protected bool _isSendDepthCaptureTTLEnabled;

        public string SendDepthCaptureTTLDevice
        {
            get => this._sendDepthCaptureTTLDevice;
            set => this.SetField(ref this._sendDepthCaptureTTLDevice, value);
        }
        protected string _sendDepthCaptureTTLDevice;



        protected void PopulateDevices()
        {
            var channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DILine, PhysicalChannelAccess.External);
            var toRemove = new List<string>();
            foreach (var c in this.AvailableDevices)
            {
                if (!channels.Contains(c))
                {
                    toRemove.Add(c);
                }
            }
            toRemove.ForEach((tr) => this.AvailableDevices.Remove(tr));
            foreach (var channel in channels)
            {
                if (!this.AvailableDevices.Contains(channel))
                {
                    this.AvailableDevices.Add(channel);
                }
            }
        }
    }
}

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
            this.IsEnabled = this.AvailableDevices.Count > 0;
        }

        public ObservableCollection<string> AvailableDevices { get; protected set; }

        public bool IsEnabled
        {
            get => this._isEnabled;
            set
            {
                if (value)
                {
                    // need to ensure a device is available
                    this.PopulateDevices(); // re-query just in case user recently connected a device
                    if (this.AvailableDevices.Count <= 0)
                    {
                        MessageBox.Show("A suitable NI device was not detected.\nPlease connect a device and try again.", 
                                        "Device not found!",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation);
                        this.SetField(ref this._isEnabled, false);
                        return;
                    }
                }
                this.SetField(ref this._isEnabled, value);
            }
        }
        protected bool _isEnabled;


        public string Device
        {
            get => this._device;
            set
            {
                this.SetField(ref this._device, value);
                this.OnDeviceChange();
            }
        }
        protected string _device;
        

        public bool IsRecieveStartRecordingTTLEnabled
        {
            get => this._isRecieveStartRecordingTTLEnabled;
            set => this.SetField(ref this._isRecieveStartRecordingTTLEnabled, value);
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
            set => this.SetField(ref this._isSendDepthCaptureTTLEnabled, value);
        }
        protected bool _isSendDepthCaptureTTLEnabled;

        public string SendDepthCaptureTTLDevice
        {
            get => this._sendDepthCaptureTTLDevice;
            set => this.SetField(ref this._sendDepthCaptureTTLDevice, value);
        }
        protected string _sendDepthCaptureTTLDevice;



        protected void OnDeviceChange()
        {
            var device = DaqSystem.Local.LoadDevice(this._device);

            
        }

        protected void PopulateDevices()
        {
            this.AvailableDevices.Clear();
            var channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DILine, PhysicalChannelAccess.External);
            foreach (var channel in channels)
            {
                this.AvailableDevices.Add(channel);
            }
        }
    }
}

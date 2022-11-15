using kinect2_nidaq.Properties;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels.AnalogDAQ
{
    public class AnalogNIDAQSettingsViewModel : ObservableObject
    {
        public AnalogNIDAQSettingsViewModel()
        {
            this.AvailableDevices = new ObservableCollection<string>();
            this.AvailableTerminals = new ObservableCollection<TerminalConfigurationItem>();
            this.AvailableVoltageRanges = new ObservableCollection<VoltageRangeItem>();

            this.SelectedChannels = new ObservableCollection<string>();
            this.AvailableChannels = new ObservableCollection<string>();

            this.SamplingRate = Settings.Default.SamplingRate;
        }

        public bool IsEnabled
        {
            get => this._isEnabled;
            set => this.SetField(ref this._isEnabled, value);
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
        public ObservableCollection<string> AvailableDevices { get; protected set; }



        public AITerminalConfiguration TerminalConfiguration
        {
            get => this._terminalConfiguration;
            set => this.SetField(ref this._terminalConfiguration, value);
        }
        protected AITerminalConfiguration _terminalConfiguration;
        public ObservableCollection<TerminalConfigurationItem> AvailableTerminals { get; protected set; }



        public Tuple<double, double> VoltageRange
        {
            get => this._voltageRange;
            set => this.SetField(ref this._voltageRange, value);
        }
        protected Tuple<double, double> _voltageRange;
        public ObservableCollection<VoltageRangeItem> AvailableVoltageRanges { get; protected set; }


        public ObservableCollection<string> SelectedChannels { get; protected set; }
        public string SelectedChannelsString
        {
            get => this.SelectedChannels.Aggregate("", (accum, item) => String.Format("{0} {1},", accum, item)).Trim(new Char[] { ' ', ',' });
        }
        public ObservableCollection<string> AvailableChannels { get; protected set; }


        public double SamplingRate
        {
            get => this._samplingRate;
            set => this.SetField(ref this._samplingRate, value);
        }
        protected double _samplingRate;


        public double MaxRate
        {
            get => this._maxRate;
            protected set => this.SetField(ref this._maxRate, value);
        }
        protected double _maxRate;


        protected void OnDeviceChange()
        {
            var device = DaqSystem.Local.LoadDevice(this._device);

            // populate channels
            this.SelectedChannels.Clear();
            this.AvailableChannels.Clear();
            foreach (var Channel in device.AIPhysicalChannels)
            {
                this.AvailableChannels.Add(Channel);
            }


            // populate terminals 
            this.AvailableTerminals.Clear();
            var aitci = Enum.GetValues(typeof(AITerminalConfiguration))
                            .Cast<AITerminalConfiguration>()
                            .Select((aitc) => new TerminalConfigurationItem(aitc));
            foreach (var at in aitci)
            {
                this.AvailableTerminals.Add(at);
            }


            // populate voltage ranges
            double[] RangeList = device.AIVoltageRanges;
            this.AvailableVoltageRanges = new ObservableCollection<VoltageRangeItem>();
            for (int i = 0; i < RangeList.Length; i = i + 2)
            {
                this.AvailableVoltageRanges.Add(new VoltageRangeItem(RangeList[i], RangeList[i + 1]));
            }


            // Set max supported sample rate
            this.MaxRate = device.AIMaximumMultiChannelRate;
        }

        protected void PopulateDevices()
        {
            this.AvailableDevices.Clear();
            foreach (var device in DaqSystem.Local.Devices)
            {
                // TODO: Probably we should filter to only devices that support this type of task!
                this.AvailableDevices.Add(device);
            }
        }
    }

    public class TerminalConfigurationItem : ObservableObject
    {
        public TerminalConfigurationItem(AITerminalConfiguration aITerminalConfiguration)
        {
            this.TerminalConfiguration = aITerminalConfiguration;
            this.DisplayName = aITerminalConfiguration.ToString();
        }
        public AITerminalConfiguration TerminalConfiguration { get; protected set; }
        public string DisplayName { get; protected set; }
    }

    public class VoltageRangeItem : ObservableObject
    {
        public VoltageRangeItem(double low, double high)
        {
            this.Low = low;
            this.High = high;
        }
        public double Low { get; protected set; }
        public double High { get; protected set; }
        public Tuple<double, double> Value { get => new Tuple<double, double>(this.Low, this.High); }
        public string DisplayName { get => String.Format("{0} {1}", this.Low, this.High); }
    }
}

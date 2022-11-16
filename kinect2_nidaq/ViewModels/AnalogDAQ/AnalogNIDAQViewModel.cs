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

namespace kinect2_nidaq.ViewModels.AnalogDAQ
{
    public class AnalogNIDAQViewModel: ObservableObject, IDeviceViewModel
    {
        private bool _isInitialized;
        protected SettingsViewModel settings;
        protected NationalInstruments.DAQmx.Task _analogInTask;
        protected BlockingCollection<AnalogWaveform<double>[]> _dataQueue;
        protected AnalogMultiChannelReader _channelReader;


        public AnalogNIDAQViewModel(SettingsViewModel settings)
        {
            this.settings = settings;
            this.settings.AnalogNIDAQ = new AnalogNIDAQSettingsViewModel();
        }
        public BlockingCollection<AnalogWaveform<double>[]> AnalogStream { get => this._dataQueue; }


        public void Initialize()
        {
            this._dataQueue = new BlockingCollection<AnalogWaveform<double>[]>(Constants.nMaxBuffer);
            this._isInitialized = true;
        }

        public void Start()
        {
            if (!this._isInitialized)
                throw new ApplicationException("You must call Initialize() before calling Start()!");

            if (this._analogInTask == null 
             && this.settings.AnalogNIDAQ.SelectedChannels.Count > 0
             && (this.settings.AnalogNIDAQ.SamplingRate > 0
             && this.settings.AnalogNIDAQ.SamplingRate < this.settings.AnalogNIDAQ.MaxRate))
            {
                this._analogInTask = new NationalInstruments.DAQmx.Task();

                this._analogInTask.AIChannels.CreateVoltageChannel(
                    this.settings.AnalogNIDAQ.SelectedChannelsString,
                    "",
                    this.settings.AnalogNIDAQ.TerminalConfiguration,
                    this.settings.AnalogNIDAQ.VoltageRange.Item1,
                    this.settings.AnalogNIDAQ.VoltageRange.Item2,
                    AIVoltageUnits.Volts);

                this._analogInTask.Timing.ConfigureSampleClock("", this.settings.AnalogNIDAQ.SamplingRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1);
                this._analogInTask.Control(TaskAction.Verify);

                this._channelReader = new AnalogMultiChannelReader(this._analogInTask.Stream);
                this._channelReader.SynchronizeCallbacks = true;
                this._channelReader.BeginReadWaveform(1, new AsyncCallback(this.AnalogIn_Callback), this._analogInTask);
            }
        }

        public void Stop()
        {
            this._analogInTask.Stop();
            this._analogInTask.Dispose();
            this._analogInTask = null;
            this._dataQueue.CompleteAdding();
        }

        private void AnalogIn_Callback(IAsyncResult ar)
        {
            if (null != this._analogInTask && this._analogInTask == ar.AsyncState)
            {
                // read in with waveform data type to get timestamp          
                var waveforms = this._channelReader.EndReadWaveform(ar);

                // Current NIDAQ timestamp
                var tmpTimeStamp = waveforms[0].GetPrecisionTimeStamps();
                var currentNITimeStamp = (double)tmpTimeStamp[0].WholeSeconds + tmpTimeStamp[0].FractionalSeconds;

                // Guaranteed to be 1
                this._dataQueue.Add(waveforms);
                this._channelReader.BeginReadWaveform(1, AnalogIn_Callback, this._analogInTask);
            }
        }
    }
}

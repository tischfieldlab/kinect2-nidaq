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
    public class AnalogNIDAQViewModel
    {
        protected NationalInstruments.DAQmx.Task _analogInTask;
        protected BlockingCollection<AnalogWaveform<double>[]> _dataQueue;
        protected AnalogMultiChannelReader _channelReader;


        public AnalogNIDAQViewModel()
        {
            this.Settings = new AnalogNIDAQSettingsViewModel();
        }
        public AnalogNIDAQSettingsViewModel Settings { get; protected set; }
        public BlockingCollection<AnalogWaveform<double>[]> AnalogStream { get => this._dataQueue; }


        public void Start()
        {
            if (this._analogInTask == null 
             && this.Settings.SelectedChannels.Count > 0
             && (this.Settings.SamplingRate > 0
             && this.Settings.SamplingRate < this.Settings.MaxRate))
            {

                this._analogInTask = new NationalInstruments.DAQmx.Task();
                this._dataQueue = new BlockingCollection<AnalogWaveform<double>[]>(Constants.nMaxBuffer);

                this._analogInTask.AIChannels.CreateVoltageChannel(
                    this.Settings.SelectedChannelsString,
                    "",
                    this.Settings.TerminalConfiguration,
                    this.Settings.VoltageRange.Item1,
                    this.Settings.VoltageRange.Item2,
                    AIVoltageUnits.Volts);

                this._analogInTask.Timing.ConfigureSampleClock("", this.Settings.SamplingRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1);
                this._analogInTask.Control(TaskAction.Verify);

                this._channelReader = new AnalogMultiChannelReader(this._analogInTask.Stream);
                this._channelReader.SynchronizeCallbacks = true;
                this._channelReader.BeginReadWaveform(1, new AsyncCallback(this.AnalogIn_Callback), this._analogInTask);

                //NidaqPrepare.IsEnabled = false;
                //Indicate that we're pulling data from the Nidaq
                // IsNidaqEnabled = true; // TODO: not sure if we need??

            }
        }

        public void Stop()
        {
            //runningTask = null;
            this._analogInTask.Stop();
            this._analogInTask.Dispose();
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

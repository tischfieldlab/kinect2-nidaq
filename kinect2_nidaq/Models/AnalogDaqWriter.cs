using NationalInstruments;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class AnalogDaqWriter : IDataWriter
    {
        private string _destPath;

        private BlockingCollection<AnalogWaveform<double>[]> _queue;
        private Task _writingTask;

        public AnalogDaqWriter(string destPath, BlockingCollection<AnalogWaveform<double>[]> queue)
        {
            this._destPath = destPath;
            this._queue = queue;
        }

        public void Start()
        {
            this._writingTask = Task.Factory.StartNew(this.NidaqRunner);
        }
        public void Stop()
        {
            if (this._writingTask != null)
            {
                try
                {
                    this._writingTask.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle((x) =>
                    {
                        Console.WriteLine("Exception finishing task:  {0} {1}\n{2}", x.GetType().ToString(), x.Message, x.StackTrace);
                        return false;
                    });
                }
                this._writingTask.Dispose();
            }
        }

        private void NidaqRunner()
        {
            using (var fileStream = new FileStream(this._destPath, FileMode.Append))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    AnalogWaveform<double>[] NIDatum = null;
                    while (this._queue.TryTake(out NIDatum, 100))
                    {
                        int nsamples = NIDatum[0].SampleCount;
                        int nchannels = NIDatum.Length;

                        double[][] data = new double[nchannels][];

                        // write out nidaq data, etc. etc.
                        for (int i = 0; i < nchannels; i++)
                        {
                            data[i] = NIDatum[i].GetScaledData();
                        }

                        NationalInstruments.PrecisionDateTime[] timestamps = NIDatum[0].GetPrecisionTimeStamps();

                        for (int i = 0; i < nsamples; i++)
                        {
                            for (int ii = 0; ii < nchannels; ii++)
                            {
                                binaryWriter.Write(data[ii][i]);
                            }
                            binaryWriter.Write((double)timestamps[i].WholeSeconds + timestamps[i].FractionalSeconds);
                        }
                    }
                }
            }
            this._queue.Dispose();
        }
    }
}

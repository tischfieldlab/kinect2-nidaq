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
        private FileStream _fileStream;
        private BinaryWriter _binaryWriter;
        private BlockingCollection<AnalogWaveform<double>[]> _queue;
        private Task _writingTask;

        public AnalogDaqWriter(string destPath, BlockingCollection<AnalogWaveform<double>[]> queue)
        {
            this._fileStream = new FileStream(destPath, FileMode.Append);
            this._binaryWriter = new BinaryWriter(this._fileStream);

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
                        double[] tmp = NIDatum[i].GetScaledData();

                        // do something with each datapoint and timestamp
                        data[i] = new double[nsamples];
                        data[i] = tmp;
                    }

                    NationalInstruments.PrecisionDateTime[] timestamps = NIDatum[0].GetPrecisionTimeStamps();

                    // now we can write out...             
                    // check for multiple samples?

                    for (int i = 0; i < nsamples; i++)
                    {
                        //string writestring = "";
                        for (int ii = 0; ii < nchannels; ii++)
                        {
                            this._binaryWriter.Write(data[ii][i]);
                            //writestring = String.Format("{0} {1}", writestring, data[ii][i]);
                        }
                        /*NidaqStream.WriteLine(String.Format("{0} {1}",
                            writestring,
                            (double)timestamps[i].WholeSeconds + timestamps[i].FractionalSeconds));*/
                        this._binaryWriter.Write((double)timestamps[i].WholeSeconds + timestamps[i].FractionalSeconds);
                    }
                }
            }
        }
    }
}

using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NationalInstruments;


namespace kinect2_nidaq.ViewModels.DigitalDAQ
{
    public class DigitalTtlListner
    {
        protected string device;
        protected System.Threading.Tasks.Task backgroundListenTask;
        protected CancellationTokenSource backgroundListenTaskCancellationTokenSource;

        public event EventHandler TTLRecieved;


        public DigitalTtlListner(string deviceLine)
        {
            this.device = deviceLine;
        }

        protected void Run(CancellationToken cancellationToken)
        {
            try
            {
                using (var readTask = new NationalInstruments.DAQmx.Task())
                {
                    readTask.DIChannels.CreateChannel(this.device, "", ChannelLineGrouping.OneChannelForEachLine);

                    // Create the reader
                    var digitalReader = new DigitalSingleChannelReader(readTask.Stream);

                    // Start the task
                    readTask.Start();

                    bool lastState = digitalReader.ReadSingleSampleSingleLine();
                    bool initState = lastState;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        bool currState = digitalReader.ReadSingleSampleSingleLine();
                        if (currState != initState && currState != lastState)
                        {
                            this.TTLRecieved?.Invoke(this, new EventArgs());
                        }
                        lastState = currState;

                    }
                    readTask.Stop();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public void Start()
        {
            this.backgroundListenTaskCancellationTokenSource = new CancellationTokenSource();
            CancellationToken ct = backgroundListenTaskCancellationTokenSource.Token;
            this.backgroundListenTask = System.Threading.Tasks.Task.Run(() => this.Run(ct), ct);
            this.backgroundListenTask.ContinueWith((antecedant) => this.Cleanup());
        }

        public void Stop()
        {
            if (this.backgroundListenTaskCancellationTokenSource != null)
            {
                this.backgroundListenTaskCancellationTokenSource.Cancel();
            }
        }

        protected void Cleanup()
        {
            if (this.backgroundListenTaskCancellationTokenSource != null)
            {
                this.backgroundListenTaskCancellationTokenSource.Cancel();
                this.backgroundListenTaskCancellationTokenSource.Dispose();
                this.backgroundListenTaskCancellationTokenSource = null;
            }
            this.backgroundListenTask.Wait();
            this.backgroundListenTask.Dispose();
            this.backgroundListenTask = null;
        }
    }
}

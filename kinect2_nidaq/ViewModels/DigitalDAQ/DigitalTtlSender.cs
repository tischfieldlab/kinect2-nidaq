using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels.DigitalDAQ
{
    public class DigitalTtlSender
    {
        public DigitalTtlSender()
        {
        }

        public void SendTTL(string deviceLine)
        {
            using (var digitalWriteTask = new NationalInstruments.DAQmx.Task())
            {
                digitalWriteTask.DOChannels.CreateChannel(deviceLine, "", ChannelLineGrouping.OneChannelForEachLine);
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                writer.WriteSingleSampleSingleLine(true, true);
                var timer = Stopwatch.StartNew();
                while (timer.IsRunning)
                {
                    if (timer.ElapsedMilliseconds >= 10)
                    {
                        timer.Stop();
                    }
                }
                writer.WriteSingleSampleSingleLine(true, false);
            }
        }
    }
}

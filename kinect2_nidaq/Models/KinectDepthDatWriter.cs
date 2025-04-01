using Accord.Video.FFMPEG;
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
    public class KinectDepthDatWriter : IDataWriter
    {
        private BlockingCollection<DepthFrameEventArgs> _queue;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;


        public KinectDepthDatWriter(string tsDestPath, string videoDestPath, BlockingCollection<DepthFrameEventArgs> queue)
        {
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;

            this._queue = queue;
        }

        public void Start()
        {
            this._writingTask = Task.Factory.StartNew(this.DepthRunner);
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

        private void DepthRunner()
        {
            using (var depthTsFileStream = new FileStream(this._tsDestPath, FileMode.Append))
            using (var depthTsStreamWriter = new StreamWriter(depthTsFileStream))

            using (var depthVideoFileStream = new FileStream(this._videoDestPath, FileMode.Append))
            using (var depthVideoStreamWriter = new BinaryWriter(depthVideoFileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    DepthFrameEventArgs depthData = null;
                    while (this._queue.TryTake(out depthData, 100))
                    {
                        if (/*IsDepthStreamEnabled && IsRecordingEnabled*/true) // TODO: I think we do not need this anymore?
                        {
                            depthTsStreamWriter.WriteLine(String.Format("{0} {1}", depthData.RelativeTime.TotalMilliseconds, depthData.TimeStamp));
                            foreach (ushort depthDatum in depthData.DepthData)
                            {
                                depthVideoStreamWriter.Write(depthDatum);
                            }
                        }
                    }
                }
            }
            this._queue.Dispose();
        }
    }
}

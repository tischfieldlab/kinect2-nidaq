using Accord.Video.FFMPEG;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels.KinectV2
{
    public class KinectV2DepthWriter
    {
        private FileStream _depthTsFileStream;
        private StreamWriter _depthTsStreamWriter;

        private FileStream _depthVideoFileStream;
        private BinaryWriter _depthVideoStreamWriter;

        private BlockingCollection<DepthFrameEventArgs> _queue;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;


        public KinectV2DepthWriter(string destPath, BlockingCollection<DepthFrameEventArgs> queue)
        {
            this._tsDestPath = destPath;
            this._videoDestPath = destPath;

            this._depthTsFileStream = new FileStream(this._tsDestPath, FileMode.Append);
            this._depthTsStreamWriter = new StreamWriter(this._depthTsFileStream);

            this._depthVideoFileStream = new FileStream(this._videoDestPath, FileMode.Append);
            this._depthVideoStreamWriter = new BinaryWriter(this._depthVideoFileStream);

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
            while (!this._queue.IsCompleted)
            {
                DepthFrameEventArgs depthData = null;
                while (this._queue.TryTake(out depthData, 100))
                {

                    if (/*IsDepthStreamEnabled && IsRecordingEnabled*/true) // TODO: I think we do not need this anymore?
                    {
                        this._depthTsStreamWriter.WriteLine(String.Format("{0} {1}", depthData.RelativeTime.TotalMilliseconds, depthData.TimeStamp));
                        foreach (ushort depthDatum in depthData.DepthData)
                        {
                            this._depthVideoStreamWriter.Write(depthDatum);
                        }
                    }
                }
            }
        }
    }
}

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
    public class KinectDepthFFV1Writer : IDataWriter
    {
        private BlockingCollection<DepthFrameEventArgs> _queue;
        private VideoFileWriter _videoWriter;
        private TimeSpan _initialTimeSpan;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;

        private DepthInfo _info;


        public KinectDepthFFV1Writer(DepthInfo info, string tsDestPath, string videoDestPath, BlockingCollection<DepthFrameEventArgs> queue)
        {
            this._info = info;
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;

            this._videoWriter = new VideoFileWriter();

            this._queue = queue;
        }

        public void Start()
        {
            this._writingTask = Task.Factory.StartNew(this.IRRunner);
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

        private void IRRunner()
        {
            using (var tsFileStream = new FileStream(this._tsDestPath, FileMode.Append))
            using (var tsStreamWriter = new StreamWriter(tsFileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    DepthFrameEventArgs data = null;
                    while (this._queue.TryTake(out data, 100))
                    {
                        if (true /*IsIRStreamEnabled && IsRecordingEnabled*/) // TODO: Not sure we need this check???
                        {
                            if (!this._videoWriter.IsOpen)
                            {
                                this._videoWriter.Open(this._videoDestPath,
                                    this._info.Width,
                                    this._info.Height,
                                    new Accord.Math.Rational(this._info.FPS),
                                    VideoCodec.FFV1,
                                    Properties.Settings.Default.BitRate);

                                this._initialTimeSpan = data.RelativeTime;
                            }

                            tsStreamWriter.WriteLine(String.Format("{0} {1}", data.RelativeTime.TotalMilliseconds, data.TimeStamp));

                            // Writing with timestamps makes FFMPEG choke!!
                            //this._irVideoWriter.WriteVideoFrame(irData.ToBitmap().ToSystemBitmap(), irData.RelativeTime - this._initialTimeSpan);

                            // Do it like this instead
                            this._videoWriter.WriteVideoFrame(data.ToBitmap().ToSystemBitmap());
                        }
                    }
                }
            }
            this._videoWriter.Close();
            this._videoWriter.Dispose();
            this._queue.Dispose();
        }
    }
}

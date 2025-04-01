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
    public class KinectIRWriter : IDataWriter
    {
        private BlockingCollection<IRFrameEventArgs> _queue;
        private VideoFileWriter _irVideoWriter;
        private TimeSpan _initialTimeSpan;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;

        private IRInfo _info;


        public KinectIRWriter(IRInfo info, string tsDestPath, string videoDestPath, BlockingCollection<IRFrameEventArgs> queue)
        {
            this._info = info;
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;

            this._irVideoWriter = new VideoFileWriter();

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
            using (var irTsFileStream = new FileStream(this._tsDestPath, FileMode.Append))
            using (var irTsStreamWriter = new StreamWriter(irTsFileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    IRFrameEventArgs irData = null;
                    while (this._queue.TryTake(out irData, 100))
                    {
                        if (true /*IsIRStreamEnabled && IsRecordingEnabled*/) // TODO: Not sure we need this check???
                        {
                            if (!this._irVideoWriter.IsOpen)
                            {
                                this._irVideoWriter.Open(this._videoDestPath,
                                    this._info.Width,
                                    this._info.Height,
                                    new Accord.Math.Rational(this._info.FPS),
                                    VideoCodec.FFV1,
                                    Properties.Settings.Default.BitRate);

                                this._initialTimeSpan = irData.RelativeTime;
                            }

                            irTsStreamWriter.WriteLine(String.Format("{0} {1}", irData.RelativeTime.TotalMilliseconds, irData.TimeStamp));

                            // Writing with timestamps makes FFMPEG choke!!
                            //this._irVideoWriter.WriteVideoFrame(irData.ToBitmap().ToSystemBitmap(), irData.RelativeTime - this._initialTimeSpan);

                            // Do it like this instead
                            this._irVideoWriter.WriteVideoFrame(irData.ToBitmap().ToSystemBitmap());
                        }
                    }
                }
            }
            this._irVideoWriter.Close();
            this._irVideoWriter.Dispose();
            this._queue.Dispose();
        }
    }
}

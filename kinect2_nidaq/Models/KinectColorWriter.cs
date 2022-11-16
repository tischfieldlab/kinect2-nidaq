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
    public class KinectColorWriter : IDataWriter
    {
        private FileStream _colorTsFileStream;
        private StreamWriter _colorTsStreamWriter;
        private BlockingCollection<ColorFrameEventArgs> _queue;
        private VideoFileWriter _colorVideoWriter;
        private TimeSpan _initialTimeSpan;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;


        public KinectColorWriter(string tsDestPath, string videoDestPath, BlockingCollection<ColorFrameEventArgs> queue)
        {
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;

            this._colorTsFileStream = new FileStream(this._tsDestPath, FileMode.Append);
            this._colorTsStreamWriter = new StreamWriter(this._colorTsFileStream);

            this._colorVideoWriter = new VideoFileWriter();

            this._queue = queue;
        }

        public void Start()
        {
            this._writingTask = Task.Factory.StartNew(this.ColorRunner);
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

        private void ColorRunner()
        {
            while (!this._queue.IsCompleted)
            {
                ColorFrameEventArgs colorData = null;
                while (this._queue.TryTake(out colorData, 100))
                {
                    if (true /*IsColorStreamEnabled && IsRecordingEnabled*/) // TODO: Not sure we need this check???
                    {
                        if (!this._colorVideoWriter.IsOpen)
                        {
                            this._colorVideoWriter.Open(this._videoDestPath, 
                                Constants.kDefaultFrameWidth,
                                Constants.kDefaultFrameHeight,
                                Constants.kFramesPerSecond,
                                VideoCodec.Default,
                                Properties.Settings.Default.BitRate);

                            this._initialTimeSpan = colorData.RelativeTime;
                        }

                        this._colorTsStreamWriter.WriteLine(String.Format("{0} {1}", colorData.RelativeTime.TotalMilliseconds, colorData.TimeStamp));
                        this._colorVideoWriter.WriteVideoFrame(colorData.ToBitmap().ToSystemBitmap(), colorData.RelativeTime - this._initialTimeSpan);
                    }
                }
            }
        }
    }
}

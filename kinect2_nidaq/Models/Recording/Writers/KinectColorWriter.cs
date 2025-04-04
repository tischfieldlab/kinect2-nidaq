using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Sensor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class KinectColorWriter : IDataWriter
    {
        private BlockingCollection<ColorFrameEventArgs> _queue;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;

        private ColorInfo _info;


        public KinectColorWriter(ColorInfo info, string tsDestPath, string videoDestPath, BlockingCollection<ColorFrameEventArgs> queue)
        {
            this._info = info;
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;
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

        IEnumerable<IVideoFrame> GetFrames()
        {
            using (var colorTsFileStream = new FileStream(this._tsDestPath, FileMode.Append))
            using (var colorTsStreamWriter = new StreamWriter(colorTsFileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    ColorFrameEventArgs colorData = null;
                    while (this._queue.TryTake(out colorData, 100))
                    {
                        colorTsStreamWriter.WriteLine(String.Format("{0} {1}", colorData.RelativeTime.TotalMilliseconds, colorData.TimeStamp));
                        yield return new BitmapVideoFrameWrapper(colorData.ToBitmap());
                    }
                }
            }
            this._queue.Dispose();
        }

        private void ColorRunner()
        {
            var writer = FFMpegArguments
                .FromPipeInput(new RawVideoPipeSource(this.GetFrames()), options => options
                    .WithFramerate(this._info.FPS))
                .OutputToFile(this._videoDestPath, true, options => options
                    .WithVideoCodec("h264_qsv")
                    .WithFramerate(this._info.FPS)
                    //.WithSpeedPreset(Speed.UltraFast)

                    )
                .ProcessSynchronously();
        }
    }
}

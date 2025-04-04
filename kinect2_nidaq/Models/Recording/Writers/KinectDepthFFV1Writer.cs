using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
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
        private readonly BlockingCollection<DepthFrameEventArgs> _queue;

        private readonly string _tsDestPath;
        private readonly string _videoDestPath;

        private Task _writingTask;

        private readonly DepthInfo _info;


        public KinectDepthFFV1Writer(DepthInfo info, string tsDestPath, string videoDestPath, BlockingCollection<DepthFrameEventArgs> queue)
        {
            this._info = info;
            this._tsDestPath = tsDestPath;
            this._videoDestPath = videoDestPath;
            this._queue = queue;
        }

        public void Start()
        {
            this._writingTask = Task.Factory.StartNew(this.Runner);
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
            using (var tsFileStream = new FileStream(this._tsDestPath, FileMode.Append))
            using (var tsStreamWriter = new StreamWriter(tsFileStream))
            {
                while (!this._queue.IsCompleted)
                {
                    DepthFrameEventArgs data = null;
                    while (this._queue.TryTake(out data, 100))
                    {
                        tsStreamWriter.WriteLine(String.Format("{0} {1}", data.RelativeTime.TotalMilliseconds, data.TimeStamp));
                        yield return new BitmapVideoFrameWrapper(data.ToBitmap());
                    }
                }
            }
            this._queue.Dispose();
        }
        private void Runner()
        {
            var writer = FFMpegArguments
                .FromPipeInput(new RawVideoPipeSource(this.GetFrames()), options => options
                    .WithFramerate(this._info.FPS))
                .OutputToFile(this._videoDestPath, true, options => options
                    .WithFramerate(this._info.FPS)
                    .WithVideoCodec("ffv1")
                    .WithCustomArgument("-level 3")
                    .WithCustomArgument("-slicecrc 1")
                    .WithCustomArgument("-slices 24")
                    .WithConstantRateFactor(10)
                    .UsingThreads(4)
                    )
                .ProcessSynchronously();
        }
    }
}

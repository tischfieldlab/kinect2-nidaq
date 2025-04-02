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
    public class KinectColorWriterAlt : IDataWriter
    {
        private BlockingCollection<ColorFrameEventArgs> _queue;
        private TimeSpan _initialTimeSpan;

        private string _tsDestPath;
        private string _videoDestPath;

        private Task _writingTask;

        private ColorInfo _info;


        public KinectColorWriterAlt(ColorInfo info, string tsDestPath, string videoDestPath, BlockingCollection<ColorFrameEventArgs> queue)
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
                        yield return new BitmapVideoFrameWrapper(colorData.ToBitmap().ToSystemBitmap());
                    }
                }
            }
            this._queue.Dispose();
        }

        private async void ColorRunner()
        {
            var writer = FFMpegArguments
                .FromPipeInput(new RawVideoPipeSource(this.GetFrames()), options => options
                    .WithFramerate(this._info.FPS))
                .OutputToFile(this._videoDestPath, true, options => options
                    .WithVideoCodec("h264")
                    .WithFramerate(this._info.FPS)
                    .WithSpeedPreset(Speed.UltraFast)

                    )
                .ProcessAsynchronously();
            var result = await writer;
            await Task.Delay(1000);
            var x = 2 + 2;
        }
    }

    public class BitmapVideoFrameWrapper : IVideoFrame, IDisposable
    {
        public int Width => Source.Width;

        public int Height => Source.Height;

        public string Format { get; private set; }

        public Bitmap Source { get; private set; }

        public BitmapVideoFrameWrapper(Bitmap bitmap)
        {
            Source = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Format = ConvertStreamFormat(bitmap.PixelFormat);
        }

        public void Serialize(Stream stream)
        {
            var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

            try
            {
                var buffer = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                Source.UnlockBits(data);
            }
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token)
        {
            var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

            try
            {
                var buffer = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                await stream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
            }
            finally
            {
                Source.UnlockBits(data);
            }
        }

        public void Dispose()
        {
            Source.Dispose();
        }

        private static string ConvertStreamFormat(System.Drawing.Imaging.PixelFormat fmt)
        {
            switch (fmt)
            {
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return "gray16le";
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return "bgr555le";
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return "bgr565le";
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return "bgr24";
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return "bgra";
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    //This is not really same as argb32
                    return "argb";
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return "rgba";
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return "rgb48le";
                default:
                    throw new NotSupportedException($"Not supported pixel format {fmt}");
            }
        }
    }
}

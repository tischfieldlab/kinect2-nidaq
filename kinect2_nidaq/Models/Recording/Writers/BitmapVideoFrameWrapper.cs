using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace kinect2_nidaq.Models
{
    public class BitmapVideoFrameWrapper : IVideoFrame, IDisposable
    {
        public int Width => Source.PixelWidth;

        public int Height => Source.PixelHeight;

        public string Format { get; private set; }

        public BitmapSource Source { get; private set; }

        public BitmapVideoFrameWrapper(BitmapSource bitmap)
        {
            Source = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Format = ConvertStreamFormat(bitmap.Format);
        }

        public void Serialize(Stream stream)
        {
            var stride = this.Width * ((this.Source.Format.BitsPerPixel + 7) / 8);
            var buffer = new byte[stride * this.Height];
            Source.CopyPixels(buffer, stride, 0);
            stream.Write(buffer, 0, buffer.Length);
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token)
        {
            var stride = this.Width * ((this.Source.Format.BitsPerPixel + 7) / 8);
            var buffer = new byte[stride * this.Height];
            Source.CopyPixels(buffer, stride, 0);
            await stream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            //Source.Dispose();
        }

        private static string ConvertStreamFormat(PixelFormat fmt)
        {
            if (fmt == PixelFormats.Gray16)
                    return "gray16le";
            /*else if (fmt == PixelFormats.Format16bppRgb555)
                    return "bgr555le";
            else if (fmt == PixelFormats.Format16bppRgb565)
                return "bgr565le";
            else if (fmt == PixelFormats.Bgr24)
                return "bgr24";*/
            else if (fmt == PixelFormats.Bgra32)
                return "bgra";
            /*else if (fmt == PixelFormats.Format32bppPArgb)
                //This is not really same as argb32
                return "argb";
            else if (fmt == PixelFormats.Format32bppRgb)
                return "rgba";
            else if (fmt == PixelFormats.Format48bppRgb)
                return "rgb48le";*/
            else
                throw new NotSupportedException($"Not supported pixel format {fmt}");
        }
    }
}

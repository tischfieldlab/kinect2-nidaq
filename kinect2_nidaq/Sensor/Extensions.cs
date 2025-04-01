using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Kinect;

namespace Sensor
{

    public static class Extensions
    {
        /// <summary>
        /// Convert to bitmap for writing out
        /// </summary>
        /// <param name="wbmp"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ToSystemBitmap(this WriteableBitmap wbmp)
        {
            System.Drawing.Bitmap bitmap = null;

            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)wbmp));
                enc.Save(stream);
                bitmap = new System.Drawing.Bitmap(stream);
            }

            return bitmap;

        }

        /// <summary>
        /// Convert ColorFrame to bitmap
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static WriteableBitmap ToBitmap(this K2ColorFrameEventArgs e)
        {
            WriteableBitmap bitmap;
            bitmap = new WriteableBitmap(e.DepthInfo.Width, e.DepthInfo.Height, Constants.kDpi, Constants.kDpi, e.ColorInfo.Format, null);

            int stride = bitmap.PixelWidth * e.ColorInfo.BytesPerPixel;
            if (null != e.ColorSpacepoints)
            {
                byte[] colorData = new byte[e.ColorInfo.Size];
                int BytesPerPixel = bitmap.Format.BitsPerPixel / 8;

                int colorIndex = 0;
                for (int depthIndex = 0; depthIndex < e.ColorSpacepoints.Length; depthIndex++)
                {
                    ColorSpacePoint colorSpacePoint = e.ColorSpacepoints[depthIndex];
                    int colorX = (int)(colorSpacePoint.X);
                    int colorY = (int)(colorSpacePoint.Y);

                    if (colorX >= 0 && colorX < e.ColorInfo.Width && colorY >= 0 && colorY < e.ColorInfo.Height)
                    {
                        for (int offset = 0; offset < BytesPerPixel; ++offset)
                        {
                            colorData[colorIndex + offset] = e.ColorData[(colorY * e.ColorInfo.Width + colorX) * BytesPerPixel + offset];
                        }
                    }

                    colorIndex += BytesPerPixel;
                }

                bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), colorData, stride, 0);
            }
            bitmap.Freeze();
            return bitmap;
        }

        public static WriteableBitmap ToBitmap(this K4AColorFrameEventArgs e)
        {
            WriteableBitmap bitmap = new WriteableBitmap(e.ColorInfo.Width, e.ColorInfo.Height, Constants.kDpi, Constants.kDpi, e.ColorInfo.Format, null);
            int stride = bitmap.PixelWidth * e.ColorInfo.BytesPerPixel;
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), e.ColorData, stride, 0);
            bitmap.Freeze();
            return bitmap;
        }

        public static WriteableBitmap ToBitmap(this ColorFrameEventArgs e)
        {
            if (e is K4AColorFrameEventArgs)
            {
                return (e as K4AColorFrameEventArgs).ToBitmap();
            }
            else if (e is K2ColorFrameEventArgs)
            {
                return (e as K2ColorFrameEventArgs).ToBitmap();
            }
            else
            {
                throw new InvalidCastException("This method only works with subtypes K4AColorFrameEventArgs or K2ColorFrameEventArgs! Got " + e.GetType().Name);
            }
        }

        public static WriteableBitmap ToBitmap(this DepthFrameEventArgs e)
        {

            PixelFormat format = PixelFormats.Bgr32;

            //ushort minDepth = e.DepthMinReliableDistance;
            //ushort maxDepth = e.DepthMaxReliableDistance;

            ushort minDepth;
            ushort maxDepth;

            try
            {
                minDepth = kinect2_nidaq.Properties.Settings.Default.DepthMinValue;
                maxDepth = kinect2_nidaq.Properties.Settings.Default.DepthMaxValue;
            }
            catch
            {
                minDepth = e.DepthMinReliableDistance;
                maxDepth = e.DepthMaxReliableDistance;
            }


            minDepth = (ushort)(minDepth >= 0 ? minDepth : 1);
            maxDepth = (ushort)(maxDepth > minDepth ? maxDepth : minDepth + 1);

            byte[] pixels = new byte[e.DepthInfo.Size * (format.BitsPerPixel + 7) / 8];
            int colorIndex = 0;

            for (int depthIndex = 0; depthIndex < e.DepthData.Length; ++depthIndex)
            {

                ushort depth = e.DepthData[depthIndex];

                //byte intensity = (byte)(depth >= minDepth ? depth : 0);
                //intensity = (byte)(depth <= maxDepth ? depth : 0);

                float intensity = (float)(depth);

                intensity = (intensity >= minDepth ? intensity : -1);
                intensity = (intensity <= maxDepth ? intensity : -1);
                intensity = (intensity - minDepth) / (maxDepth - minDepth);

                // negative values are mapped to 1 (which becomes 0)

                intensity = (intensity < 0 ? 1 : intensity);

                // from float to byte (256 values)

                byte intensityB = (byte)(255 * (1 - intensity));

                pixels[colorIndex++] = intensityB;
                pixels[colorIndex++] = intensityB;
                pixels[colorIndex++] = intensityB;

                ++colorIndex;

            }

            int stride = e.DepthInfo.Width * format.BitsPerPixel / 8;
            WriteableBitmap bitmap;
            bitmap = new WriteableBitmap(e.DepthInfo.Width, e.DepthInfo.Height, Constants.kDpi, Constants.kDpi, format, null);
            bitmap.WritePixels(new Int32Rect(0, 0, e.DepthInfo.Width, e.DepthInfo.Height), pixels, stride, 0);
            bitmap.Freeze();
            return bitmap;
        }
        public static WriteableBitmap ToBitmap(this IRFrameEventArgs e)
        {
            var format = PixelFormats.Gray16;
            int stride = e.IRInfo.Width * format.BitsPerPixel / 8;

            WriteableBitmap bitmap;
            bitmap = new WriteableBitmap(e.IRInfo.Width, e.IRInfo.Height, Constants.kDpi, Constants.kDpi, format, null);
            bitmap.WritePixels(new Int32Rect(0, 0, e.IRInfo.Width, e.IRInfo.Height), e.IRData, stride, 0);
            bitmap.Freeze();
            return bitmap;
        }
    }
}

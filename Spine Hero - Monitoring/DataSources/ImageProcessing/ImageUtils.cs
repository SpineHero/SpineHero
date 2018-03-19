using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using SpineHero.Monitoring.Properties;
using Point = OpenCvSharp.Point;

namespace SpineHero.Monitoring.DataSources.ImageProcessing
{
    public static class ImageUtils
    {
        private static readonly int MM_PER_BYTE = Const.Default.MMPerByte;
        private static readonly int MIN_DISTANCE = Const.Default.MinDistance;
        private static readonly int MAX_DISTANCE = MIN_DISTANCE + 255 * MM_PER_BYTE;

        public static BitmapSource ToBitmapSource(Bitmap image)
        {
            if (image == null) return null;
            IntPtr ptr = image.GetHbitmap(); //obtain the Hbitmap

            BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(ptr); //release the HBitmap
            return bs;
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            if (bitmapsource == null) return null;
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static byte TrimDepth(short depth)
        {
            if (depth >= MIN_DISTANCE && depth <= MAX_DISTANCE)
            {
                return (byte)((depth - MIN_DISTANCE) / MM_PER_BYTE);
            }
            return 0;
        }

        public static int GetDepthValue(Mat image, Point p, int m, int n)
        {
            if (m == 1 && n == 1) return image.Get<byte>(p.Y, p.X);
            var y = p.Y - m / 2;
            if (y < 0) y = 0;
            var yy = y + m;
            if (yy > image.Height) yy = image.Height - 1;
            var x = p.X - n / 2;
            if (x < 0) x = 0;
            var xx = x + n;
            if (xx > image.Width) xx = image.Width - 1;
            var submat = image[y, yy, x, xx];
            var bytes = new MatOfByte(submat);
            var indexer = bytes.GetIndexer();
            int count = 0;
            int sum = 0;

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var v = indexer[i, j];
                    sum += v;
                    if (v != 0) count++;
                }
            }
            submat.Dispose();
            bytes.Dispose();
            if (count == 0) return 0;
            return sum / count;
        }

        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
    }
}
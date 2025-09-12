using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace LineDrawer
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Плавное затухание изображения: каждый канал приближается к чёрному на долю fadeSpeed.
        /// Bgr32 без альфы.
        /// </summary>
        /// <param name="bitmap">Целевой битмап</param>
        /// <param name="fadeSpeed">0.0..1.0 — коэффициент смешивания к белому за кадр</param>
        public static void Fade(this WriteableBitmap bitmap, double fadeSpeed)
        {
            if (bitmap == null || fadeSpeed <= 0.0)
                return;

            var clamped = Math.Max(0.0, Math.Min(1.0, fadeSpeed));

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stride = bitmap.BackBufferStride;

            // Коэффициент затухания (умножение каналов)
            var k = 1.0 - clamped;

            bitmap.Lock();
            try
            {
                unsafe
                {
                    byte* basePtr = (byte*)bitmap.BackBuffer.ToPointer();
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = basePtr + y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int idx = x * 4;
                            row[idx + 0] = (byte)(row[idx + 0] * k); // B
                            row[idx + 1] = (byte)(row[idx + 1] * k); // G
                            row[idx + 2] = (byte)(row[idx + 2] * k); // R
                        }
                    }
                }

                bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            }
            finally
            {
                bitmap.Unlock();
            }
        }
    }
}



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
            var byteCount = stride * height;

            var buffer = new byte[byteCount];

            bitmap.Lock();
            try
            {
                Marshal.Copy(bitmap.BackBuffer, buffer, 0, byteCount);

                // Bgr32: 4 байта на пиксель: B, G, R, X
                for (int i = 0; i < byteCount; i += 4)
                {
                    // Смешиваем к чёрному (0): new = old * (1 - clamped)
                    buffer[i + 0] = (byte)(buffer[i + 0] * (1.0 - clamped)); // B
                    buffer[i + 1] = (byte)(buffer[i + 1] * (1.0 - clamped)); // G
                    buffer[i + 2] = (byte)(buffer[i + 2] * (1.0 - clamped)); // R
                    // buffer[i + 3] не используется в Bgr32
                }

                Marshal.Copy(buffer, 0, bitmap.BackBuffer, byteCount);
                bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            }
            finally
            {
                bitmap.Unlock();
            }
        }
    }
}



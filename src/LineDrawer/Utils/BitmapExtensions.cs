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
        /// <param name="fadeSpeed">0.0..1.0 — коэффициент смешивания за шаг</param>
        /// <param name="gridStep">Шаг сетки (1 — каждый пиксель)</param>
        /// <param name="phaseX">Смещение по X в пределах [0..gridStep-1]</param>
        /// <param name="phaseY">Смещение по Y в пределах [0..gridStep-1]</param>
        public static void Fade(this WriteableBitmap bitmap, double fadeSpeed, int gridStep = 1, int phaseX = 0, int phaseY = 0)
        {
            if (bitmap == null || fadeSpeed <= 0.0)
                return;

            var clamped = Math.Max(0.0, Math.Min(1.0, fadeSpeed));

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stride = bitmap.BackBufferStride;

            // Коэффициент затухания (умножение каналов)
            var k = 1.0 - clamped;
            if (gridStep < 1) gridStep = 1;
            phaseX = ((phaseX % gridStep) + gridStep) % gridStep;
            phaseY = ((phaseY % gridStep) + gridStep) % gridStep;

            bitmap.Lock();
            try
            {
                unsafe
                {
                    byte* basePtr = (byte*)bitmap.BackBuffer.ToPointer();
                    for (int y = phaseY; y < height; y += gridStep)
                    {
                        byte* row = basePtr + y * stride;
                        for (int x = phaseX; x < width; x += gridStep)
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



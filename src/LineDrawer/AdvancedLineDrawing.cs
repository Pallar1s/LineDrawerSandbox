using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LineDrawer
{
    /// <summary>
    /// Класс для расширенной отрисовки линий с антиалиасингом и сглаживанием
    /// </summary>
    public static class AdvancedLineDrawing
    {
        // Кэш для сглаженных точек
        private static readonly Dictionary<string, List<Vector2>> SmoothPointsCache = new Dictionary<string, List<Vector2>>();
        private static readonly object CacheLock = new object();
        /// <summary>
        /// Отрисовка линии с улучшенным антиалиасингом
        /// </summary>
        /// <param name="bitmap">Битмап для отрисовки</param>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="color">Цвет линии</param>
        /// <param name="thickness">Толщина линии</param>
        /// <param name="antialiasingLevel">Уровень антиалиасинга (1-5)</param>
        public static void DrawLineWithAntialiasing(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, 
            Color color, int thickness, int antialiasingLevel = 3)
        {
            if (antialiasingLevel <= 1)
            {
                // Простая отрисовка без антиалиасинга
                bitmap.DrawLineAa(x1, y1, x2, y2, color, thickness);
                return;
            }

            // Множественная отрисовка для создания эффекта антиалиасинга
            var alphaStep = 255 / antialiasingLevel;
            var offsetStep = 1.0f / antialiasingLevel;
            
            for (int i = 0; i < antialiasingLevel; i++)
            {
                var offset = (i - antialiasingLevel / 2.0f) * offsetStep;
                var alpha = Math.Max(10, 255 - i * alphaStep);
                
                var antialiasedColor = Color.FromArgb((byte)alpha, color.R, color.G, color.B);
                
                // Отрисовка с небольшим смещением
                var offsetX1 = x1 + (int)(offset * Math.Sign(x2 - x1));
                var offsetY1 = y1 + (int)(offset * Math.Sign(y2 - y1));
                var offsetX2 = x2 + (int)(offset * Math.Sign(x2 - x1));
                var offsetY2 = y2 + (int)(offset * Math.Sign(y2 - y1));
                
                bitmap.DrawLineAa(offsetX1, offsetY1, offsetX2, offsetY2, antialiasedColor, thickness);
            }
        }

        /// <summary>
        /// Отрисовка сглаженной линии с использованием алгоритма Брезенхема
        /// </summary>
        /// <param name="bitmap">Битмап для отрисовки</param>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="color">Цвет линии</param>
        /// <param name="thickness">Толщина линии</param>
        /// <param name="smoothingLevel">Уровень сглаживания (1-10)</param>
        public static void DrawSmoothLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, 
            Color color, int thickness, int smoothingLevel = 5)
        {
            if (smoothingLevel <= 1)
            {
                bitmap.DrawLineAa(x1, y1, x2, y2, color, thickness);
                return;
            }

            // Применяем сглаживание через интерполяцию
            var points = GenerateSmoothPoints(x1, y1, x2, y2, smoothingLevel);
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                
                // Градиентная прозрачность для плавного перехода
                var alpha = (byte)(255 - (i * 20 / smoothingLevel));
                var smoothColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                
                bitmap.DrawLineAa((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, smoothColor, thickness);
            }
        }

        /// <summary>
        /// Комбинированная отрисовка с антиалиасингом и сглаживанием
        /// </summary>
        /// <param name="bitmap">Битмап для отрисовки</param>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="color">Цвет линии</param>
        /// <param name="thickness">Толщина линии</param>
        /// <param name="antialiasingLevel">Уровень антиалиасинга</param>
        /// <param name="smoothingLevel">Уровень сглаживания</param>
        public static void DrawAdvancedLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, 
            Color color, int thickness, int antialiasingLevel = 3, int smoothingLevel = 5)
        {
            if (smoothingLevel > 1)
            {
                // Сначала применяем сглаживание
                var points = GenerateSmoothPoints(x1, y1, x2, y2, smoothingLevel);
                
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    
                    // Затем применяем антиалиасинг к каждому сегменту
                    if (antialiasingLevel > 1)
                    {
                        bitmap.DrawLineWithAntialiasing((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, 
                            color, thickness, antialiasingLevel);
                    }
                    else
                    {
                        bitmap.DrawLineAa((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, color, thickness);
                    }
                }
            }
            else if (antialiasingLevel > 1)
            {
                // Только антиалиасинг
                bitmap.DrawLineWithAntialiasing(x1, y1, x2, y2, color, thickness, antialiasingLevel);
            }
            else
            {
                // Стандартная отрисовка
                bitmap.DrawLineAa(x1, y1, x2, y2, color, thickness);
            }
        }

        /// <summary>
        /// Генерация сглаженных точек для линии с кэшированием
        /// </summary>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="smoothingLevel">Уровень сглаживания</param>
        /// <returns>Список сглаженных точек</returns>
        private static List<Vector2> GenerateSmoothPoints(int x1, int y1, int x2, int y2, int smoothingLevel)
        {
            // Создаем ключ для кэша
            var cacheKey = $"{x1},{y1},{x2},{y2},{smoothingLevel}";
            
            lock (CacheLock)
            {
                if (SmoothPointsCache.TryGetValue(cacheKey, out var cachedPoints))
                {
                    return cachedPoints;
                }
            }
            
            var points = new List<Vector2>();
            var segments = Math.Max(2, smoothingLevel * 2);
            
            for (int i = 0; i <= segments; i++)
            {
                var t = (float)i / segments;
                
                // Применяем функцию сглаживания (ease-in-out)
                var smoothT = SmoothStep(t);
                
                var x = x1 + (x2 - x1) * smoothT;
                var y = y1 + (y2 - y1) * smoothT;
                
                points.Add(new Vector2(x, y));
            }
            
            // Кэшируем результат
            lock (CacheLock)
            {
                if (SmoothPointsCache.Count > 1000) // Ограничиваем размер кэша
                {
                    SmoothPointsCache.Clear();
                }
                SmoothPointsCache[cacheKey] = points;
            }
            
            return points;
        }

        /// <summary>
        /// Функция сглаживания (smoothstep)
        /// </summary>
        /// <param name="t">Параметр от 0 до 1</param>
        /// <returns>Сглаженное значение</returns>
        private static float SmoothStep(float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return t * t * (3.0f - 2.0f * t);
        }

        /// <summary>
        /// Очистка кэша сглаженных точек
        /// </summary>
        public static void ClearCache()
        {
            lock (CacheLock)
            {
                SmoothPointsCache.Clear();
            }
        }

        /// <summary>
        /// Получение статистики кэша
        /// </summary>
        /// <returns>Количество элементов в кэше</returns>
        public static int GetCacheSize()
        {
            lock (CacheLock)
            {
                return SmoothPointsCache.Count;
            }
        }

        /// <summary>
        /// Отрисовка линии с эффектом размытия
        /// </summary>
        /// <param name="bitmap">Битмап для отрисовки</param>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="color">Цвет линии</param>
        /// <param name="thickness">Толщина линии</param>
        /// <param name="blurRadius">Радиус размытия</param>
        public static void DrawBlurredLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, 
            Color color, int thickness, int blurRadius = 2)
        {
            // Отрисовка основной линии
            bitmap.DrawLineAa(x1, y1, x2, y2, color, thickness);
            
            // Добавляем размытие
            for (int r = 1; r <= blurRadius; r++)
            {
                var alpha = (byte)(50 / r); // Уменьшаем прозрачность с увеличением радиуса
                var blurredColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                
                // Отрисовка в разных направлениях для создания эффекта размытия
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (dx * dx + dy * dy <= r * r)
                        {
                            bitmap.DrawLineAa(x1 + dx, y1 + dy, x2 + dx, y2 + dy, blurredColor, thickness);
                        }
                    }
                }
            }
        }
    }
}

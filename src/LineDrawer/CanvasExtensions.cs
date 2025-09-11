using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LineDrawer
{
    public static class CanvasExtensions
    {
        public static void DrawCircle(this Canvas cv, int x, int y, int width, int height, Color color)
        {
            var brush = new SolidColorBrush {Color = color};
            Ellipse circle = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = brush
            };

            circle.Opacity = 0.5;
            cv.Children.Add(circle);

            circle.SetValue(Canvas.LeftProperty, (double)x - width/2);
            circle.SetValue(Canvas.TopProperty, (double)y - height/2);
        }

        public static void DrawLine(this Canvas cv, int x1, int y1, int x2, int y2, Color color)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush {Color = color},
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };

            cv.Children.Add(line);
        }

        /// <summary>
        /// Отрисовка сглаженной линии на Canvas с антиалиасингом
        /// </summary>
        /// <param name="cv">Canvas для отрисовки</param>
        /// <param name="x1">Начальная X координата</param>
        /// <param name="y1">Начальная Y координата</param>
        /// <param name="x2">Конечная X координата</param>
        /// <param name="y2">Конечная Y координата</param>
        /// <param name="color">Цвет линии</param>
        /// <param name="thickness">Толщина линии</param>
        /// <param name="enableAntialiasing">Включить антиалиасинг</param>
        public static void DrawSmoothLine(this Canvas cv, int x1, int y1, int x2, int y2, Color color, 
            double thickness = 5, bool enableAntialiasing = true)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush {Color = color},
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };

            if (enableAntialiasing)
            {
                // Включаем сглаживание для лучшего качества
                line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Unspecified);
                line.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
            }

            cv.Children.Add(line);
        }
    }
}
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
                StrokeThickness = 5
            };

            cv.Children.Add(line);
        }
    }
}
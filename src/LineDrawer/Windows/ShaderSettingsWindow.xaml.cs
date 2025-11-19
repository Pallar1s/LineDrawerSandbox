using System.Windows;

namespace LineDrawer.Windows
{
    public partial class ShaderSettingsWindow : Window
    {
        public ShaderSettingsWindow(DrawingModel model)
        {
            this.InitializeComponent();
            this.DataContext = model;
        }
    }
}


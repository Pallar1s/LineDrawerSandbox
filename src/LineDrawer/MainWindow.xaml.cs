using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageProducer;
using Newtonsoft.Json;

namespace LineDrawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private JointImageProducer producer;

        private const int BitmapSize = 2000;
        private const int BitmapSizeHalf = BitmapSize / 2;
        
        private WriteableBitmap bitmap = new WriteableBitmap(BitmapSize, BitmapSize, 96, 96, PixelFormats.Bgr32, null);

        private Matrix3x2 scaleTransform;
        private readonly DrawingModel model;
        
        public MainWindow()
        {
            ObservableCollection<ProducerModelInfo> modelCollection;

            try
            {
                var presetsJson = File.ReadAllText("Presets\\presets.json");
                var presetItems = JsonConvert.DeserializeObject<ProducerModelInfo[]>(presetsJson);
                modelCollection = new ObservableCollection<ProducerModelInfo>(presetItems);
            }
            catch
            {
                modelCollection = new ObservableCollection<ProducerModelInfo>();
            }

            this.model = new DrawingModel
            {
                Joints = new ObservableCollection<JointModelInfo>(),
                OverallSpeed = 10,
                PauseRender = true,
                ShowJoints = true,
                Presets = modelCollection 
            };
            
            this.model.PropertyChanged += ModelOnPropertyChanged;

            InitializeComponent();
            this.DataContext = this.model;
            
            if (modelCollection.Count > 0)
                this.PresetsComboBox.SelectedItem = modelCollection.First();
            
            this.Reset();
            Task.Run(this.Run);
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.model.OverallSpeed))
                this.producer.Speed = this.model.OverallSpeed * 0.1f;
        }

        private void Reset()
        {
            this.model.PauseRender = true;
            this.model.PreviousPositions = null;
            
            var maxSize = this.model.Joints.Sum(x => x.Size) + 10.0f;
            
            var joints = this.model.Joints.Select(x =>
                new JointInfo
                {
                    Size = x.Size / maxSize,
                    Speed = x.Speed / 100_000f
                }).ToList();

            this.producer = new JointImageProducer(joints)
            {
                Speed = this.model.OverallSpeed * 0.1f
            };
            
            this.bitmap.Clear();
            this.model.PauseRender = false;
        }

        private async Task Run()
        {
            while (true)
            {
                if (this.model.Halt)
                    return;
                
                if (!this.model.PauseRender)
                {
                    var positions = this.producer.Tick().ToArray();

                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.model.Halt)
                            return;
                        
                        DrawModel(positions);
                    });

                    this.model.PreviousPositions = positions;
                }

                await Task.Delay(10);
            }
        }

        private void DrawModel(Vector2[] positions)
        {
            if (this.model.PreviousPositions != null)
            {
                var i = 0;
                foreach (var pos in this.model.PreviousPositions)
                {
                    this.bitmap.DrawLineAa(
                        (int)(pos.X * BitmapSizeHalf) + BitmapSizeHalf,
                        (int)(pos.Y * BitmapSizeHalf) + BitmapSizeHalf,
                        (int)(positions[i].X * BitmapSizeHalf) + BitmapSizeHalf,
                        (int)(positions[i].Y * BitmapSizeHalf) + BitmapSizeHalf, Colors.White, 4);
                    i++;
                }
            }

            this.MainImage.Source = this.bitmap;

            this.MainCanvas.Children.Clear();

            if (this.model.ShowJoints)
            {
                var prevX = 0;
                var prevY = 0;

                foreach (var pos in positions)
                {
                    var vector = Vector2.Transform(pos, this.scaleTransform);
                    var newX = (int)vector.X;
                    var newY = (int)vector.Y;
                    this.MainCanvas.DrawCircle(newX, newY, 10, 10);
                    this.MainCanvas.DrawLine(prevX, prevY, newX, newY);
                                
                    prevX = newX;
                    prevY = newY;
                }
            }
        }

        private void MainImage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.scaleTransform = Matrix3x2.CreateScale((float)this.MainImage.ActualWidth/2, (float)this.MainImage.ActualHeight/2);
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Reset();
        }

        private void PresetsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var modelInfo = this.PresetsComboBox.SelectedItem as ProducerModelInfo;

            if (modelInfo != null)
            {
                this.model.Joints.Clear();
                foreach (var info in modelInfo.Joints)
                {
                    this.model.Joints.Add(info);
                }
            }
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.model.Halt = true;
        }
    }
}
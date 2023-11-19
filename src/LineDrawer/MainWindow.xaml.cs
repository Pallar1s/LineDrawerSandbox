using System;
using System.Collections.Generic;
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

        private const int TraceLength = 35;
        private const int BitmapSize = 2000;
        private const int BitmapSizeHalf = BitmapSize / 2;
        private double colorIteration = 0.0d;
        private Color defaultBitmapColor = Colors.White;
        private Color defaultTraceColor = Colors.Red;
        
        private WriteableBitmap bitmap = new WriteableBitmap(BitmapSize, BitmapSize, 96, 96, PixelFormats.Bgr32, null);
        private Queue<Vector2> traceQueue = new Queue<Vector2>(TraceLength);

        private Matrix3x2 scaleTransform;
        private readonly DrawingModel model;
        
        private long producerVersion = 0;
        
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
                OverallSpeed = 2,
                PauseRender = true,
                ShowJoints = true,
                ShowTrace = true,
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
            this.producerVersion = DateTime.UtcNow.Ticks;
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
            
            this.traceQueue.Clear();
            this.bitmap.Clear();
            this.model.PauseRender = false;
        }

        private async Task Run()
        {
            while (true)
            {
                if (this.model.Halt)
                    return;

                var currentVersion = this.producerVersion;
                if (!this.model.PauseRender && currentVersion == this.producerVersion)
                {
                    var positions = this.producer.Tick().ToArray();

                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.model.Halt || this.model.PauseRender)
                            return;
                        
                        if (currentVersion == this.producerVersion)
                            DrawModel(positions);
                    });

                    if (!this.model.PauseRender && currentVersion == this.producerVersion)
                        this.model.PreviousPositions = positions;
                }

                await Task.Delay(10);
            }
        }

        private void DrawModel(Vector2[] positions)
        {
            var drawTraceColor = this.defaultTraceColor;
            var drawBitmapColor = this.defaultBitmapColor;

            if (this.model.UseGradient)
            {
                var gradientColor = ColorExtensions.Hsl2Rgb(this.colorIteration, 0.5, 0.5);
                this.colorIteration += 0.001;

                if (this.colorIteration >= 1.0d)
                    this.colorIteration = 0;

                drawTraceColor = gradientColor;
                drawBitmapColor = gradientColor;
            }

            if (this.model.PreviousPositions != null)
            {
                var i = 0;
                foreach (var pos in this.model.PreviousPositions)
                {
                    var modelJoint = this.model.Joints[i];

                    if (modelJoint.Enabled)
                    {
                        this.bitmap.DrawLineAa(
                            (int)(pos.X * BitmapSizeHalf) + BitmapSizeHalf,
                            (int)(pos.Y * BitmapSizeHalf) + BitmapSizeHalf,
                            (int)(positions[i].X * BitmapSizeHalf) + BitmapSizeHalf,
                            (int)(positions[i].Y * BitmapSizeHalf) + BitmapSizeHalf, drawBitmapColor, 6);
                    }

                    i++;
                }
            }

            this.MainImage.Source = this.bitmap;
            this.MainCanvas.Children.Clear();

            var prevX = 0;
            var prevY = 0;

            Vector2 lastPos = default;

            if (this.model.ShowJoints || this.model.ShowTrace)
            {
                foreach (var pos in positions)
                {
                    var vector = Vector2.Transform(pos, this.scaleTransform);
                    var newX = (int)vector.X;
                    var newY = (int)vector.Y;

                    if (this.model.ShowJoints)
                    {
                        this.MainCanvas.DrawCircle(newX, newY, 10, 10, new Color { R = 255, A = 255 });
                        this.MainCanvas.DrawLine(prevX, prevY, newX, newY, color: Color.FromRgb(0, 255, 0));
                    }

                    prevX = newX;
                    prevY = newY;
                    lastPos = pos;
                }

                this.traceQueue.Enqueue(lastPos);

                if (this.traceQueue.Count > TraceLength)
                    this.traceQueue.Dequeue();

                if (this.model.ShowTrace)
                {
                    var i = 0;
                    var traceColor = drawTraceColor;
                    foreach (var item in this.traceQueue)
                    {
                        double opacity = i / (double)TraceLength;
                        var vector = Vector2.Transform(item, this.scaleTransform);
                        var newX = (int)vector.X;
                        var newY = (int)vector.Y;
                        traceColor.A = (byte)(opacity * 255);
                        this.MainCanvas.DrawCircle(newX, newY, (int)(20 * opacity), (int)(20 * opacity), traceColor);
                        i++;
                    }
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
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using ImageProducer;
using LineDrawer.Effects;
using LineDrawer.Windows;
using Newtonsoft.Json;

namespace LineDrawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private JointImageProducer producer;
        
        private const int BitmapSize = 2048;
        private const int BitmapSizeHalf = BitmapSize / 2;
        private double colorIteration = 0.0d;
        
        private WriteableBitmap bitmap = new WriteableBitmap(BitmapSize, BitmapSize, 96, 96, PixelFormats.Bgr32, null);
        
        private Matrix3x2 scaleTransform;
        private DateTime previousTime;
        private double lastDeltaSeconds;
        private double fadeInterval;
        private int fadePhaseX;
        private int fadePhaseY;
        private readonly DrawingModel model;
        private ParametersWindow parametersWindow;
        private ShaderSettingsWindow shaderSettingsWindow;
        private readonly Dictionary<PostEffectMode, ShaderEffect> postEffects;
        
        public DrawingModel Model => this.model;
        
        private long producerVersion = 0;
        
        public MainWindow()
        {
            ObservableCollection<ProducerModelInfo> modelCollection;

            try
            {
                var presetsJson = File.ReadAllText("presets.json");
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
                Presets = modelCollection,
                EnableAntialiasing = true,
                AntialiasingLevel = 3,
                EnableSmoothing = true,
                SmoothingLevel = 5,
                LineThickness = 6,
                EnableFading = false,
                FadeSpeed = 0.4,
                FadeGridStep = 3,
                EnablePostEffect = true,
                PostEffectMode = PostEffectMode.SoftGlow
            };
            
            // Устанавливаем первый пресет как текущий, если есть
            if (modelCollection.Count > 0)
            {
                this.model.CurrentPreset = modelCollection.First();
            }
            
            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.model.ShaderParametersChanged += ModelOnShaderParametersChanged;
            this.model.ModelReset += ModelOnModelReset;
            
            InitializeComponent();
            this.DataContext = this.model;

            this.postEffects = new Dictionary<PostEffectMode, ShaderEffect>
            {
                { PostEffectMode.SoftGlow, new SoftGlowEffect() },
                { PostEffectMode.EdgePulse, new EdgePulseEffect() },
                { PostEffectMode.ChromaticAberration, new ChromaticAberrationEffect() },
                { PostEffectMode.FogOverlay, new FogOverlayEffect() }
            };
            
            this.Reset();
            previousTime = DateTime.Now;
            this.MainImage.Source = this.bitmap;
            this.UpdatePostEffect();
            Task.Run(this.Run);
        }

        private void ModelOnModelReset(object? sender, EventArgs e)
        {
            this.Reset();
        }

        private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.model.OverallSpeed))
                this.producer.Speed = this.model.OverallSpeed * 0.1f;
            
            if (e.PropertyName is nameof(DrawingModel.EnablePostEffect) or nameof(DrawingModel.PostEffectMode))
                this.UpdatePostEffect();
        }

        public void Reset()
        {
            this.model.PauseRender = true;
            
            this.producerVersion = DateTime.UtcNow.Ticks;
            this.model.PreviousPositions = null;
            
            var maxSize = this.model.Joints.Sum(x => x.Size) + 10.0f;
            
            var joints = this.model.Joints.Select(x =>
                new JointInfo
                {
                    Size = x.Size / maxSize,
                    Speed = x.Speed / 100_000f,
                    PulseEnabled = x.PulseEnabled,
                    PulseMinCoef = x.PulseMinCoef,
                    PulseSpeed = x.PulseSpeed,
                    ColorR = x.ColorR,
                    ColorG = x.ColorG,
                    ColorB = x.ColorB
                }).ToList();

            this.producer = new JointImageProducer(joints)
            {
                Speed = this.model.OverallSpeed * 0.1f
            };
            
            this.bitmap.Clear();
            AdvancedLineDrawing.ClearCache(); // Очищаем кэш при сбросе
            this.model.PauseRender = false;
            this.fadePhaseX = 0;
            this.fadePhaseY = 0;
        }

        private async Task Run()
        {
            while (true)
            {
                if (this.model.Halt)
                    return;

                var currentVersion = this.producerVersion;
                var currentTime = DateTime.Now;
                var dt = currentTime - this.previousTime;
                this.lastDeltaSeconds = dt.TotalSeconds;
                if (!this.model.PauseRender && currentVersion == this.producerVersion)
                {
                    var positions = this.producer.Tick(dt).ToArray();

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
                this.previousTime = currentTime;
            }
        }

        private void DrawModel(Vector2[] positions)
        {
            // Плавное затухание текущего содержимого битмапа (с интервалом и сеткой)
            if (this.model is { EnableFading: true, FadeSpeed: > 0 })
                this.FadeImage();

            //Основная отрисовка
            if (this.model.PreviousPositions != null)
                this.DrawNewStepOnBitmap(positions);

            this.DrawEffects(positions);
        }
        
        private void FadeImage()
        {
            var needFade = this.fadeInterval >= (0.1 - 0.1 * this.model.FadeSpeed);

            if (needFade)
            {
                var step = Math.Max(1, this.model.FadeGridStep);
                this.bitmap.Fade(0.05, step, this.fadePhaseX, this.fadePhaseY);
                // сдвигаем фазу, чтобы постепенно покрыть всю сетку
                this.fadePhaseX = (this.fadePhaseX + 1) % step;
                if (this.fadePhaseX == 0)
                    this.fadePhaseY = (this.fadePhaseY + 1) % step;
                this.fadeInterval = 0;
            }
            else
            {
                this.fadeInterval += this.lastDeltaSeconds;
            }
        }

        private void DrawNewStepOnBitmap(Vector2[] positions)
        {
            var i = 0;
            foreach (var pos in this.model.PreviousPositions)
            {
                var modelJoint = this.model.Joints[i];

                if (modelJoint.Enabled)
                {
                    var jointColor = modelJoint.GetColor();
                    if (modelJoint.Gradient)
                        modelJoint.ColorIteration += 0.001;
                        
                    var x1 = (int)(pos.X * BitmapSizeHalf) + BitmapSizeHalf;
                    var y1 = (int)(pos.Y * BitmapSizeHalf) + BitmapSizeHalf;
                    var x2 = (int)(positions[i].X * BitmapSizeHalf) + BitmapSizeHalf;
                    var y2 = (int)(positions[i].Y * BitmapSizeHalf) + BitmapSizeHalf;
                        
                    // Используем улучшенную отрисовку линий
                    if (this.model.EnableAntialiasing || this.model.EnableSmoothing)
                    {
                        this.bitmap.DrawAdvancedLine(x1, y1, x2, y2, jointColor, this.model.LineThickness, 
                            this.model.EnableAntialiasing ? this.model.AntialiasingLevel : 1,
                            this.model.EnableSmoothing ? this.model.SmoothingLevel : 1);
                    }
                    else
                    {
                        // Стандартная отрисовка
                        this.bitmap.DrawLineAa(x1, y1, x2, y2, jointColor, this.model.LineThickness);
                    }
                }

                i++;
            }
        }

        private void DrawEffects(Vector2[] positions)
        {
            this.MainCanvas.Children.Clear();
            
            var prevX = 0;
            var prevY = 0;
            var k = 0;
            foreach (var pos in positions)
            {
                var vector = Vector2.Transform(pos, this.scaleTransform);
                var newX = (int)vector.X;
                var newY = (int)vector.Y;

                if (this.model.ShowJoints)
                {
                    this.MainCanvas.DrawCircle(newX, newY, 10, 10, new Color { R = 255, A = 255 });
                    this.MainCanvas.DrawSmoothLine(prevX, prevY, newX, newY, Color.FromRgb(0, 255, 0), 
                        3, this.model.EnableAntialiasing);
                }

                var joint = this.model.Joints[k];
                
                if (joint.ShowTrace)
                {
                    var traceQueue = this.model.Joints[k].TraceQueue;
                    traceQueue.Enqueue(pos);
                    
                    if (traceQueue.Count > JointModelInfo.TraceLength)
                        traceQueue.Dequeue();
                    
                    var i = 0;
                    var  traceColor = joint.GetColor();
                     
                    foreach (var item in traceQueue)
                    {
                        double opacity = i / (double)JointModelInfo.TraceLength;
                        var traceVector = Vector2.Transform(item, this.scaleTransform);
                        var traceNewX = (int)traceVector.X;
                        var traceNewY = (int)traceVector.Y;
                        traceColor.A = (byte)(opacity * 255);
                        this.MainCanvas.DrawCircle(traceNewX, traceNewY, (int)(20 * opacity), (int)(20 * opacity), traceColor);
                        i++;
                    }
                }

                prevX = newX;
                prevY = newY;
                k++;
            }
        }

        private void MainImage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.scaleTransform = Matrix3x2.CreateScale((float)this.MainImage.ActualWidth/2, (float)this.MainImage.ActualHeight/2);
        }

        public void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var modelInfo = e.AddedItems.Count > 0 ? e.AddedItems[0] as ProducerModelInfo : null;

            if (modelInfo != null && modelInfo != this.model.CurrentPreset)
            {
                this.model.CurrentPreset = modelInfo;
                this.Reset();
            }
        }

        private void ParametersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.parametersWindow == null)
            {
                this.parametersWindow = new ParametersWindow(this);
                this.parametersWindow.Show();
            }
            else
            {
                if (!parametersWindow.IsVisible)
                    this.parametersWindow.Show();
                
                this.parametersWindow.Activate();
            }
        }

        private void ShaderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.shaderSettingsWindow == null)
            {
                this.shaderSettingsWindow = new ShaderSettingsWindow(this.model)
                {
                    Owner = this
                };
                this.shaderSettingsWindow.Closed += (_, _) => this.shaderSettingsWindow = null;
                this.shaderSettingsWindow.Show();
            }
            else
            {
                if (!this.shaderSettingsWindow.IsVisible)
                    this.shaderSettingsWindow.Show();
                
                this.shaderSettingsWindow.Activate();
            }
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            this.model.Halt = true;
            
            if (parametersWindow != null)
                parametersWindow.Close();
            
            if (shaderSettingsWindow != null)
                shaderSettingsWindow.Close();
        }
        
        private void ModelOnShaderParametersChanged(object? sender, EventArgs e)
        {
            this.Dispatcher.Invoke(this.UpdatePostEffect);
        }
        
        private void UpdatePostEffect()
        {
            if (this.MainImage == null)
                return;
            
            if (!this.model.EnablePostEffect || this.model.PostEffectMode == PostEffectMode.None)
            {
                this.MainImage.Effect = null;
                return;
            }

            if (!this.postEffects.TryGetValue(this.model.PostEffectMode, out var effect))
            {
                this.MainImage.Effect = null;
                return;
            }

            switch (effect)
            {
                case SoftGlowEffect softGlow:
                    softGlow.Strength = this.model.GetShaderParameterValue(ShaderParameterKeys.Strength);
                    break;
                case EdgePulseEffect edgePulse:
                    edgePulse.Strength = this.model.GetShaderParameterValue(ShaderParameterKeys.Strength);
                    break;
                case ChromaticAberrationEffect chromatic:
                    var offset = this.model.GetShaderParameterValue(ShaderParameterKeys.Offset);
                    chromatic.Offset = offset * 0.001;
                    break;
                case FogOverlayEffect fog:
                    fog.Density = this.model.GetShaderParameterValue(ShaderParameterKeys.FogDensity);
                    fog.Height = this.model.GetShaderParameterValue(ShaderParameterKeys.FogHeight);
                    break;
            }

            this.MainImage.Effect = effect;
        }
    }
}
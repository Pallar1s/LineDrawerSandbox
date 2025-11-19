using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using LineDrawer.Annotations;

namespace LineDrawer
{
    public enum PostEffectMode
    {
        None,
        SoftGlow,
        EdgePulse,
        ChromaticAberration,
        FogOverlay
    }
    
    public class DrawingModel : INotifyPropertyChanged
    {
        private bool showJoints;
        private int overallSpeed;
        private bool enableAntialiasing;
        private int antialiasingLevel;
        private bool enableSmoothing;
        private int smoothingLevel;
        private int lineThickness;
        private bool enableFading;
        private double fadeSpeed;
        private int fadeGridStep;
        private bool enablePostEffect;
        private PostEffectMode postEffectMode = PostEffectMode.SoftGlow;
        private readonly ObservableCollection<ShaderParameterModel> shaderParameters = new();
        
        public DrawingModel()
        {
            this.Joints = new ObservableCollection<JointModelInfo>();
            this.RebuildShaderParameters();
        }
        
        public ObservableCollection<JointModelInfo> Joints { get; set; }
        
        public bool ShowJoints
        {
            get => this.showJoints;
            set
            {
                if (this.showJoints != value)
                {
                    this.showJoints = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public int OverallSpeed
        {
            get => this.overallSpeed;
            set
            {
                if (this.overallSpeed != value)
                {
                    this.overallSpeed = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public int LineThickness
        {
            get => this.lineThickness;
            set
            {
                var newValue = Math.Max(1, Math.Min(50, value));
                if (this.lineThickness != newValue)
                {
                    this.lineThickness = newValue;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool EnableAntialiasing
        {
            get => this.enableAntialiasing;
            set
            {
                if (this.enableAntialiasing != value)
                {
                    this.enableAntialiasing = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public int AntialiasingLevel
        {
            get => this.antialiasingLevel;
            set
            {
                if (this.antialiasingLevel != value)
                {
                    this.antialiasingLevel = Math.Max(1, Math.Min(5, value)); // Ограничиваем от 1 до 5
                    this.OnPropertyChanged();
                }
            }
        }

        public bool EnableSmoothing
        {
            get => this.enableSmoothing;
            set
            {
                if (this.enableSmoothing != value)
                {
                    this.enableSmoothing = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public int SmoothingLevel
        {
            get => this.smoothingLevel;
            set
            {
                if (this.smoothingLevel != value)
                {
                    this.smoothingLevel = Math.Max(1, Math.Min(10, value)); // Ограничиваем от 1 до 10
                    this.OnPropertyChanged();
                }
            }
        }

        public bool EnableFading
        {
            get => this.enableFading;
            set
            {
                if (this.enableFading != value)
                {
                    this.enableFading = value;
                    this.OnPropertyChanged();
                }
            }
        }

        // 0.0 - без затухания, 1.0 - очень быстрое затухание (на кадр)
        public double FadeSpeed
        {
            get => this.fadeSpeed;
            set
            {
                var clamped = Math.Max(0.0, Math.Min(1.0, value));
                if (Math.Abs(this.fadeSpeed - clamped) > double.Epsilon)
                {
                    this.fadeSpeed = clamped;
                    this.OnPropertyChanged();
                }
            }
        }

        // Шаг сетки для выборочного затухания (1 — каждый пиксель)
        public int FadeGridStep
        {
            get => this.fadeGridStep;
            set
            {
                var clamped = Math.Max(1, Math.Min(32, value));
                if (this.fadeGridStep != clamped)
                {
                    this.fadeGridStep = clamped;
                    this.OnPropertyChanged();
                }
            }
        }
        
        public bool EnablePostEffect
        {
            get => this.enablePostEffect;
            set
            {
                if (this.enablePostEffect != value)
                {
                    this.enablePostEffect = value;
                    this.OnPropertyChanged();
                }
            }
        }
        
        public PostEffectMode PostEffectMode
        {
            get => this.postEffectMode;
            set
            {
                if (this.postEffectMode != value)
                {
                    this.postEffectMode = value;
                    this.OnPropertyChanged();
                    this.RebuildShaderParameters();
                }
            }
        }
        
        public ObservableCollection<ShaderParameterModel> ShaderParameters => this.shaderParameters;
        
        public event EventHandler? ShaderParametersChanged;
        
        public bool PauseRender { get; set; }
        
        public bool Halt { get; set; }
        
        public Vector2[]? PreviousPositions { get; set; }
        
        public ObservableCollection<ProducerModelInfo> Presets { get; set; }
        
        private ProducerModelInfo? currentPreset;
        
        public ProducerModelInfo? CurrentPreset
        {
            get => this.currentPreset;
            set
            {
                if (this.currentPreset != value)
                {
                    if (this.currentPreset?.Joints != null)
                        this.currentPreset.Joints.CollectionChanged -= JointsOnCollectionChanged;
                    
                    this.currentPreset = value;
                    
                    if (this.currentPreset?.Joints != null)
                        this.currentPreset.Joints.CollectionChanged += JointsOnCollectionChanged;
                    
                    ResetJointsByPreset();
                    this.OnPropertyChanged();
                }
            }
        }
        
        private void JointsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ResetJointsByPreset();
        }
        
        private void ResetJointsByPreset()
        {
            Joints.Clear();

            if (this.currentPreset != null)
            {
                foreach (var joint in currentPreset.Joints)
                {
                    this.Joints.Add(joint);
                }
            }
            
            this.ModelReset?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler? ModelReset;
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public double GetShaderParameterValue(string key)
        {
            var parameter = this.shaderParameters.FirstOrDefault(p => p.Key == key);
            return parameter?.Value ?? 0.0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void RebuildShaderParameters()
        {
            foreach (var parameter in this.shaderParameters)
                parameter.ValueChanged -= ShaderParameterOnValueChanged;

            this.shaderParameters.Clear();

            foreach (var definition in ShaderParameterDefinitions.GetDefinitions(this.postEffectMode))
            {
                var parameter = new ShaderParameterModel(definition);
                parameter.ValueChanged += ShaderParameterOnValueChanged;
                this.shaderParameters.Add(parameter);
            }

            this.ShaderParametersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ShaderParameterOnValueChanged(object? sender, EventArgs e)
        {
            this.ShaderParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
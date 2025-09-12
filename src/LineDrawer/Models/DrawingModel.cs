using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using LineDrawer.Annotations;

namespace LineDrawer
{
    public class DrawingModel : INotifyPropertyChanged
    {
        private bool showJoints;
        private bool showTrace;
        private int overallSpeed;
        private bool useGradient;
        private bool enableAntialiasing;
        private int antialiasingLevel;
        private bool enableSmoothing;
        private int smoothingLevel;
        
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
        
        public bool ShowTrace
        {
            get => this.showTrace;
            set
            {
                if (this.showTrace != value)
                {
                    this.showTrace = value;
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
        
        public bool UseGradient
        {
            get => this.useGradient;
            set
            {
                if (this.useGradient != value)
                {
                    this.useGradient = value;
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

        public bool PauseRender { get; set; }
        
        public bool Halt { get; set; }
        
        public Vector2[]? PreviousPositions { get; set; }
        
        public ObservableCollection<ProducerModelInfo> Presets { get; set; }
        
        private ProducerModelInfo currentPreset;
        
        public ProducerModelInfo CurrentPreset
        {
            get => this.currentPreset;
            set
            {
                if (this.currentPreset != value)
                {
                    if (this.currentPreset?.Joints != null)
                    {
                        this.currentPreset.Joints.CollectionChanged -= JointsOnCollectionChanged;
                    }

                    this.currentPreset = value;
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
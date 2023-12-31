using System.Collections.ObjectModel;
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

        public bool PauseRender { get; set; }
        
        public bool Halt { get; set; }
        
        public Vector2[]? PreviousPositions { get; set; }
        
        public ObservableCollection<ProducerModelInfo> Presets { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
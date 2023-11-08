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
        private int overallSpeed;
        
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
        
        public bool PauseRender { get; set; }
        
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
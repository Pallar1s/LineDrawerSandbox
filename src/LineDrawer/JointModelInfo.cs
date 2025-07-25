using System.ComponentModel;
using System.Runtime.CompilerServices;
using LineDrawer.Annotations;

namespace LineDrawer
{
    public class JointModelInfo: INotifyPropertyChanged
    {
        private int size;
        private int speed;
        private bool enabled = true;
        private bool pulseEnabled = false;
        private float pulseMinCoef = 0.5f;
        private float pulseSpeed = 1.0f;
        
        public int Size
        {
            get { return this.size;}
            set
            {
                if (value != this.size)
                {
                    this.size = value;
                    this.OnPropertyChanged();
                }
            }
        }
        
        public int Speed {
            get
            {
                return this.speed;
            }
            set
            {
                if (value != this.speed)
                {
                    this.speed = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            
            set
            {
                if (value != this.enabled)
                {
                    this.enabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool PulseEnabled
        {
            get => this.pulseEnabled;
            set
            {
                if (this.pulseEnabled != value)
                {
                    this.pulseEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public float PulseMinCoef
        {
            get => this.pulseMinCoef;
            set
            {
                if (this.pulseMinCoef != value)
                {
                    this.pulseMinCoef = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public float PulseSpeed
        {
            get => this.pulseSpeed;
            set
            {
                if (this.pulseSpeed != value)
                {
                    this.pulseSpeed = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
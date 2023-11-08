using System.ComponentModel;
using System.Runtime.CompilerServices;
using LineDrawer.Annotations;

namespace LineDrawer
{
    public class JointModelInfo: INotifyPropertyChanged
    {
        private int size;
        private int speed;
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
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
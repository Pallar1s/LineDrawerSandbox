using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LineDrawer.Annotations;

namespace LineDrawer
{
    public class ProducerModelInfo: INotifyPropertyChanged
    {
        private string name;
        
        public string Name
        {
            get => this.name;
            set
            {
                if (name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public JointModelInfo[] Joints { get; set; }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ProducerModelInfo CreateNew()
        {
            return new ProducerModelInfo
            {
                Name = "New preset",
                Joints = new JointModelInfo[]
                {
                    new JointModelInfo
                    {
                        JointColor = Colors.Green,
                        Size = 100,
                        Speed = 1000
                    }
                }
            };
        }
    }
}
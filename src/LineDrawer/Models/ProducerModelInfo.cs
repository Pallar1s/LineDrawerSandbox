using System;
using System.Collections.ObjectModel;
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
        public ObservableCollection<JointModelInfo> Joints { get; set; }
        
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
                Joints = new ObservableCollection<JointModelInfo>
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

        public void Randomize()
        {
            var jointsCount = Random.Shared.Next(2, 7);

            Joints.Clear();
            for (int i = 0; i < jointsCount; i++)
            {
                
                var newJoint = new JointModelInfo
                {
                    Size = Random.Shared.Next(1, 100) * 5,
                    Speed = Random.Shared.Next(-20, 20) * 1000,
                    Enabled = true,
                    ColorR = Random.Shared.Next(0, 255),
                    ColorG = Random.Shared.Next(0, 255),
                    ColorB = Random.Shared.Next(0, 255),
                };
                Joints.Add(newJoint);
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LineDrawer
{
    public class ShaderParameterModel : INotifyPropertyChanged
    {
        private double value;

        public ShaderParameterModel(ShaderParameterDefinition definition)
        {
            this.Key = definition.Key;
            this.DisplayName = definition.DisplayName;
            this.Min = definition.Min;
            this.Max = definition.Max;
            this.value = definition.DefaultValue;
        }

        public string Key { get; }
        public string DisplayName { get; }
        public double Min { get; }
        public double Max { get; }

        public double Value
        {
            get => this.value;
            set
            {
                var clamped = Math.Max(this.Min, Math.Min(this.Max, value));
                if (Math.Abs(this.value - clamped) > double.Epsilon)
                {
                    this.value = clamped;
                    this.OnPropertyChanged();
                    this.ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? ValueChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


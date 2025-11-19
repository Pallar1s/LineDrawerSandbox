using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LineDrawer.Effects
{
    public class ChromaticAberrationEffect : ShaderEffect
    {
        private static readonly PixelShader PixelShaderResource = new PixelShader
        {
            UriSource = new Uri("/LineDrawer;component/Shaders/ChromaticAberration.ps", UriKind.Relative)
        };

        public ChromaticAberrationEffect()
        {
            this.PixelShader = PixelShaderResource;
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(OffsetProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ChromaticAberrationEffect), 0);

        public double Offset
        {
            get => (double)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(double), typeof(ChromaticAberrationEffect),
                new UIPropertyMetadata(0.002, PixelShaderConstantCallback(0)));
    }
}


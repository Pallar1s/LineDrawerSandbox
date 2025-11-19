using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LineDrawer.Effects
{
    public class SoftGlowEffect : ShaderEffect
    {
        private static readonly PixelShader PixelShaderResource = new PixelShader
        {
            UriSource = new Uri("/LineDrawer;component/Shaders/SoftGlow.ps", UriKind.Relative)
        };

        public SoftGlowEffect()
        {
            this.PixelShader = PixelShaderResource;
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(StrengthProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(SoftGlowEffect), 0);

        public double Strength
        {
            get => (double)GetValue(StrengthProperty);
            set => SetValue(StrengthProperty, value);
        }

        public static readonly DependencyProperty StrengthProperty =
            DependencyProperty.Register(nameof(Strength), typeof(double), typeof(SoftGlowEffect),
                new UIPropertyMetadata(0.8, PixelShaderConstantCallback(0)));
    }
}


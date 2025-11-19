using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LineDrawer.Effects
{
    public class EdgePulseEffect : ShaderEffect
    {
        private static readonly PixelShader PixelShaderResource = new PixelShader
        {
            UriSource = new Uri("/LineDrawer;component/Shaders/EdgePulse.ps", UriKind.Relative)
        };

        public EdgePulseEffect()
        {
            this.PixelShader = PixelShaderResource;
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(StrengthProperty);
            this.UpdateShaderValue(GlowColorProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(EdgePulseEffect), 0);

        public double Strength
        {
            get => (double)GetValue(StrengthProperty);
            set => SetValue(StrengthProperty, value);
        }

        public static readonly DependencyProperty StrengthProperty =
            DependencyProperty.Register(nameof(Strength), typeof(double), typeof(EdgePulseEffect),
                new UIPropertyMetadata(0.8, PixelShaderConstantCallback(0)));

        public Color GlowColor
        {
            get => (Color)GetValue(GlowColorProperty);
            set => SetValue(GlowColorProperty, value);
        }

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register(nameof(GlowColor), typeof(Color), typeof(EdgePulseEffect),
                new UIPropertyMetadata(Color.FromRgb(0, 200, 255), PixelShaderConstantCallback(1)));
    }
}


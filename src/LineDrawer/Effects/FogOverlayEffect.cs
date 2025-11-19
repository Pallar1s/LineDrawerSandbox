using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace LineDrawer.Effects
{
    public class FogOverlayEffect : ShaderEffect
    {
        private static readonly PixelShader PixelShaderResource = new PixelShader
        {
            UriSource = new Uri("/LineDrawer;component/Shaders/FogOverlay.ps", UriKind.Relative)
        };

        public FogOverlayEffect()
        {
            this.PixelShader = PixelShaderResource;
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(DensityProperty);
            this.UpdateShaderValue(HeightProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(FogOverlayEffect), 0);

        public double Density
        {
            get => (double)GetValue(DensityProperty);
            set => SetValue(DensityProperty, value);
        }

        public static readonly DependencyProperty DensityProperty =
            DependencyProperty.Register(nameof(Density), typeof(double), typeof(FogOverlayEffect),
                new UIPropertyMetadata(0.4, PixelShaderConstantCallback(0)));

        public double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(nameof(Height), typeof(double), typeof(FogOverlayEffect),
                new UIPropertyMetadata(1.0, PixelShaderConstantCallback(1)));
    }
}


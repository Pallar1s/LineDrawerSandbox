using System;
using System.Collections.Generic;

namespace LineDrawer
{
    public record ShaderParameterDefinition(string Key, string DisplayName, double Min, double Max, double DefaultValue);

    public static class ShaderParameterKeys
    {
        public const string Strength = "strength";
        public const string Offset = "offset";
        public const string FogDensity = "fogDensity";
        public const string FogHeight = "fogHeight";
    }

    public static class ShaderParameterDefinitions
    {
        private static readonly Dictionary<PostEffectMode, ShaderParameterDefinition[]> Definitions = new()
        {
            {
                PostEffectMode.SoftGlow, new[]
                {
                    new ShaderParameterDefinition(ShaderParameterKeys.Strength, "Интенсивность", 0.0, 2.0, 0.9)
                }
            },
            {
                PostEffectMode.EdgePulse, new[]
                {
                    new ShaderParameterDefinition(ShaderParameterKeys.Strength, "Интенсивность", 0.0, 2.0, 0.8)
                }
            },
            {
                PostEffectMode.ChromaticAberration, new[]
                {
                    new ShaderParameterDefinition(ShaderParameterKeys.Offset, "Смещение", 0.0, 5.0, 1.5)
                }
            },
            {
                PostEffectMode.FogOverlay, new[]
                {
                    new ShaderParameterDefinition(ShaderParameterKeys.FogDensity, "Плотность", 0.0, 1.0, 0.4),
                    new ShaderParameterDefinition(ShaderParameterKeys.FogHeight, "Высота", 0.2, 3.0, 1.0)
                }
            }
        };

        public static IReadOnlyList<ShaderParameterDefinition> GetDefinitions(PostEffectMode mode) =>
            Definitions.TryGetValue(mode, out var defs) ? defs : Array.Empty<ShaderParameterDefinition>();
    }
}


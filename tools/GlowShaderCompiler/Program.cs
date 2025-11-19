using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

internal record ShaderDefinition(string Name, string SourceFile, string OutputFile);

internal static class Program
{
    public static void Main()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var hlslDirectory = Path.Combine(projectRoot, "Shaders");
        var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));
        var shadersDirectory = Path.Combine(repoRoot, "src", "LineDrawer", "Shaders");
        Directory.CreateDirectory(shadersDirectory);

        foreach (var shader in Shaders)
        {
            var sourcePath = Path.Combine(hlslDirectory, shader.SourceFile);
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Source file '{sourcePath}' not found, skipping.");
                continue;
            }

            var sourceText = File.ReadAllText(sourcePath);
            var outputPath = Path.Combine(shadersDirectory, shader.OutputFile);
            ShaderCompiler.Compile(shader.Name, sourceText, outputPath);
        }
    }

    private static readonly ShaderDefinition[] Shaders =
    {
        new("SoftGlow", "SoftGlow.hlsl", "SoftGlow.ps"),
        new("EdgePulse", "EdgePulse.hlsl", "EdgePulse.ps"),
        new("ChromaticAberration", "ChromaticAberration.hlsl", "ChromaticAberration.ps"),
        new("FogOverlay", "FogOverlay.hlsl", "FogOverlay.ps")
    };
}

internal static class ShaderCompiler
{
    public static unsafe void Compile(string name, string source, string outputPath)
    {
        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(source);
        fixed (byte* ptr = sourceBytes)
        {
            var result = Compiler.Compile(ptr, new PointerUSize((uint)sourceBytes.Length), name, null, null,
                "main", "ps_2_0", ShaderFlags.OptimizationLevel3, EffectFlags.None, out Blob compiled, out Blob errorBlob);

            if (result.Failure)
            {
                Console.WriteLine($"{name} failed: {errorBlob.AsString()}");
                return;
            }

            File.WriteAllBytes(outputPath, compiled.AsBytes().ToArray());
            Console.WriteLine($"Shader '{name}' -> {outputPath} ({compiled.BufferSize} bytes)");
        }
    }
}

sampler2D inputSampler : register(s0);
float Strength : register(c0);
float3 GlowColor : register(c1);

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(inputSampler, uv);
    float luminance = dot(baseColor.rgb, float3(0.299, 0.587, 0.114));
    float2 delta = float2(0.0015, 0.0015);
    
    float edge = 0.0;
    float3 weights = float3(0.299, 0.587, 0.114);
    edge += abs(luminance - dot(tex2D(inputSampler, uv + float2(delta.x, 0)).rgb, weights));
    edge += abs(luminance - dot(tex2D(inputSampler, uv - float2(delta.x, 0)).rgb, weights));
    edge += abs(luminance - dot(tex2D(inputSampler, uv + float2(0, delta.y)).rgb, weights));
    edge += abs(luminance - dot(tex2D(inputSampler, uv - float2(0, delta.y)).rgb, weights));
    edge = saturate(edge * 0.75);
    
    float3 glow = GlowColor * edge * Strength;
    baseColor.rgb = saturate(baseColor.rgb + glow);
    return baseColor;
}


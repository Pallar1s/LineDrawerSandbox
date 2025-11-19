sampler2D inputSampler : register(s0);
float Strength : register(c0);

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(inputSampler, uv);
    float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float3 glow = luminance.xxx * float3(0.2, 0.8, 1.0);
    color.rgb = saturate(color.rgb + glow * Strength);
    return color;
}


sampler2D inputSampler : register(s0);
float Density : register(c0);
float Height : register(c1);

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(inputSampler, uv);
    float gradient = saturate(1.0 - uv.y);
    float heightFactor = saturate(pow(gradient, Height));
    float fogAmount = saturate(heightFactor * Density);
    float3 fogColor = float3(0.85, 0.9, 1.0);
    color.rgb = lerp(color.rgb, fogColor, fogAmount);
    return color;
}


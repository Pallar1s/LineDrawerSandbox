sampler2D inputSampler : register(s0);
float Offset : register(c0);

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 shift = float2(Offset, Offset * 0.5);
    float red = tex2D(inputSampler, uv + shift).r;
    float green = tex2D(inputSampler, uv).g;
    float blue = tex2D(inputSampler, uv - shift).b;
    float alpha = tex2D(inputSampler, uv).a;
    return float4(red, green, blue, alpha);
}


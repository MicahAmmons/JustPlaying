sampler2D TextureSampler : register(s0);

// Set from C# before drawing
float4 TargetColor; // RGBA, values 0..1

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample the texture at this UV coordinate
    float4 texColor = tex2D(TextureSampler, texCoord);

    // Calculate perceived brightness from the texture's RGB (luminance)
    float brightness = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Replace RGB with TargetColor scaled by brightness
    // Preserve texture alpha and multiply by vertex color alpha
    return float4(TargetColor.rgb * brightness, texColor.a * color.a);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}

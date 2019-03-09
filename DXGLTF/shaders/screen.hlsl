Texture2D ShaderTexture: register(t0);
SamplerState Sampler: register(s0);

struct VS_IN
{
	float3 position : POSITION;
	float2 uv: TEXCOORD0;
	float4 color: COLOR0;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float2 uv: TEXCOORD0;
	float4 color: COLOR0;
};


PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.position = float4(input.position, 1);
	output.uv = float2(input.uv.x, input.uv.y);
	output.color = input.color; 
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return ShaderTexture.Sample(Sampler, input.uv);
}

struct VS_IN
{
	float3 position : POSITION;
	float3 normal : NORMAL;
    float4 uv: TEXCOORD0;
};

struct PS_IN
{
	float4 position : SV_POSITION;
    float4 uv: TEXCOORD0;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.position = float4(input.position, 0);
	output.uv = input.uv;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.uv;
}

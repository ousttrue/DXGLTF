struct VS_IN
{
	float3 position : POSITION;
	float4 color: COLOR0;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float4 color: COLOR0;
};

cbuffer Camera: register(b0)
{
	float4x4 wvp;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.position = mul(float4(input.position, 1), wvp);
	output.color = input.color; 
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.color;
}

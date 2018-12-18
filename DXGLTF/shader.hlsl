struct VS_IN
{
	float3 position : POSITION;
};

struct PS_IN
{
	float4 position : SV_POSITION;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.position = float4(input.position.xy, 0.5, 0.5);
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return float4(1, 1, 1, 1);
}

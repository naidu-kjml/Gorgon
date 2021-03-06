// Texture and sampler for blitting a texture.
Texture2D _texture : register(t0);
SamplerState _sampler : register(s0);

// The ID of the vertex streamed from our application into the shader.
// We use this ID to generate vertices on the fly to pass to our geometry shader.
struct VertexIn
{
	uint ID : SV_VertexID;
};

// Our default blitting vertex.
struct VertexOutput
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float2 uv : TEXCOORD;
};

// The transformation matrices (for vertex shader).
cbuffer WorldViewProjection : register(b0)
{
	float4x4 Projection;
	float4x4 World;	
	float Offset;
}

// Our vertex shader used to generate vertices for our geometry shader.
VertexOutput BufferlessVs(VertexIn vsIn)
{
	VertexOutput output;

	switch(vsIn.ID)
	{
        case 0:
            output.position = float4(0, -0.5f, 0.5f, 1.0f);
            output.color = float4(1.0f, 0.0f, 0.0f, 1.0f);			
			output.uv = float2(0.5f, 0);
            break;
        case 1:
            output.position = float4(0.5f, -0.5f, -0.5f, 1.0f);
            output.color = float4(0.0f, 1.0f, 0.0f, 1.0f);
			output.uv = float2(1.0f, 1.0f);
            break;
        case 2:
            output.position = float4(-0.5f, -0.5f, -0.5f, 1.0f);
            output.color = float4(0.0f, 0.0f, 1.0f, 1.0f);
			output.uv = float2(0, 1.0f);
            break;
	}

	output.position = mul(World, output.position);	

	return output;
}

// Our vertex shader for drawing to our render target.
VertexOutput VsMain(VertexOutput vertex)
{
	VertexOutput output = vertex;

	output.position = mul(World * Projection, output.position);

	return output;
}

// Our geometry shader.  
// This is used to generate our primitives on the fly from a list of triangle vertices generated by the vertex shader above.
[maxvertexcount(12)]
void GsMain( triangle VertexOutput input[3], inout TriangleStream<VertexOutput> output )
{
	VertexOutput outVtx;

	float4 tipPos = float4(((input[0].position.xyz + input[1].position.xyz + input[2].position.xyz) / 3.0f) + float3(0, Offset, 0), 1.0f);

	// Build the walls of the pyramid.
	for (int i = 0; i < 3; ++i)
	{
		outVtx.position = mul(Projection, input[i].position);
		outVtx.uv = input[1].uv;
		outVtx.color = input[i].color;
		output.Append(outVtx);

		int index = (i + 1) % 3;
		outVtx.position = mul(Projection, input[index].position);
		outVtx.uv = input[2].uv;
		outVtx.color = input[index].color;
		output.Append(outVtx);

		outVtx.position = mul(Projection, tipPos);
		outVtx.uv = input[0].uv;
		outVtx.color = input[0].color;
		output.Append(outVtx);

		output.RestartStrip();
	}

	// Draw the actual triangle we sent in.
	for (int i = 2; i >= 0; --i)
	{
		outVtx.position = mul(Projection, input[i].position);
		outVtx.uv = input[i].uv;
		outVtx.color = input[i].color;
		output.Append(outVtx);
	}

	output.RestartStrip();
}

// Our pixel shader for drawing to our render target.
float4 PsMain(VertexOutput vertex) : SV_Target
{
	return _texture.Sample(_sampler, vertex.uv) * vertex.color;	
}

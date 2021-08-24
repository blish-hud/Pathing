#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

#define DEBUG false
#define DEBUGBORDER 0.475

float3 PlayerPosition;

float Opacity;

float FadeNear;
float FadeFar;

float4 TintColor;

float PlayerFadeRadius;
bool FadeCenter;
bool FadeNearCamera;
float3 CameraPosition;

bool ShowDebugWireframe;

matrix World;
matrix View;
matrix Projection;
matrix PlayerView;

int Race;
int Mount;

static const float RaceMaskRad[] = {
	0.10, // Asura
	0.20, // Charr
	0.13, // Human
	0.21, // Norn
	0.13  // Sylvari
};

static const float MountRadOffset[] = {
	0.00, // None
	0.08, // Jackal
	0.11, // Griffon
	0.10, // Springer
	0.12, // Skimmer
	0.11, // Raptor
	0.14, // Roller Beetle
	0.11, // Warclaw
	0.11, // Skyscale
};

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0) {
    Texture = (Texture);
};

Texture2D FadeTexture : register(t1);
sampler FadeTextureSampler : register(s1) {
    Texture = (FadeTexture);
};

struct VSInput {
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VSOutput {
    float4 Position : SV_Position;
    float2 TextureCoordinate : TEXCOORD0;
    float  Distance : float;
    float3 ProjectedPosition : float3;
	float CameraDistance : A1;
	float CameraPlayerDistance : A2;
};

struct PixelShaderOutput {
    float4 Color : COLOR0;
};

float GetFadeRad() {
	return RaceMaskRad[Race] + MountRadOffset[Mount] + 0.25;
}

VSOutput VertexShaderFunction(VSInput input) {
    VSOutput output;

    matrix modelView = mul(World, View);

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    viewPosition = mul(input.Position, modelView);

    output.Position = mul(viewPosition, Projection);

    // Get distance player is from marker
    output.Distance = distance(worldPosition.xyz, PlayerPosition) / 0.0254f;
	output.CameraDistance = distance(worldPosition.xyz, CameraPosition) / 0.0254f;
	output.CameraPlayerDistance = distance(PlayerPosition, CameraPosition) / 0.0254f;

    output.ProjectedPosition = normalize(mul(worldPosition, PlayerView).xyz) * (distance(CameraPosition, PlayerPosition) * 0.1);

    output.TextureCoordinate = input.TextureCoordinate.xy;

    return output;
}

float DissolvePosition(float2 position, float2 projectedPosition) {
    float3 color = tex2D(FadeTextureSampler, position).rgb;
    half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
	
	return lerp(-1.5, 1.0, length(projectedPosition) / GetFadeRad());
}

PixelShaderOutput PixelShaderFunction(VSOutput input) {
    PixelShaderOutput output;

	// Marker is fully faded out due to its distance
	if (FadeFar > 0) {
		clip(FadeFar - input.Distance);
	}
	
	// Debug outline (outlines the quad)
	if (ShowDebugWireframe && (abs(input.TextureCoordinate.x - 0.5) > DEBUGBORDER || abs(input.TextureCoordinate.y - 0.5) > DEBUGBORDER)) {
		output.Color.rgba = float4(1, 0, 0, 1);
		return output;
	}
	
	bool inCenter = false;

    if (FadeCenter && length(input.ProjectedPosition.xy) < GetFadeRad()) {
		Opacity = Opacity * clamp(DissolvePosition(input.TextureCoordinate, input.ProjectedPosition.xy), 0.075, 1.0);
		
		inCenter = true;
    } 
	
	if (FadeNear > 0 && FadeFar > 0) {
		// If the marker is within the two fade distances, we start masking it
        float3 color = tex2D(FadeTextureSampler, input.TextureCoordinate).rgb;
        half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
        float nearDist = input.Distance - FadeNear;
		float mult = clamp(nearDist / (FadeFar - FadeNear), 0.0, 1.0);
        clip(val - mult);
		
		Opacity = Opacity * (1.0 - mult);
    }
	
	if (FadeNearCamera) {
		Opacity = Opacity * clamp(1.0 - (input.CameraPlayerDistance - input.CameraDistance) / input.CameraPlayerDistance * 2.0, 0.0, 1.0);
	}

    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * TintColor * Opacity;

	if (DEBUG && inCenter) {
		output.Color.rgba = output.Color.rgba * 0.5;
		output.Color.r = 0.75;
	}

    return output;
}

technique {
    pass {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
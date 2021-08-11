#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

float TotalMilliseconds;
float FlowSpeed;
float3 PlayerPosition;

float Opacity;

float FadeNear;
float FadeFar;

float PlayerFadeRadius;
bool FadeCenter;

float FadeDistance;

float4 TintColor;
float3 CameraPosition;

float4x4 PlayerView;
float4x4 WorldViewProjection;

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

float GetFadeRad() {
	return RaceMaskRad[Race] + MountRadOffset[Mount] + 0.15;
}

Texture2D Texture : register(t0);
sampler2D TextureSampler : register(s0) {
    Texture = (Texture);
};

Texture2D FadeTexture : register(t1);
sampler2D FadeTextureSampler : register(s1) {
    Texture = (FadeTexture);
};

struct VertexShaderInput {
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput {
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 ProjectedPosition : float3;
    float  Distance : float;
};

struct PixelShaderOutput {
    float4 Color : COLOR0;
};

// NOTE: The path is drawn backwards 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

    // Get distance player is from current spot in trail (so that we can fade it out a bit)
    output.Distance = distance(input.Position.xyz, PlayerPosition) / 0.0254f;

    output.ProjectedPosition = normalize(mul(input.Position, PlayerView).xyz) * (distance(CameraPosition, PlayerPosition) * 0.1);

    // Pass on to PS (some redundant for later)
    output.Color = input.Color * Opacity;

    // make the trail slowly move along the path
    output.TextureCoordinate = float2(input.TextureCoordinate.x, input.TextureCoordinate.y + (TotalMilliseconds / 1000) * FlowSpeed);

    return output;
}

float DissolvePosition(float2 position, float2 projectedPosition) {
    float3 color = tex2D(FadeTextureSampler, position).rgb;
    half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
	
	return lerp(-1.5, 1.0, length(projectedPosition) / GetFadeRad());
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) {
    PixelShaderOutput output;

    // Handle fade far (first since it'll clip and can skip the rest of this if it's too far away)
    clip(FadeFar - input.Distance);

	// Handle fadeCenter (if enabled by player) - we ignore this if the player is zoomed in really close
    if (FadeCenter && distance(PlayerPosition, CameraPosition) > 1.5 && length(input.ProjectedPosition.xy) < GetFadeRad()) {
		Opacity = Opacity * clamp(DissolvePosition(input.TextureCoordinate, input.ProjectedPosition.xy), 0.075, 1.0);
    }

    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * TintColor * input.Color * Opacity;

    return output;
}

technique {
    pass {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}

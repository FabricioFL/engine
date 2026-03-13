#version 330 core

struct PointLight {
    vec3 position;
    float range;
    vec3 color;
    float intensity;
};

layout(std140) uniform LightBlock {
    // Directional light (32 bytes)
    vec3 dirDirection;  float _pad0;
    vec3 dirColor;      float dirIntensity;
    // Ambient + count (16 bytes)
    vec3 ambientColor;  int pointLightCount;
    // Point lights (32 bytes each)
    PointLight pointLights[16];
};

uniform vec4 uColor;
uniform float uShininess;
uniform vec3 uViewPos;
uniform sampler2D uTexture;
uniform int uHasTexture;

in vec3 vFragPos;
in vec3 vNormal;
in vec2 vTexCoord;

out vec4 FragColor;

vec3 CalcDirLight(vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-dirDirection);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uShininess);
    return (diff + spec * 0.5) * dirColor * dirIntensity;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    float distance = length(light.position - fragPos);
    float attenuation = clamp(1.0 - distance / light.range, 0.0, 1.0);
    attenuation *= attenuation;
    return diff * light.color * light.intensity * attenuation;
}

void main()
{
    vec4 texColor = uHasTexture == 1 ? texture(uTexture, vTexCoord) : vec4(1.0);
    vec4 baseColor = uColor * texColor;

    if (baseColor.a < 0.01)
        discard;

    vec3 normal = normalize(vNormal);
    vec3 viewDir = normalize(uViewPos - vFragPos);

    vec3 result = ambientColor;
    result += CalcDirLight(normal, viewDir);

    for (int i = 0; i < pointLightCount && i < 16; i++)
        result += CalcPointLight(pointLights[i], normal, vFragPos, viewDir);

    FragColor = vec4(result * baseColor.rgb, baseColor.a);
}

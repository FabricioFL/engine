#version 330 core

uniform sampler2D uScene;
uniform sampler2D uDepth;
uniform vec3 uFogColor;
uniform float uFogStart;
uniform float uFogEnd;
uniform float uFogDensity;
uniform float uNearPlane;
uniform float uFarPlane;

in vec2 vTexCoord;
out vec4 FragColor;

float linearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0;
    return (2.0 * uNearPlane * uFarPlane) / (uFarPlane + uNearPlane - z * (uFarPlane - uNearPlane));
}

void main()
{
    vec3 sceneColor = texture(uScene, vTexCoord).rgb;
    float depth = texture(uDepth, vTexCoord).r;
    float linearDepth = linearizeDepth(depth);

    float fogFactor = clamp((linearDepth - uFogStart) / (uFogEnd - uFogStart), 0.0, 1.0);
    fogFactor = 1.0 - exp(-uFogDensity * linearDepth);
    fogFactor = clamp(fogFactor, 0.0, 1.0);

    FragColor = vec4(mix(sceneColor, uFogColor, fogFactor), 1.0);
}

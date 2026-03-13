#version 330 core

uniform sampler2D uScene;
uniform sampler2D uBloom;
uniform float uBloomIntensity;

in vec2 vTexCoord;
out vec4 FragColor;

void main()
{
    vec3 scene = texture(uScene, vTexCoord).rgb;
    vec3 bloom = texture(uBloom, vTexCoord).rgb;
    FragColor = vec4(scene + bloom * uBloomIntensity, 1.0);
}

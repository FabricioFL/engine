#version 330 core

uniform sampler2D uTexture;
uniform vec4 uColor;

in vec2 vTexCoord;

out vec4 FragColor;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    if (texColor.a < 0.01)
        discard;
    FragColor = texColor * uColor;
}

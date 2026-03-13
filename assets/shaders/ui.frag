#version 330 core

uniform sampler2D uTexture;
uniform int uHasTexture;
uniform vec4 uColor;

in vec2 vTexCoord;
out vec4 FragColor;

void main()
{
    if (uHasTexture == 1)
    {
        vec4 texColor = texture(uTexture, vTexCoord);
        FragColor = texColor * uColor;
    }
    else
    {
        FragColor = uColor;
    }
}

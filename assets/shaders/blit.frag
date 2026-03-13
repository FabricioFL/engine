#version 330 core

uniform sampler2D uScene;

in vec2 vTexCoord;
out vec4 FragColor;

void main()
{
    FragColor = texture(uScene, vTexCoord);
}

#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform vec2 uUvOffset;
uniform vec2 uUvScale;
uniform int uFlipX;
uniform int uFlipY;

out vec2 vTexCoord;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);

    vec2 uv = aTexCoord;
    if (uFlipX == 1) uv.x = 1.0 - uv.x;
    if (uFlipY == 1) uv.y = 1.0 - uv.y;
    vTexCoord = uUvOffset + uv * uUvScale;
}

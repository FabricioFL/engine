#version 330 core

uniform vec4 uColor;
uniform sampler2D uTexture;
uniform int uHasTexture;

in vec3 vNormal;
in vec2 vTexCoord;

out vec4 FragColor;

void main()
{
    vec4 texColor = uHasTexture == 1 ? texture(uTexture, vTexCoord) : vec4(1.0);
    vec3 normal = normalize(vNormal);

    // Simple directional light baked in
    vec3 lightDir = normalize(vec3(0.3, 1.0, 0.5));
    float diff = max(dot(normal, lightDir), 0.0);
    float ambient = 0.3;
    float lighting = ambient + diff * 0.7;

    FragColor = vec4(uColor.rgb * texColor.rgb * lighting, uColor.a * texColor.a);
}

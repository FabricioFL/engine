using System.Numerics;

namespace Engine.Rendering;

public sealed class Material
{
    public Shader Shader { get; }
    public Texture2D? DiffuseTexture { get; set; }
    public Vector4 Color { get; set; } = Vector4.One;
    public float Shininess { get; set; } = 32.0f;

    public Material(Shader shader)
    {
        Shader = shader;
    }

    public void Apply()
    {
        Shader.Use();
        Shader.SetVector4("uColor", Color);
        Shader.SetFloat("uShininess", Shininess);

        if (DiffuseTexture != null)
        {
            DiffuseTexture.Bind();
            Shader.SetInt("uTexture", 0);
            Shader.SetInt("uHasTexture", 1);
        }
        else
        {
            Shader.SetInt("uHasTexture", 0);
        }
    }
}

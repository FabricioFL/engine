using Silk.NET.OpenGL;

namespace Engine.Rendering;

public static class MeshFactory
{
    public static Mesh CreateQuad(GL gl)
    {
        float[] vertices =
        {
            // position          // normal           // texcoord
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, 0.0f,
             0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f, 0.0f,
             0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, 1.0f,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionNormalTexture);
    }

    public static Mesh CreateCube(GL gl)
    {
        float[] vertices =
        {
            // Front face
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,  0.0f, 1.0f,
            // Back face
             0.5f, -0.5f, -0.5f,  0.0f, 0.0f,-1.0f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,-1.0f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,-1.0f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f, 0.0f,-1.0f,  0.0f, 1.0f,
            // Top face
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f,  0.0f, 1.0f,
            // Bottom face
            -0.5f, -0.5f, -0.5f,  0.0f,-1.0f, 0.0f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f,-1.0f, 0.0f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f,-1.0f, 0.0f,  1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,-1.0f, 0.0f,  0.0f, 1.0f,
            // Right face
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
            // Left face
            -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f, -1.0f, 0.0f, 0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f, 0.0f, 0.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, -1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
        };
        uint[] indices =
        {
            0,1,2, 2,3,0,       // front
            4,5,6, 6,7,4,       // back
            8,9,10, 10,11,8,    // top
            12,13,14, 14,15,12, // bottom
            16,17,18, 18,19,16, // right
            20,21,22, 22,23,20  // left
        };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionNormalTexture);
    }

    public static Mesh CreatePlane(GL gl, float size = 10f)
    {
        float h = size / 2f;
        float[] vertices =
        {
            -h, 0, -h,  0,1,0,  0,0,
             h, 0, -h,  0,1,0,  size,0,
             h, 0,  h,  0,1,0,  size,size,
            -h, 0,  h,  0,1,0,  0,size,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionNormalTexture);
    }

    public static Mesh CreateScreenQuad(GL gl)
    {
        float[] vertices =
        {
            // position       // texcoord
            -1.0f, -1.0f, 0.0f,  0.0f, 0.0f,
             1.0f, -1.0f, 0.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,  1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f,  0.0f, 1.0f,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionTexture);
    }

    public static Mesh CreateCylinder(GL gl, float radius = 0.5f, float height = 1.0f, int segments = 12)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        float halfH = height / 2f;

        // Side faces
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathF.PI * 2f * i / segments;
            float x = MathF.Cos(angle) * radius;
            float z = MathF.Sin(angle) * radius;
            float nx = MathF.Cos(angle);
            float nz = MathF.Sin(angle);
            float u = (float)i / segments;

            // Bottom vertex
            vertices.AddRange(new[] { x, -halfH, z, nx, 0f, nz, u, 0f });
            // Top vertex
            vertices.AddRange(new[] { x,  halfH, z, nx, 0f, nz, u, 1f });
        }

        for (int i = 0; i < segments; i++)
        {
            uint b = (uint)(i * 2);
            indices.AddRange(new[] { b, b + 1, b + 3, b + 3, b + 2, b });
        }

        // Top cap
        uint topCenter = (uint)(vertices.Count / 8);
        vertices.AddRange(new[] { 0f, halfH, 0f, 0f, 1f, 0f, 0.5f, 0.5f });
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathF.PI * 2f * i / segments;
            float x = MathF.Cos(angle) * radius;
            float z = MathF.Sin(angle) * radius;
            vertices.AddRange(new[] { x, halfH, z, 0f, 1f, 0f, x / radius * 0.5f + 0.5f, z / radius * 0.5f + 0.5f });
        }
        for (int i = 0; i < segments; i++)
            indices.AddRange(new[] { topCenter, topCenter + (uint)i + 1, topCenter + (uint)i + 2 });

        // Bottom cap
        uint botCenter = (uint)(vertices.Count / 8);
        vertices.AddRange(new[] { 0f, -halfH, 0f, 0f, -1f, 0f, 0.5f, 0.5f });
        for (int i = 0; i <= segments; i++)
        {
            float angle = MathF.PI * 2f * i / segments;
            float x = MathF.Cos(angle) * radius;
            float z = MathF.Sin(angle) * radius;
            vertices.AddRange(new[] { x, -halfH, z, 0f, -1f, 0f, x / radius * 0.5f + 0.5f, z / radius * 0.5f + 0.5f });
        }
        for (int i = 0; i < segments; i++)
            indices.AddRange(new[] { botCenter, botCenter + (uint)i + 2, botCenter + (uint)i + 1 });

        return new Mesh(gl, vertices.ToArray(), indices.ToArray(), VertexLayout.PositionNormalTexture);
    }

    public static Mesh CreatePyramid(GL gl, float baseSize = 1.0f, float height = 1.0f)
    {
        float h = baseSize / 2f;
        float ny = baseSize / MathF.Sqrt(baseSize * baseSize + height * height);
        float nh = height / MathF.Sqrt(baseSize * baseSize + height * height);

        float[] vertices =
        {
            // Front face
             0.0f, height, 0.0f,  0f, nh, ny,  0.5f, 1.0f,
            -h,    0.0f,   h,     0f, nh, ny,  0.0f, 0.0f,
             h,    0.0f,   h,     0f, nh, ny,  1.0f, 0.0f,
            // Right face
             0.0f, height, 0.0f,  ny, nh, 0f,  0.5f, 1.0f,
             h,    0.0f,   h,     ny, nh, 0f,  0.0f, 0.0f,
             h,    0.0f,  -h,     ny, nh, 0f,  1.0f, 0.0f,
            // Back face
             0.0f, height, 0.0f,   0f, nh, -ny,  0.5f, 1.0f,
             h,    0.0f,  -h,      0f, nh, -ny,  0.0f, 0.0f,
            -h,    0.0f,  -h,      0f, nh, -ny,  1.0f, 0.0f,
            // Left face
             0.0f, height, 0.0f,  -ny, nh, 0f,  0.5f, 1.0f,
            -h,    0.0f,  -h,     -ny, nh, 0f,  0.0f, 0.0f,
            -h,    0.0f,   h,     -ny, nh, 0f,  1.0f, 0.0f,
            // Bottom face
            -h, 0.0f,  h,   0f, -1f, 0f,  0f, 0f,
             h, 0.0f,  h,   0f, -1f, 0f,  1f, 0f,
             h, 0.0f, -h,   0f, -1f, 0f,  1f, 1f,
            -h, 0.0f, -h,   0f, -1f, 0f,  0f, 1f,
        };
        uint[] indices =
        {
            0, 1, 2,     // front
            3, 4, 5,     // right
            6, 7, 8,     // back
            9, 10, 11,   // left
            12,13,14, 14,15,12  // bottom
        };
        return new Mesh(gl, vertices, indices, VertexLayout.PositionNormalTexture);
    }
}

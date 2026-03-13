using System.Numerics;

namespace Engine.Rendering;

public sealed class TextureAtlas
{
    public Texture2D Texture { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }
    public int Columns { get; }
    public int Rows { get; }

    public TextureAtlas(Texture2D texture, int cellWidth, int cellHeight)
    {
        Texture = texture;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Columns = texture.Width / cellWidth;
        Rows = texture.Height / cellHeight;
    }

    public (Vector2 offset, Vector2 scale) GetUV(int index)
    {
        int col = index % Columns;
        int row = index / Columns;

        float u = (float)col / Columns;
        float v = (float)row / Rows;
        float uScale = 1.0f / Columns;
        float vScale = 1.0f / Rows;

        return (new Vector2(u, v), new Vector2(uScale, vScale));
    }
}

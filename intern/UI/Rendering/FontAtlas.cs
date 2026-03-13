using Silk.NET.OpenGL;

namespace Engine.UI.Rendering;

public sealed class FontAtlas : IDisposable
{
    private readonly GL _gl;
    private readonly uint _textureId;

    public const int GlyphWidth = 6;
    public const int GlyphHeight = 10;
    public const int CharsPerRow = 16;
    public const int AtlasWidth = GlyphWidth * CharsPerRow;  // 96
    public const int AtlasHeight = GlyphHeight * 6;           // 60
    public const int FirstChar = 32;
    public const int LastChar = 126;

    public uint TextureId => _textureId;

    public FontAtlas(GL gl)
    {
        _gl = gl;
        _textureId = GenerateAtlas();
    }

    private unsafe uint GenerateAtlas()
    {
        byte[] pixels = new byte[AtlasWidth * AtlasHeight * 4]; // RGBA

        for (int c = FirstChar; c <= LastChar; c++)
        {
            int idx = c - FirstChar;
            int col = idx % CharsPerRow;
            int row = idx / CharsPerRow;
            int baseX = col * GlyphWidth;
            int baseY = row * GlyphHeight;

            ulong[] glyph = GetGlyph(c);

            for (int gy = 0; gy < GlyphHeight && gy < glyph.Length; gy++)
            {
                ulong bits = glyph[gy];
                for (int gx = 0; gx < GlyphWidth; gx++)
                {
                    bool on = ((bits >> (GlyphWidth - 1 - gx)) & 1) != 0;
                    int px = baseX + gx;
                    int py = baseY + gy;
                    int pi = (py * AtlasWidth + px) * 4;
                    byte v = on ? (byte)255 : (byte)0;
                    pixels[pi] = 255;
                    pixels[pi + 1] = 255;
                    pixels[pi + 2] = 255;
                    pixels[pi + 3] = v;
                }
            }
        }

        uint tex = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, tex);
        fixed (byte* p = pixels)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)AtlasWidth, (uint)AtlasHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        return tex;
    }

    public (float u, float v, float uSize, float vSize) GetCharUV(char c)
    {
        int idx = c - FirstChar;
        if (idx < 0 || idx > LastChar - FirstChar) idx = 0;
        int col = idx % CharsPerRow;
        int row = idx / CharsPerRow;
        float u = (float)(col * GlyphWidth) / AtlasWidth;
        float v = (float)(row * GlyphHeight) / AtlasHeight;
        float uSize = (float)GlyphWidth / AtlasWidth;
        float vSize = (float)GlyphHeight / AtlasHeight;
        return (u, v, uSize, vSize);
    }

    // 6x10 bitmap font glyphs (6 bits wide per row, 10 rows)
    private static ulong[] GetGlyph(int c) => c switch
    {
        ' ' => new ulong[] { 0,0,0,0,0,0,0,0,0,0 },
        '!' => new ulong[] { 0,0b001000,0b001000,0b001000,0b001000,0b001000,0,0b001000,0,0 },
        '"' => new ulong[] { 0,0b010100,0b010100,0,0,0,0,0,0,0 },
        '#' => new ulong[] { 0,0b010100,0b111110,0b010100,0b010100,0b111110,0b010100,0,0,0 },
        '$' => new ulong[] { 0b001000,0b011110,0b101000,0b011100,0b001010,0b111100,0b001000,0,0,0 },
        '%' => new ulong[] { 0,0b100010,0b000100,0b001000,0b010000,0b100010,0,0,0,0 },
        '&' => new ulong[] { 0,0b011000,0b100100,0b011000,0b100100,0b100010,0b011100,0,0,0 },
        '\'' => new ulong[] { 0,0b001000,0b001000,0,0,0,0,0,0,0 },
        '(' => new ulong[] { 0,0b000100,0b001000,0b001000,0b001000,0b001000,0b000100,0,0,0 },
        ')' => new ulong[] { 0,0b010000,0b001000,0b001000,0b001000,0b001000,0b010000,0,0,0 },
        '*' => new ulong[] { 0,0,0b010100,0b001000,0b010100,0,0,0,0,0 },
        '+' => new ulong[] { 0,0,0b001000,0b001000,0b111110,0b001000,0b001000,0,0,0 },
        ',' => new ulong[] { 0,0,0,0,0,0,0b001000,0b001000,0b010000,0 },
        '-' => new ulong[] { 0,0,0,0,0b111110,0,0,0,0,0 },
        '.' => new ulong[] { 0,0,0,0,0,0,0b001000,0,0,0 },
        '/' => new ulong[] { 0,0b000010,0b000100,0b001000,0b010000,0b100000,0,0,0,0 },
        '0' => new ulong[] { 0,0b011100,0b100010,0b100110,0b101010,0b110010,0b011100,0,0,0 },
        '1' => new ulong[] { 0,0b001000,0b011000,0b001000,0b001000,0b001000,0b011100,0,0,0 },
        '2' => new ulong[] { 0,0b011100,0b100010,0b000100,0b001000,0b010000,0b111110,0,0,0 },
        '3' => new ulong[] { 0,0b011100,0b100010,0b001100,0b000010,0b100010,0b011100,0,0,0 },
        '4' => new ulong[] { 0,0b000100,0b001100,0b010100,0b111110,0b000100,0b000100,0,0,0 },
        '5' => new ulong[] { 0,0b111110,0b100000,0b111100,0b000010,0b100010,0b011100,0,0,0 },
        '6' => new ulong[] { 0,0b011100,0b100000,0b111100,0b100010,0b100010,0b011100,0,0,0 },
        '7' => new ulong[] { 0,0b111110,0b000010,0b000100,0b001000,0b010000,0b010000,0,0,0 },
        '8' => new ulong[] { 0,0b011100,0b100010,0b011100,0b100010,0b100010,0b011100,0,0,0 },
        '9' => new ulong[] { 0,0b011100,0b100010,0b011110,0b000010,0b000100,0b011000,0,0,0 },
        ':' => new ulong[] { 0,0,0b001000,0,0,0b001000,0,0,0,0 },
        ';' => new ulong[] { 0,0,0b001000,0,0,0b001000,0b010000,0,0,0 },
        '<' => new ulong[] { 0,0b000100,0b001000,0b010000,0b001000,0b000100,0,0,0,0 },
        '=' => new ulong[] { 0,0,0b111110,0,0b111110,0,0,0,0,0 },
        '>' => new ulong[] { 0,0b010000,0b001000,0b000100,0b001000,0b010000,0,0,0,0 },
        '?' => new ulong[] { 0,0b011100,0b100010,0b000100,0b001000,0,0b001000,0,0,0 },
        '@' => new ulong[] { 0,0b011100,0b100010,0b101110,0b101010,0b101100,0b011100,0,0,0 },
        'A' => new ulong[] { 0,0b011100,0b100010,0b100010,0b111110,0b100010,0b100010,0,0,0 },
        'B' => new ulong[] { 0,0b111100,0b100010,0b111100,0b100010,0b100010,0b111100,0,0,0 },
        'C' => new ulong[] { 0,0b011100,0b100010,0b100000,0b100000,0b100010,0b011100,0,0,0 },
        'D' => new ulong[] { 0,0b111100,0b100010,0b100010,0b100010,0b100010,0b111100,0,0,0 },
        'E' => new ulong[] { 0,0b111110,0b100000,0b111100,0b100000,0b100000,0b111110,0,0,0 },
        'F' => new ulong[] { 0,0b111110,0b100000,0b111100,0b100000,0b100000,0b100000,0,0,0 },
        'G' => new ulong[] { 0,0b011100,0b100010,0b100000,0b100110,0b100010,0b011100,0,0,0 },
        'H' => new ulong[] { 0,0b100010,0b100010,0b111110,0b100010,0b100010,0b100010,0,0,0 },
        'I' => new ulong[] { 0,0b011100,0b001000,0b001000,0b001000,0b001000,0b011100,0,0,0 },
        'J' => new ulong[] { 0,0b000010,0b000010,0b000010,0b000010,0b100010,0b011100,0,0,0 },
        'K' => new ulong[] { 0,0b100010,0b100100,0b111000,0b100100,0b100010,0b100010,0,0,0 },
        'L' => new ulong[] { 0,0b100000,0b100000,0b100000,0b100000,0b100000,0b111110,0,0,0 },
        'M' => new ulong[] { 0,0b100010,0b110110,0b101010,0b100010,0b100010,0b100010,0,0,0 },
        'N' => new ulong[] { 0,0b100010,0b110010,0b101010,0b100110,0b100010,0b100010,0,0,0 },
        'O' => new ulong[] { 0,0b011100,0b100010,0b100010,0b100010,0b100010,0b011100,0,0,0 },
        'P' => new ulong[] { 0,0b111100,0b100010,0b111100,0b100000,0b100000,0b100000,0,0,0 },
        'Q' => new ulong[] { 0,0b011100,0b100010,0b100010,0b101010,0b100100,0b011010,0,0,0 },
        'R' => new ulong[] { 0,0b111100,0b100010,0b111100,0b100100,0b100010,0b100010,0,0,0 },
        'S' => new ulong[] { 0,0b011110,0b100000,0b011100,0b000010,0b000010,0b111100,0,0,0 },
        'T' => new ulong[] { 0,0b111110,0b001000,0b001000,0b001000,0b001000,0b001000,0,0,0 },
        'U' => new ulong[] { 0,0b100010,0b100010,0b100010,0b100010,0b100010,0b011100,0,0,0 },
        'V' => new ulong[] { 0,0b100010,0b100010,0b100010,0b010100,0b010100,0b001000,0,0,0 },
        'W' => new ulong[] { 0,0b100010,0b100010,0b101010,0b101010,0b110110,0b100010,0,0,0 },
        'X' => new ulong[] { 0,0b100010,0b010100,0b001000,0b010100,0b100010,0b100010,0,0,0 },
        'Y' => new ulong[] { 0,0b100010,0b010100,0b001000,0b001000,0b001000,0b001000,0,0,0 },
        'Z' => new ulong[] { 0,0b111110,0b000100,0b001000,0b010000,0b100000,0b111110,0,0,0 },
        '[' => new ulong[] { 0,0b011100,0b010000,0b010000,0b010000,0b010000,0b011100,0,0,0 },
        '\\' => new ulong[] { 0,0b100000,0b010000,0b001000,0b000100,0b000010,0,0,0,0 },
        ']' => new ulong[] { 0,0b011100,0b000100,0b000100,0b000100,0b000100,0b011100,0,0,0 },
        '^' => new ulong[] { 0,0b001000,0b010100,0,0,0,0,0,0,0 },
        '_' => new ulong[] { 0,0,0,0,0,0,0,0b111110,0,0 },
        '`' => new ulong[] { 0,0b010000,0b001000,0,0,0,0,0,0,0 },
        'a' => new ulong[] { 0,0,0b011100,0b000010,0b011110,0b100010,0b011110,0,0,0 },
        'b' => new ulong[] { 0,0b100000,0b100000,0b111100,0b100010,0b100010,0b111100,0,0,0 },
        'c' => new ulong[] { 0,0,0b011110,0b100000,0b100000,0b100000,0b011110,0,0,0 },
        'd' => new ulong[] { 0,0b000010,0b000010,0b011110,0b100010,0b100010,0b011110,0,0,0 },
        'e' => new ulong[] { 0,0,0b011100,0b100010,0b111110,0b100000,0b011100,0,0,0 },
        'f' => new ulong[] { 0,0b001100,0b010000,0b111100,0b010000,0b010000,0b010000,0,0,0 },
        'g' => new ulong[] { 0,0,0b011110,0b100010,0b011110,0b000010,0b011100,0,0,0 },
        'h' => new ulong[] { 0,0b100000,0b100000,0b111100,0b100010,0b100010,0b100010,0,0,0 },
        'i' => new ulong[] { 0,0b001000,0,0b011000,0b001000,0b001000,0b011100,0,0,0 },
        'j' => new ulong[] { 0,0b000100,0,0b000100,0b000100,0b100100,0b011000,0,0,0 },
        'k' => new ulong[] { 0,0b100000,0b100100,0b101000,0b110000,0b101000,0b100100,0,0,0 },
        'l' => new ulong[] { 0,0b011000,0b001000,0b001000,0b001000,0b001000,0b011100,0,0,0 },
        'm' => new ulong[] { 0,0,0b110100,0b101010,0b101010,0b101010,0b101010,0,0,0 },
        'n' => new ulong[] { 0,0,0b111100,0b100010,0b100010,0b100010,0b100010,0,0,0 },
        'o' => new ulong[] { 0,0,0b011100,0b100010,0b100010,0b100010,0b011100,0,0,0 },
        'p' => new ulong[] { 0,0,0b111100,0b100010,0b111100,0b100000,0b100000,0,0,0 },
        'q' => new ulong[] { 0,0,0b011110,0b100010,0b011110,0b000010,0b000010,0,0,0 },
        'r' => new ulong[] { 0,0,0b101100,0b110000,0b100000,0b100000,0b100000,0,0,0 },
        's' => new ulong[] { 0,0,0b011110,0b100000,0b011100,0b000010,0b111100,0,0,0 },
        't' => new ulong[] { 0,0b010000,0b111100,0b010000,0b010000,0b010000,0b001100,0,0,0 },
        'u' => new ulong[] { 0,0,0b100010,0b100010,0b100010,0b100010,0b011100,0,0,0 },
        'v' => new ulong[] { 0,0,0b100010,0b100010,0b010100,0b010100,0b001000,0,0,0 },
        'w' => new ulong[] { 0,0,0b100010,0b101010,0b101010,0b101010,0b010100,0,0,0 },
        'x' => new ulong[] { 0,0,0b100010,0b010100,0b001000,0b010100,0b100010,0,0,0 },
        'y' => new ulong[] { 0,0,0b100010,0b100010,0b011110,0b000010,0b011100,0,0,0 },
        'z' => new ulong[] { 0,0,0b111110,0b000100,0b001000,0b010000,0b111110,0,0,0 },
        '{' => new ulong[] { 0,0b000110,0b001000,0b110000,0b001000,0b001000,0b000110,0,0,0 },
        '|' => new ulong[] { 0,0b001000,0b001000,0b001000,0b001000,0b001000,0b001000,0,0,0 },
        '}' => new ulong[] { 0,0b110000,0b001000,0b000110,0b001000,0b001000,0b110000,0,0,0 },
        '~' => new ulong[] { 0,0,0,0b010100,0b101010,0,0,0,0,0 },
        _ => new ulong[] { 0,0b111110,0b100010,0b100010,0b100010,0b111110,0,0,0,0 },
    };

    public void Dispose()
    {
        _gl.DeleteTexture(_textureId);
    }
}

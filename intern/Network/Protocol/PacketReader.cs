using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Network.Protocol;

public ref struct PacketReader
{
    private readonly ReadOnlySpan<byte> _data;
    private int _position;

    public int Position => _position;
    public int Remaining => _data.Length - _position;

    public PacketReader(ReadOnlySpan<byte> data)
    {
        _data = data;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte() => _data[_position++];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(_data.Slice(_position));
        _position += 2;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(_data.Slice(_position));
        _position += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        var value = BinaryPrimitives.ReadSingleLittleEndian(_data.Slice(_position));
        _position += 4;
        return value;
    }

    public string ReadString()
    {
        short length = ReadInt16();
        var value = Encoding.UTF8.GetString(_data.Slice(_position, length));
        _position += length;
        return value;
    }

    public ReadOnlySpan<byte> ReadBytes()
    {
        int length = ReadInt32();
        var span = _data.Slice(_position, length);
        _position += length;
        return span;
    }
}

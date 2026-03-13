using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Memory;

public sealed class ArenaAllocator : IDisposable
{
    private readonly unsafe byte* _buffer;
    private readonly int _capacity;
    private int _offset;

    public unsafe ArenaAllocator(int capacityBytes)
    {
        _capacity = capacityBytes;
        _buffer = (byte*)NativeMemory.AllocZeroed((nuint)capacityBytes);
        _offset = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe Span<T> Allocate<T>(int count) where T : unmanaged
    {
        int size = count * sizeof(T);
        int alignedOffset = (_offset + 7) & ~7; // 8-byte alignment
        if (alignedOffset + size > _capacity)
            throw new OutOfMemoryException("Arena allocator out of space");

        var span = new Span<T>(_buffer + alignedOffset, count);
        _offset = alignedOffset + size;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _offset = 0;
    }

    public unsafe void Dispose()
    {
        NativeMemory.Free(_buffer);
    }
}

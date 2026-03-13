using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Memory;

public sealed class NativeBuffer<T> : IDisposable where T : unmanaged
{
    private unsafe T* _ptr;
    private int _count;

    public int Count => _count;

    public unsafe NativeBuffer(int initialCount)
    {
        _count = initialCount;
        _ptr = (T*)NativeMemory.AllocZeroed((nuint)(initialCount * sizeof(T)));
    }

    public unsafe ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _ptr[index];
    }

    public unsafe Span<T> AsSpan() => new(_ptr, _count);

    public unsafe T* GetPointer() => _ptr;

    public unsafe void Resize(int newCount)
    {
        _ptr = (T*)NativeMemory.Realloc(_ptr, (nuint)(newCount * sizeof(T)));
        _count = newCount;
    }

    public unsafe void Dispose()
    {
        if (_ptr != null)
        {
            NativeMemory.Free(_ptr);
            _ptr = null;
        }
    }
}

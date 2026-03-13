using System.Collections.Concurrent;

namespace Engine.Memory;

public sealed class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly int _maxSize;

    public ObjectPool(int maxSize = 256)
    {
        _maxSize = maxSize;
    }

    public T Rent()
    {
        return _pool.TryTake(out var item) ? item : new T();
    }

    public void Return(T item)
    {
        if (_pool.Count < _maxSize)
            _pool.Add(item);
    }
}

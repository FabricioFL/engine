using System.Runtime.CompilerServices;

namespace Engine.ECS;

public sealed class ComponentPool<T> where T : struct, IComponent
{
    private T[] _components;
    private int[] _entityToIndex;
    private int[] _indexToEntity;
    private int _count;

    public int Count => _count;

    public ComponentPool(int initialCapacity = 256)
    {
        _components = new T[initialCapacity];
        _entityToIndex = new int[initialCapacity];
        _indexToEntity = new int[initialCapacity];
        _count = 0;

        Array.Fill(_entityToIndex, -1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int entityId)
    {
        return entityId >= 0 && entityId < _entityToIndex.Length && _entityToIndex[entityId] != -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(int entityId)
    {
        return ref _components[_entityToIndex[entityId]];
    }

    public ref T Add(int entityId, in T component)
    {
        EnsureEntityCapacity(entityId);

        int index = _count;
        EnsureDenseCapacity(index);

        _components[index] = component;
        _entityToIndex[entityId] = index;
        _indexToEntity[index] = entityId;
        _count++;

        return ref _components[index];
    }

    public void Remove(int entityId)
    {
        if (!Has(entityId)) return;

        int indexToRemove = _entityToIndex[entityId];
        int lastIndex = _count - 1;

        if (indexToRemove < lastIndex)
        {
            _components[indexToRemove] = _components[lastIndex];
            int movedEntity = _indexToEntity[lastIndex];
            _indexToEntity[indexToRemove] = movedEntity;
            _entityToIndex[movedEntity] = indexToRemove;
        }

        _entityToIndex[entityId] = -1;
        _count--;
    }

    public Span<T> AsSpan() => _components.AsSpan(0, _count);

    public int GetEntityAtIndex(int denseIndex) => _indexToEntity[denseIndex];

    private void EnsureEntityCapacity(int entityId)
    {
        if (entityId < _entityToIndex.Length) return;

        int newSize = System.Math.Max(_entityToIndex.Length * 2, entityId + 1);
        var newSparse = new int[newSize];
        Array.Fill(newSparse, -1);
        Array.Copy(_entityToIndex, newSparse, _entityToIndex.Length);
        _entityToIndex = newSparse;
    }

    private void EnsureDenseCapacity(int index)
    {
        if (index < _components.Length) return;

        int newSize = _components.Length * 2;
        Array.Resize(ref _components, newSize);
        Array.Resize(ref _indexToEntity, newSize);
    }
}

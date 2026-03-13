namespace Engine.ECS;

public sealed class EntityManager
{
    private int _nextId;
    private int[] _generations;
    private readonly Queue<int> _freeIds = new();
    private readonly Dictionary<Type, object> _pools = new();
    private int _capacity;

    public EntityManager(int initialCapacity = 1024)
    {
        _capacity = initialCapacity;
        _generations = new int[initialCapacity];
        _nextId = 0;
    }

    public Entity CreateEntity()
    {
        int id;
        if (_freeIds.Count > 0)
        {
            id = _freeIds.Dequeue();
        }
        else
        {
            id = _nextId++;
            if (id >= _capacity)
            {
                _capacity *= 2;
                Array.Resize(ref _generations, _capacity);
            }
        }

        return new Entity(id, _generations[id]);
    }

    public void DestroyEntity(Entity entity)
    {
        if (!IsAlive(entity)) return;

        foreach (var pool in _pools.Values)
        {
            if (pool is IComponentPoolRemover remover)
                remover.Remove(entity.Id);
        }

        _generations[entity.Id]++;
        _freeIds.Enqueue(entity.Id);
    }

    public bool IsAlive(Entity entity)
    {
        return entity.Id >= 0 && entity.Id < _nextId && _generations[entity.Id] == entity.Generation;
    }

    public ref T AddComponent<T>(Entity entity, in T component = default) where T : struct, IComponent
    {
        return ref GetPool<T>().Add(entity.Id, in component);
    }

    public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
    {
        GetPool<T>().Remove(entity.Id);
    }

    public bool HasComponent<T>(Entity entity) where T : struct, IComponent
    {
        return GetPool<T>().Has(entity.Id);
    }

    public ref T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        return ref GetPool<T>().Get(entity.Id);
    }

    public ComponentPool<T> GetPool<T>() where T : struct, IComponent
    {
        var type = typeof(T);
        if (!_pools.TryGetValue(type, out var pool))
        {
            pool = new ComponentPool<T>();
            _pools[type] = pool;
        }
        return (ComponentPool<T>)pool;
    }

    private interface IComponentPoolRemover
    {
        void Remove(int entityId);
    }
}

// Make ComponentPool implement the remover interface via extension
// This is handled by the pool itself having Remove(int)

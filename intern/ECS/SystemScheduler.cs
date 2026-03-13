using Engine.Core;

namespace Engine.ECS;

public sealed class SystemScheduler
{
    private readonly List<SystemBase> _systems = new();
    private bool _sorted;

    public void Register(SystemBase system)
    {
        _systems.Add(system);
        _sorted = false;
    }

    public void Unregister(SystemBase system)
    {
        _systems.Remove(system);
    }

    public void RunAll(EntityManager entities, in GameTime time)
    {
        if (!_sorted)
        {
            _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _sorted = true;
        }

        for (int i = 0; i < _systems.Count; i++)
        {
            if (_systems[i].Enabled)
                _systems[i].Update(entities, in time);
        }
    }
}

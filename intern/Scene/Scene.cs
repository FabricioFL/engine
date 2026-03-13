using Engine.ECS;

namespace Engine.Scene;

public class Scene
{
    public string Name { get; }
    private readonly List<Entity> _entities = new();
    private readonly List<SystemBase> _systems = new();

    public IReadOnlyList<Entity> Entities => _entities;
    public IReadOnlyList<SystemBase> Systems => _systems;

    public Scene(string name)
    {
        Name = name;
    }

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    public void AddSystem(SystemBase system)
    {
        _systems.Add(system);
    }

    public virtual void OnLoad(EntityManager entityManager, IServiceProvider services) { }
    public virtual void OnUnload(EntityManager entityManager) { }
}

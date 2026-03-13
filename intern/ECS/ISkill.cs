namespace Engine.ECS;

public interface ISkill
{
    string Name { get; }
    float Cooldown { get; }
    void Execute(Entity entity, IServiceProvider services);
    void Cancel(Entity entity);
}

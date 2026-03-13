using Engine.Core;

namespace Engine.ECS;

public interface IScript
{
    void OnAttach(Entity entity, IServiceProvider services);
    void OnUpdate(Entity entity, in GameTime time);
    void OnDetach(Entity entity);
}

using Engine.Core;

namespace Engine.ECS;

public abstract class SystemBase
{
    public virtual int Priority => 0;
    public virtual bool Enabled { get; set; } = true;

    public abstract void Update(EntityManager entities, in GameTime time);
}

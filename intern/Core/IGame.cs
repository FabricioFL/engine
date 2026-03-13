namespace Engine.Core;

public interface IGame
{
    void Initialize(IServiceProvider services);
    void LoadContent();
    void Update(in GameTime time);
    void Shutdown();
}

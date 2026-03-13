using Engine.Config;
using Engine.ECS;
using Engine.Logging;

namespace Engine.Scene;

public sealed class SceneManager
{
    private readonly EntityManager _entityManager;
    private readonly SystemScheduler _systemScheduler;
    private readonly SceneConfig _config;
    private readonly ILogger _logger;
    private readonly Dictionary<string, Scene> _scenes = new();

    private Scene? _activeScene;
    private string? _pendingScene;

    public Scene? ActiveScene => _activeScene;

    public SceneManager(EntityManager entityManager, SystemScheduler systemScheduler,
        SceneConfig config, ILogger logger)
    {
        _entityManager = entityManager;
        _systemScheduler = systemScheduler;
        _config = config;
        _logger = logger;
    }

    public void Register(string name, Scene scene)
    {
        _scenes[name] = scene;
    }

    public void LoadScene(string name)
    {
        _pendingScene = name;
    }

    public void LoadDefaultScene()
    {
        if (!string.IsNullOrEmpty(_config.DefaultScene) && _scenes.ContainsKey(_config.DefaultScene))
        {
            TransitionTo(_config.DefaultScene);
        }
    }

    public void ProcessPendingTransitions()
    {
        if (_pendingScene == null) return;

        TransitionTo(_pendingScene);
        _pendingScene = null;
    }

    private void TransitionTo(string name)
    {
        if (!_scenes.TryGetValue(name, out var newScene))
        {
            _logger.Error("SceneManager", $"Scene '{name}' not found");
            return;
        }

        if (_activeScene != null)
        {
            _activeScene.OnUnload(_entityManager);
            foreach (var system in _activeScene.Systems)
                _systemScheduler.Unregister(system);
            _logger.Info("SceneManager", $"Unloaded scene '{_activeScene.Name}'");
        }

        _activeScene = newScene;
        foreach (var system in newScene.Systems)
            _systemScheduler.Register(system);
        newScene.OnLoad(_entityManager, null!);

        _logger.Info("SceneManager", $"Loaded scene '{name}'");
    }
}

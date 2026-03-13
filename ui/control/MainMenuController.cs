using Engine.UI.Runtime;

namespace Game.UI;

public class MainMenuController : UiControllerBase
{
    public Action? OnPlay { get; set; }
    public Action? OnOptions { get; set; }
    public Action? OnExit { get; set; }

    public override void OnLoad()
    {
        RegisterClick("btn-play", () => OnPlay?.Invoke());
        RegisterClick("btn-options", () => OnOptions?.Invoke());
        RegisterClick("btn-exit", () => OnExit?.Invoke());
    }
}

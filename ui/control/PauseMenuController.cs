using Engine.UI.Runtime;

namespace Game.UI;

public class PauseMenuController : UiControllerBase
{
    public Action? OnResume { get; set; }
    public Action? OnOptions { get; set; }
    public Action? OnBackToMenu { get; set; }
    public Action? OnExitGame { get; set; }

    public override void OnLoad()
    {
        RegisterClick("btn-resume", () => OnResume?.Invoke());
        RegisterClick("btn-pause-options", () => OnOptions?.Invoke());
        RegisterClick("btn-back-menu", () => OnBackToMenu?.Invoke());
        RegisterClick("btn-exit-game", () => OnExitGame?.Invoke());
    }
}

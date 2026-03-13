using Engine.Core;
using Engine.UI.Runtime;

namespace Game.UI;

public class DeathController : UiControllerBase
{
    public Action? OnRevive { get; set; }
    public Action? OnMainMenu { get; set; }

    public override void OnLoad()
    {
        RegisterClick("btn-revive", () => OnRevive?.Invoke());
        RegisterClick("btn-death-menu", () => OnMainMenu?.Invoke());
    }
}

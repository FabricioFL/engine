using System.Numerics;
using Engine.Core;
using Engine.UI.Markup;
using Engine.UI.Runtime;

namespace Game.UI;

public class HudController : UiControllerBase
{
    private float _displayedHealth = 1.0f;
    private float _targetHealth = 1.0f;
    private float _damageFlashAlpha = 0f;
    private float _thunderFlashAlpha = 0f;
    private UiNode? _healthFill;
    private UiNode? _damageFlash;

    public override void OnLoad()
    {
        _healthFill = FindById("health-bar-fill");
        _damageFlash = FindById("damage-flash");
    }

    public void SetHealth(float healthPercent)
    {
        _targetHealth = System.Math.Clamp(healthPercent, 0f, 1f);

        if (_targetHealth < _displayedHealth)
            _damageFlashAlpha = 0.3f;
    }

    public void SetThunderFlash(float alpha)
    {
        _thunderFlashAlpha = alpha;
    }

    public override void OnUpdate(in GameTime time)
    {
        // Smoothly animate health bar
        _displayedHealth = _displayedHealth + (_targetHealth - _displayedHealth) * 8f * time.DeltaTime;

        // Update health bar width
        if (_healthFill != null)
        {
            _healthFill.Style.Width = 200f * _displayedHealth;

            if (_displayedHealth > 0.5f)
                _healthFill.Style.BackgroundColor = new Vector4(0.13f, 0.8f, 0.27f, 0.93f);
            else if (_displayedHealth > 0.25f)
                _healthFill.Style.BackgroundColor = new Vector4(0.9f, 0.7f, 0.1f, 0.93f);
            else
                _healthFill.Style.BackgroundColor = new Vector4(0.9f, 0.15f, 0.1f, 0.93f);
        }

        // Combined flash effect (damage + thunder)
        float totalFlash = System.Math.Max(_damageFlashAlpha, _thunderFlashAlpha * 0.4f);

        if (_damageFlash != null && totalFlash > 0.001f)
        {
            _damageFlashAlpha = System.Math.Max(0f, _damageFlashAlpha - 2f * time.DeltaTime);

            _damageFlash.Style.Display = DisplayMode.Flex;

            // Red for damage, white for thunder
            if (_damageFlashAlpha > _thunderFlashAlpha * 0.4f)
                _damageFlash.Style.BackgroundColor = new Vector4(1f, 0f, 0f, _damageFlashAlpha);
            else
                _damageFlash.Style.BackgroundColor = new Vector4(1f, 1f, 1f, _thunderFlashAlpha * 0.4f);
        }
        else if (_damageFlash != null)
        {
            _damageFlash.Style.Display = DisplayMode.None;
        }
    }
}

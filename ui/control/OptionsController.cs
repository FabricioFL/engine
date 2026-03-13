using System.Numerics;
using Engine.Core;
using Engine.UI.Markup;
using Engine.UI.Runtime;
using Silk.NET.Windowing;

namespace Game.UI;

public class OptionsController : UiControllerBase
{
    public Action? OnBack { get; set; }
    public bool InvertMouse { get; private set; }

    private IWindow? _window;
    private int _screenModeIndex;
    private int _resolutionIndex;
    private float _volume = 0.7f;
    private UiNode? _volumeFill;

    private static readonly string[] ScreenModes = { "Windowed", "Fullscreen", "Borderless" };
    private static readonly (int w, int h)[] CommonResolutions =
    {
        (800, 600), (1024, 768), (1280, 720), (1280, 1024),
        (1366, 768), (1600, 900), (1920, 1080), (2560, 1440)
    };

    private (int w, int h)[] _validResolutions = CommonResolutions;

    public void SetWindow(IWindow window)
    {
        _window = window;
        // Filter to valid resolutions for this monitor
        if (window.Monitor != null)
        {
            var monitorRes = window.Monitor.VideoMode.Resolution;
            if (monitorRes.HasValue)
            {
                var maxW = monitorRes.Value.X;
                var maxH = monitorRes.Value.Y;
                _validResolutions = CommonResolutions
                    .Where(r => r.w <= maxW && r.h <= maxH)
                    .ToArray();
                if (_validResolutions.Length == 0)
                    _validResolutions = new[] { (maxW, maxH) };
            }
        }

        // Find current resolution index
        var curSize = window.Size;
        for (int i = 0; i < _validResolutions.Length; i++)
        {
            if (_validResolutions[i].w == curSize.X && _validResolutions[i].h == curSize.Y)
            {
                _resolutionIndex = i;
                break;
            }
        }
    }

    public override void OnLoad()
    {
        RegisterClick("btn-screen-mode", CycleScreenMode);
        RegisterClick("btn-resolution", CycleResolution);
        RegisterClick("btn-vol-down", () => SetVolume(_volume - 0.1f));
        RegisterClick("btn-vol-up", () => SetVolume(_volume + 0.1f));
        RegisterClick("btn-invert-mouse", ToggleInvertMouse);
        RegisterClick("btn-options-back", () => OnBack?.Invoke());

        _volumeFill = FindById("volume-bar-fill");
        UpdateLabels();
    }

    private void CycleScreenMode()
    {
        _screenModeIndex = (_screenModeIndex + 1) % ScreenModes.Length;
        ApplyScreenMode();
        UpdateLabels();
    }

    private void CycleResolution()
    {
        _resolutionIndex = (_resolutionIndex + 1) % _validResolutions.Length;
        ApplyResolution();
        UpdateLabels();
    }

    private void SetVolume(float vol)
    {
        _volume = System.Math.Clamp(vol, 0f, 1f);
        UpdateLabels();
    }

    private void ToggleInvertMouse()
    {
        InvertMouse = !InvertMouse;
        UpdateLabels();
    }

    private void ApplyScreenMode()
    {
        if (_window == null) return;
        switch (_screenModeIndex)
        {
            case 0: // Windowed
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Resizable;
                break;
            case 1: // Fullscreen
                _window.WindowState = WindowState.Fullscreen;
                break;
            case 2: // Borderless
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Hidden;
                // Maximize to fill screen
                if (_window.Monitor?.VideoMode.Resolution != null)
                {
                    var res = _window.Monitor.VideoMode.Resolution.Value;
                    _window.Size = new Silk.NET.Maths.Vector2D<int>(res.X, res.Y);
                    _window.Position = new Silk.NET.Maths.Vector2D<int>(0, 0);
                }
                break;
        }
    }

    private void ApplyResolution()
    {
        if (_window == null) return;
        var res = _validResolutions[_resolutionIndex];
        _window.Size = new Silk.NET.Maths.Vector2D<int>(res.w, res.h);
    }

    private void UpdateLabels()
    {
        SetText("btn-screen-mode", ScreenModes[_screenModeIndex]);
        if (_validResolutions.Length > 0)
        {
            var res = _validResolutions[_resolutionIndex];
            SetText("btn-resolution", $"{res.w}x{res.h}");
        }

        SetText("btn-invert-mouse", InvertMouse ? "On" : "Off");

        if (_volumeFill != null)
        {
            _volumeFill.Style.Width = 180f * _volume;
        }
    }
}

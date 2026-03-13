using System.Numerics;
using Silk.NET.Input;

namespace Engine.Input;

public sealed class InputManager
{
    private IInputContext? _context;
    private readonly HashSet<Key> _keysDown = new();
    private readonly HashSet<Key> _keysPressed = new();
    private readonly HashSet<Key> _keysReleased = new();
    private readonly HashSet<Key> _prevKeysDown = new();

    private readonly HashSet<MouseButton> _mouseDown = new();
    private readonly HashSet<MouseButton> _mousePressed = new();
    private readonly HashSet<MouseButton> _mouseReleased = new();
    private readonly HashSet<MouseButton> _prevMouseDown = new();

    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public float ScrollDelta { get; private set; }
    public bool IsCursorLocked { get; private set; }

    private Vector2 _prevMousePos;
    private bool _firstMouseUpdate = true;

    public void Initialize(IInputContext context)
    {
        _context = context;

        for (int i = 0; i < context.Keyboards.Count; i++)
        {
            var keyboard = context.Keyboards[i];
            keyboard.KeyDown += (kb, key, _) => _keysDown.Add(key);
            keyboard.KeyUp += (kb, key, _) => _keysDown.Remove(key);
        }

        for (int i = 0; i < context.Mice.Count; i++)
        {
            var mouse = context.Mice[i];
            mouse.MouseDown += (m, btn) => _mouseDown.Add(btn);
            mouse.MouseUp += (m, btn) => _mouseDown.Remove(btn);
            mouse.Scroll += (m, wheel) => ScrollDelta = wheel.Y;
            mouse.MouseMove += (m, pos) => MousePosition = new Vector2(pos.X, pos.Y);
        }
    }

    public void LockCursor()
    {
        if (_context == null) return;
        for (int i = 0; i < _context.Mice.Count; i++)
        {
            _context.Mice[i].Cursor.CursorMode = CursorMode.Raw;
        }
        IsCursorLocked = true;
        _firstMouseUpdate = true;
    }

    public void UnlockCursor()
    {
        if (_context == null) return;
        for (int i = 0; i < _context.Mice.Count; i++)
        {
            _context.Mice[i].Cursor.CursorMode = CursorMode.Normal;
        }
        IsCursorLocked = false;
    }

    public void Update()
    {
        // Keys: detect pressed/released this frame
        _keysPressed.Clear();
        _keysReleased.Clear();
        foreach (var key in _keysDown)
        {
            if (!_prevKeysDown.Contains(key))
                _keysPressed.Add(key);
        }
        foreach (var key in _prevKeysDown)
        {
            if (!_keysDown.Contains(key))
                _keysReleased.Add(key);
        }
        _prevKeysDown.Clear();
        foreach (var key in _keysDown)
            _prevKeysDown.Add(key);

        // Mouse buttons
        _mousePressed.Clear();
        _mouseReleased.Clear();
        foreach (var btn in _mouseDown)
        {
            if (!_prevMouseDown.Contains(btn))
                _mousePressed.Add(btn);
        }
        foreach (var btn in _prevMouseDown)
        {
            if (!_mouseDown.Contains(btn))
                _mouseReleased.Add(btn);
        }
        _prevMouseDown.Clear();
        foreach (var btn in _mouseDown)
            _prevMouseDown.Add(btn);

        // Mouse delta - skip first frame after lock to avoid huge jump
        if (_firstMouseUpdate)
        {
            MouseDelta = Vector2.Zero;
            _prevMousePos = MousePosition;
            _firstMouseUpdate = false;
        }
        else
        {
            MouseDelta = MousePosition - _prevMousePos;
            _prevMousePos = MousePosition;
        }

        ScrollDelta = 0; // Reset after frame
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);
    public bool IsKeyPressed(Key key) => _keysPressed.Contains(key);
    public bool IsKeyReleased(Key key) => _keysReleased.Contains(key);

    public bool IsMouseDown(MouseButton btn) => _mouseDown.Contains(btn);
    public bool IsMousePressed(MouseButton btn) => _mousePressed.Contains(btn);
    public bool IsMouseReleased(MouseButton btn) => _mouseReleased.Contains(btn);
}

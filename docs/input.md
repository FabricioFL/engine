# Input

The input system provides keyboard, mouse, and gamepad support via Silk.NET.

## InputManager

Access via DI:

```csharp
var input = services.GetRequiredService<InputManager>();
```

The engine calls `input.Update()` each frame automatically.

## Keyboard

```csharp
// Held down
if (input.IsKeyDown(Key.W)) { /* moving forward */ }

// Pressed this frame (not last frame)
if (input.IsKeyPressed(Key.Space)) { /* jump */ }

// Released this frame
if (input.IsKeyReleased(Key.E)) { /* interact */ }
```

Common keys: `Key.W`, `Key.A`, `Key.S`, `Key.D`, `Key.Space`, `Key.Escape`, `Key.ShiftLeft`, `Key.Enter`, `Key.Up`, `Key.Down`, `Key.Left`, `Key.Right`, `Key.E`, `Key.Q`, `Key.Tab`, `Key.F`.

## Mouse

### Position and Movement

```csharp
Vector2 pos = input.MousePosition;    // Screen coordinates
Vector2 delta = input.MouseDelta;     // Movement since last frame
float scroll = input.ScrollDelta;     // Scroll wheel
```

### Buttons

```csharp
if (input.IsMouseDown(MouseButton.Left)) { /* holding */ }
if (input.IsMousePressed(MouseButton.Right)) { /* clicked this frame */ }
if (input.IsMouseReleased(MouseButton.Middle)) { /* released this frame */ }
```

### Cursor Lock

Lock the cursor to the window center (for FPS/TPS camera):

```csharp
input.LockCursor();    // Hide and lock
input.UnlockCursor();  // Show and unlock

bool locked = input.IsCursorLocked;
```

When locked, `MouseDelta` gives relative movement while the cursor stays centered.

## Third-Person Camera Example

```csharp
public void Update(in GameTime time)
{
    if (_state == GameState.Playing)
    {
        var delta = _input.MouseDelta;

        _cameraYaw += delta.X * sensitivity;
        _cameraPitch -= delta.Y * sensitivity;
        _cameraPitch = Math.Clamp(_cameraPitch, -89f, 89f);

        // Position camera behind player
        float yawRad = _cameraYaw * MathF.PI / 180f;
        float pitchRad = _cameraPitch * MathF.PI / 180f;

        float camX = MathF.Sin(yawRad) * MathF.Cos(pitchRad) * distance;
        float camY = MathF.Sin(pitchRad) * distance;
        float camZ = MathF.Cos(yawRad) * MathF.Cos(pitchRad) * distance;

        camera.Position = playerPos - new Vector3(camX, -camY, camZ);
        camera.Yaw = _cameraYaw;
        camera.Pitch = _cameraPitch;
    }
}
```

## Movement with Diagonal Normalization

```csharp
float yawRad = _playerYaw * MathF.PI / 180f;
var forward = new Vector3(MathF.Sin(yawRad), 0, MathF.Cos(yawRad));
var right = new Vector3(forward.Z, 0, -forward.X);

var movement = Vector3.Zero;
if (input.IsKeyDown(Key.W)) movement += forward;
if (input.IsKeyDown(Key.S)) movement -= forward;
if (input.IsKeyDown(Key.A)) movement += right;
if (input.IsKeyDown(Key.D)) movement -= right;

if (movement.LengthSquared() > 0)
{
    movement = Vector3.Normalize(movement); // Prevents faster diagonal movement
    physics.SetBodyVelocity(bodyHandle, movement * speed + gravity);
}
```

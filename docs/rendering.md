# Rendering

The rendering system uses OpenGL 3.3 with a forward rendering pipeline. It handles meshes, sprites, materials, lighting, and post-processing.

## RenderPipeline

The central rendering coordinator. Access it via DI:

```csharp
var renderer = services.GetRequiredService<RenderPipeline>();
```

### Registering Meshes and Materials

Meshes and materials are registered with the pipeline and referenced by handle:

```csharp
int meshHandle = renderer.RegisterMesh(mesh);
int materialHandle = renderer.RegisterMaterial(material);

// Later retrieval
Mesh? mesh = renderer.GetMesh(meshHandle);
Material? mat = renderer.GetMaterial(materialHandle);
```

Assign handles to `RenderComponent.MeshHandle` and `RenderComponent.MaterialHandle` on entities.

### Custom Render Callback

Hook into the render loop for custom drawing:

```csharp
renderer.OnSceneRendered = () =>
{
    // Custom rendering after scene meshes but before post-processing
    rainRenderer.Render(camera);
};
```

## Camera

The camera controls the view and projection matrices:

```csharp
var camera = services.GetRequiredService<Camera>();

camera.Position = new Vector3(0, 10, -20);
camera.Yaw = 0f;
camera.Pitch = -15f;
camera.Fov = 60f;
camera.NearPlane = 0.1f;
camera.FarPlane = 500f;
```

### Properties

| Property | Description |
|---|---|
| Position | World position |
| Yaw, Pitch | Rotation in degrees |
| Fov | Field of view in degrees |
| NearPlane, FarPlane | Clipping planes |
| AspectRatio | Width / height (set automatically on resize) |
| Front, Up, Right | Computed direction vectors |
| ViewMatrix | Computed view matrix |
| ProjectionMatrix | Computed projection matrix |
| IsOrthographic | Switch to orthographic projection |
| OrthoSize | Orthographic half-size |

Matrices are recomputed each frame by the engine before rendering.

## Mesh

Represents a vertex array object with index data:

```csharp
mesh.Draw(); // Bind VAO and issue draw call
```

### MeshFactory

Create common shapes:

```csharp
Mesh quad = MeshFactory.CreateQuad(gl);
Mesh cube = MeshFactory.CreateCube(gl);
Mesh plane = MeshFactory.CreatePlane(gl, size: 50f);
Mesh cylinder = MeshFactory.CreateCylinder(gl, radius: 0.5f, height: 2f, segments: 16);
Mesh pyramid = MeshFactory.CreatePyramid(gl, baseSize: 1f, height: 2f);
Mesh screenQuad = MeshFactory.CreateScreenQuad(gl); // Full-screen post-process quad
```

## Material

A material binds a shader with rendering properties:

```csharp
var material = new Material(shader)
{
    Color = new Vector4(0.8f, 0.2f, 0.2f, 1f),
    DiffuseTexture = texture,
    Shininess = 32f
};
```

Call `material.Apply()` to bind the shader and set uniforms. The render pipeline does this automatically for entities with `RenderComponent`.

## Shader

GLSL shader program wrapper:

```csharp
var shader = assets.LoadShader("standard", "assets/shaders/standard.vert", "assets/shaders/standard.frag");

shader.Use();
shader.SetMatrix4("uModel", modelMatrix);
shader.SetVector3("uColor", new Vector3(1, 0, 0));
shader.SetFloat("uTime", (float)time.TotalTime);
shader.SetInt("uTexture", 0);
```

### Shader Library

The asset manager caches shaders:

```csharp
assets.LoadShader("standard", vertPath, fragPath); // Load once
Shader shader = assets.Shaders.Get("standard");    // Retrieve by name
```

## Texture2D

Load textures from image files:

```csharp
var texture = assets.LoadTexture("player", "assets/textures/player.png");

texture.Bind(TextureUnit.Texture0);
```

Properties: `Handle`, `Width`, `Height`.

## Lighting

The lighting system supports directional and point lights, managed by `LightManager`.

### Setup

Lights are entities with `TransformComponent` + `LightComponent`:

```csharp
// Sun
var sun = entities.CreateEntity();
entities.AddComponent(sun, TransformComponent.Default);
entities.AddComponent(sun, LightComponent.Directional(
    direction: new Vector3(-0.3f, -1f, -0.5f),
    color: new Vector3(0.1f, 0.1f, 0.2f),
    intensity: 0.3f
));

// Lamp
var lamp = entities.CreateEntity();
entities.AddComponent(lamp, TransformComponent.Default with
{
    Position = new Vector3(5, 3, 0)
});
entities.AddComponent(lamp, LightComponent.Point(
    color: new Vector3(1f, 0.7f, 0.3f),
    intensity: 3.0f,
    range: 20f
));
```

### Ambient Light

```csharp
renderer.LightManager.AmbientColor = new Vector3(0.05f, 0.05f, 0.08f);
```

### Limits

Maximum 16 lights. Light data is uploaded to a UBO each frame by `LightManager.UpdateLights()`.

## Post-Processing

Post-processing effects are applied after the scene is rendered to framebuffers.

### Initialization

```csharp
renderer.InitializePostProcessing(windowWidth, windowHeight);
```

The engine handles resizing automatically.

### Bloom

Extracts bright pixels, applies gaussian blur, and composites back:

```csharp
renderer.Bloom.Threshold = 0.8f;
renderer.Bloom.Intensity = 1.5f;
renderer.Bloom.BlurPasses = 5;
```

### Fog

Depth-based fog for atmosphere:

```csharp
renderer.Fog.Enabled = true;
renderer.Fog.FogColor = new Vector3(0.02f, 0.02f, 0.05f);
renderer.Fog.FogStart = 10f;
renderer.Fog.FogEnd = 80f;
renderer.Fog.FogDensity = 1.0f;
```

## Rain Renderer

Particle-based rain effect:

```csharp
var rain = new RainRenderer(gl, shader);
rain.Enabled = true;
rain.Intensity = 1.5f;

// In Update:
rain.PlayerPosition = playerPosition;
rain.Update(time.DeltaTime);

// In render callback:
renderer.OnSceneRendered = () => rain.Render(camera);
```

## Sprites

For 2D billboarded sprites in 3D space, use `SpriteComponent`:

```csharp
entities.AddComponent(entity, SpriteComponent.Default with
{
    TextureHandle = texHandle,
    SpriteSize = new Vector2(1, 2),
    Billboard = BillboardMode.Vertical
});
```

Billboard modes:
- `None` — no billboarding
- `Full` — always faces camera
- `Vertical` — rotates on Y axis to face camera

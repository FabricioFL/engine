using System.Numerics;
using BepuPhysics;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Engine.Assets;
using Engine.Core;
using Engine.ECS;
using Engine.ECS.Components;
using Engine.Input;
using Engine.Physics;
using Engine.Rendering;
using Engine.Rendering.Effects;
using Engine.UI.Runtime;
using Game.UI;

namespace Game;

public enum GameState { MainMenu, Playing, Paused, Options, Dead }

public class MyGame : IGame
{
    private IServiceProvider _services = null!;
    private EntityManager _entities = null!;
    private InputManager _input = null!;
    private RenderPipeline _renderPipeline = null!;
    private PhysicsWorld _physics = null!;
    private UiManager _uiManager = null!;
    private IWindow _window = null!;
    private GL _gl = null!;

    private Entity _player;
    private Entity _playerLight;
    private BodyHandle _playerBody;
    private float _moveSpeed = 5.0f;
    private float _sprintMultiplier = 1.8f;
    private float _jumpForce = 5.5f;
    private float _mouseSensitivity = 0.15f;
    private bool _isGrounded = true;

    // Third-person camera
    private float _cameraYaw = 0f;
    private float _cameraPitch = 15f;
    private float _cameraDistance = 8f;
    private float _cameraHeightOffset = 1.0f;
    private float _playerYaw = 0f;
    private float _cameraAutoCorrectSpeed = 2.5f;

    // Map size
    private const float MapHalfSize = 55f;

    // Scene data
    private readonly List<(Entity entity, Vector3 position)> _hazards = new();
    private readonly List<Entity> _lampLights = new();
    private readonly Random _rng = new();
    private bool _gameSceneBuilt;

    // Menu scene
    private float _menuCameraAngle;

    // Rain & thunder
    private RainRenderer? _rain;
    private float _thunderTimer;
    private float _thunderFlashAlpha;
    private float _baseAmbientLevel = 0.04f;

    // Game state
    private GameState _state = GameState.MainMenu;
    private GameState _prevOptionsState;

    // UI controllers
    private HudController _hudController = null!;
    private MainMenuController _mainMenuController = null!;
    private PauseMenuController _pauseMenuController = null!;
    private OptionsController _optionsController = null!;
    private DeathController _deathController = null!;

    // Shared mesh/material handles
    private int _cubeHandle;
    private int _cylinderHandle;
    private int _pyramidHandle;
    private int _planeHandle;
    private Engine.Rendering.Shader _shader = null!;
    private Engine.Rendering.Shader _rainShader = null!;
    private int _groundMatH;
    private int _playerMatH;
    private int _trunkMatH;
    private int _leafMatH;
    private int _houseMatH;
    private int _roofMatH;
    private int _windowMatH;
    private int _fenceMatH;
    private int _lampPoleMatH;
    private int _lampHeadMatH;
    private int _hazardMatH;
    private int _spikeMatH;

    public void Initialize(IServiceProvider services)
    {
        _services = services;
        _entities = services.GetRequiredService<EntityManager>();
        _input = services.GetRequiredService<InputManager>();
        _renderPipeline = services.GetRequiredService<RenderPipeline>();
        _physics = services.GetRequiredService<PhysicsWorld>();
        _uiManager = services.GetRequiredService<UiManager>();
        _window = services.GetRequiredService<IWindow>();
        _gl = services.GetRequiredService<GL>();
    }

    public void LoadContent()
    {
        _gl.ClearColor(0.02f, 0.02f, 0.06f, 1.0f);

        var assets = _services.GetRequiredService<AssetManager>();

        _shader = assets.LoadShader("standard",
            "assets/shaders/standard.vert", "assets/shaders/standard.frag");
        _rainShader = assets.LoadShader("rain",
            "assets/shaders/rain.vert", "assets/shaders/rain.frag");

        // Materials
        var groundMat = new Material(_shader) { Color = new Vector4(0.12f, 0.18f, 0.08f, 1.0f) };
        var playerMat = new Material(_shader) { Color = new Vector4(0.2f, 0.5f, 0.9f, 1.0f) };
        var treeTrunkMat = new Material(_shader) { Color = new Vector4(0.35f, 0.22f, 0.1f, 1.0f) };
        var treeLeafMat = new Material(_shader) { Color = new Vector4(0.08f, 0.28f, 0.06f, 1.0f) };
        var houseMat = new Material(_shader) { Color = new Vector4(0.45f, 0.35f, 0.25f, 1.0f) };
        var roofMat = new Material(_shader) { Color = new Vector4(0.55f, 0.2f, 0.15f, 1.0f) };
        var windowMat = new Material(_shader) { Color = new Vector4(0.9f, 0.8f, 0.4f, 0.3f) };
        var fenceMat = new Material(_shader) { Color = new Vector4(0.4f, 0.3f, 0.18f, 1.0f) };
        var lampPoleMat = new Material(_shader) { Color = new Vector4(0.3f, 0.3f, 0.3f, 1.0f) };
        var lampHeadMat = new Material(_shader) { Color = new Vector4(1.0f, 0.95f, 0.7f, 1.0f) };
        var hazardMat = new Material(_shader) { Color = new Vector4(0.85f, 0.15f, 0.15f, 1.0f) };
        var hazardSpikeMat = new Material(_shader) { Color = new Vector4(0.6f, 0.1f, 0.1f, 1.0f) };

        // Meshes
        var cubeMesh = MeshFactory.CreateCube(_gl);
        var cylinderMesh = MeshFactory.CreateCylinder(_gl, 0.5f, 1.0f, 12);
        var pyramidMesh = MeshFactory.CreatePyramid(_gl, 1.0f, 1.0f);
        var planeMesh = MeshFactory.CreatePlane(_gl, 120f);

        _cubeHandle = _renderPipeline.RegisterMesh(cubeMesh);
        _cylinderHandle = _renderPipeline.RegisterMesh(cylinderMesh);
        _pyramidHandle = _renderPipeline.RegisterMesh(pyramidMesh);
        _planeHandle = _renderPipeline.RegisterMesh(planeMesh);

        _groundMatH = _renderPipeline.RegisterMaterial(groundMat);
        _playerMatH = _renderPipeline.RegisterMaterial(playerMat);
        _trunkMatH = _renderPipeline.RegisterMaterial(treeTrunkMat);
        _leafMatH = _renderPipeline.RegisterMaterial(treeLeafMat);
        _houseMatH = _renderPipeline.RegisterMaterial(houseMat);
        _roofMatH = _renderPipeline.RegisterMaterial(roofMat);
        _windowMatH = _renderPipeline.RegisterMaterial(windowMat);
        _fenceMatH = _renderPipeline.RegisterMaterial(fenceMat);
        _lampPoleMatH = _renderPipeline.RegisterMaterial(lampPoleMat);
        _lampHeadMatH = _renderPipeline.RegisterMaterial(lampHeadMat);
        _hazardMatH = _renderPipeline.RegisterMaterial(hazardMat);
        _spikeMatH = _renderPipeline.RegisterMaterial(hazardSpikeMat);

        // Lighting (shared across both scenes)
        _renderPipeline.LightManager!.AmbientColor = new Vector3(_baseAmbientLevel);

        // Moonlight
        var moonlight = _entities.CreateEntity();
        _entities.AddComponent(moonlight, TransformComponent.Default);
        _entities.AddComponent(moonlight, LightComponent.Directional(
            new Vector3(0.3f, -1f, -0.5f), new Vector3(0.2f, 0.2f, 0.35f), 0.2f));

        // Post-processing
        var bloom = _renderPipeline.Bloom;
        if (bloom != null) { bloom.Threshold = 0.6f; bloom.Intensity = 1.5f; }
        var fog = _renderPipeline.Fog;
        if (fog != null)
        {
            fog.Enabled = true;
            fog.FogColor = new Vector3(0.03f, 0.03f, 0.06f);
            fog.FogStart = 15f;
            fog.FogEnd = 60f;
            fog.FogDensity = 0.015f;
        }

        // Build menu scene (ground + trees + lamps for atmosphere)
        BuildMenuScene();

        // Rain (visible in both scenes)
        _rain = new RainRenderer(_gl, _rainShader);
        _thunderTimer = 8f + (float)_rng.NextDouble() * 12f;
        _renderPipeline.OnSceneRendered = () => _rain?.Render(_renderPipeline.Camera);

        // UI controllers
        _hudController = new HudController();

        _mainMenuController = new MainMenuController();
        _mainMenuController.OnPlay = () =>
        {
            if (!_gameSceneBuilt) BuildGameScene();
            ChangeState(GameState.Playing);
        };
        _mainMenuController.OnOptions = () => { _prevOptionsState = GameState.MainMenu; ChangeState(GameState.Options); };
        _mainMenuController.OnExit = () => _window.Close();

        _pauseMenuController = new PauseMenuController();
        _pauseMenuController.OnResume = () => ChangeState(GameState.Playing);
        _pauseMenuController.OnOptions = () => { _prevOptionsState = GameState.Paused; ChangeState(GameState.Options); };
        _pauseMenuController.OnBackToMenu = () => ChangeState(GameState.MainMenu);
        _pauseMenuController.OnExitGame = () => _window.Close();

        _optionsController = new OptionsController();
        _optionsController.SetWindow(_window);
        _optionsController.OnBack = () => ChangeState(_prevOptionsState);

        _deathController = new DeathController();
        _deathController.OnRevive = () =>
        {
            // Reset player health and position
            ref var health = ref _entities.GetComponent<HealthComponent>(_player);
            health = HealthComponent.Create(100f, 0.4f);
            _physics.Simulation.Bodies[_playerBody].Pose.Position = new Vector3(0, 2f, 0);
            _physics.Simulation.Bodies[_playerBody].Velocity.Linear = Vector3.Zero;
            _physics.Simulation.Awakener.AwakenBody(_playerBody);
            ChangeState(GameState.Playing);
        };
        _deathController.OnMainMenu = () => ChangeState(GameState.MainMenu);

        ChangeState(GameState.MainMenu);
    }

    private void BuildMenuScene()
    {
        // Ground
        var ground = _entities.CreateEntity();
        _entities.AddComponent(ground, TransformComponent.Default);
        _entities.AddComponent(ground, new RenderComponent
        {
            MeshHandle = _planeHandle, MaterialHandle = _groundMatH,
            Visible = true, CastShadow = false
        });
        var groundShape = _physics.CreateBox(120f, 0.2f, 120f);
        _physics.AddStaticBody(new Vector3(0, -0.1f, 0), Quaternion.Identity, groundShape);

        // Scatter some trees for menu atmosphere
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)_rng.NextDouble() * MathF.PI * 2f;
            float dist = 8f + (float)_rng.NextDouble() * 20f;
            PlaceTree(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist, _trunkMatH, _leafMatH);
        }

        // A few lamp posts for visual interest
        Vector3[] menuLamps = { new(6, 0, 0), new(-6, 0, 0), new(0, 0, 6), new(0, 0, -6) };
        foreach (var pos in menuLamps)
            BuildSingleLampPost(pos, _lampPoleMatH, _lampHeadMatH);

        // Menu camera start
        var camera = _renderPipeline.Camera;
        camera.Position = new Vector3(15f, 6f, 0);
        camera.Pitch = -15f;
        camera.Yaw = 180f;
    }

    private void BuildGameScene()
    {
        _gameSceneBuilt = true;

        // Player
        _player = _entities.CreateEntity();
        _entities.AddComponent(_player, new TransformComponent
        {
            Position = new Vector3(0, 1.0f, 0),
            Rotation = Quaternion.Identity,
            Scale = new Vector3(0.8f, 1.6f, 0.8f),
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(_player, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = _playerMatH,
            Visible = true, CastShadow = true
        });
        _entities.AddComponent(_player, HealthComponent.Create(100f, 0.4f));

        var playerShape = _physics.CreateBox(0.8f, 1.6f, 0.8f);
        _playerBody = _physics.AddDynamicBody(new Vector3(0, 1.0f, 0), Quaternion.Identity, 70f, playerShape);
        _entities.AddComponent(_player, new RigidbodyComponent
        {
            BodyHandle = _playerBody.Value,
            Mass = 70f,
            IsKinematic = false,
            UseGravity = true
        });
        _physics.Simulation.Bodies[_playerBody].LocalInertia = new BepuPhysics.BodyInertia
        {
            InverseMass = 1f / 70f
        };

        // Player light
        _playerLight = _entities.CreateEntity();
        _entities.AddComponent(_playerLight, new TransformComponent
        {
            Position = new Vector3(0, 2.0f, 0),
            Rotation = Quaternion.Identity,
            Scale = Vector3.One,
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(_playerLight, LightComponent.Point(
            new Vector3(0.8f, 0.7f, 0.5f), 1.2f, 12f));

        // More trees (including dense borders)
        BuildTrees(_trunkMatH, _leafMatH);
        BuildHouses(_houseMatH, _roofMatH, _windowMatH, _fenceMatH);
        BuildLampPosts(_lampPoleMatH, _lampHeadMatH);
        BuildHazards(_hazardMatH, _spikeMatH);
        BuildInvisibleWalls();

        // HUD
        _uiManager.Mount("hud.html", _hudController);
    }

    private void ChangeState(GameState newState)
    {
        // Unmount previous state UI
        switch (_state)
        {
            case GameState.MainMenu:
                if (_uiManager.IsMounted("main-menu.html"))
                    _uiManager.Unmount("main-menu.html");
                break;
            case GameState.Paused:
                if (_uiManager.IsMounted("pause-menu.html"))
                    _uiManager.Unmount("pause-menu.html");
                break;
            case GameState.Options:
                if (_uiManager.IsMounted("options.html"))
                    _uiManager.Unmount("options.html");
                break;
            case GameState.Dead:
                if (_uiManager.IsMounted("death.html"))
                    _uiManager.Unmount("death.html");
                break;
        }

        _state = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                _uiManager.Mount("main-menu.html", _mainMenuController);
                _input.UnlockCursor();
                if (_gameSceneBuilt)
                    SetPlayerVisible(false);
                break;
            case GameState.Playing:
                _input.LockCursor();
                if (_gameSceneBuilt)
                    SetPlayerVisible(true);
                break;
            case GameState.Paused:
                _uiManager.Mount("pause-menu.html", _pauseMenuController);
                _input.UnlockCursor();
                break;
            case GameState.Options:
                _uiManager.Mount("options.html", _optionsController);
                _input.UnlockCursor();
                break;
            case GameState.Dead:
                _uiManager.Mount("death.html", _deathController);
                _input.UnlockCursor();
                break;
        }
    }

    private void SetPlayerVisible(bool visible)
    {
        ref var render = ref _entities.GetComponent<RenderComponent>(_player);
        render.Visible = visible;
    }

    public void Update(in GameTime time)
    {
        // Handle click for UI
        if (_state != GameState.Playing && _input.IsMousePressed(MouseButton.Left))
            _uiManager.HandleClick(_input.MousePosition);

        switch (_state)
        {
            case GameState.MainMenu:
                UpdateMainMenu(time);
                break;
            case GameState.Playing:
                UpdatePlaying(time);
                break;
            case GameState.Paused:
                if (_input.IsKeyPressed(Key.Escape))
                    ChangeState(GameState.Playing);
                break;
            case GameState.Options:
                if (_input.IsKeyPressed(Key.Escape))
                    ChangeState(_prevOptionsState);
                break;
        }

        UpdateRain(time);

        if (_state == GameState.Playing && _gameSceneBuilt)
        {
            ref var health = ref _entities.GetComponent<HealthComponent>(_player);
            _hudController.SetHealth(health.HealthPercent);

            if (!health.IsAlive)
                ChangeState(GameState.Dead);
        }
    }

    private void UpdateMainMenu(in GameTime time)
    {
        _menuCameraAngle += 3f * time.DeltaTime;
        var camera = _renderPipeline.Camera;
        camera.Position = new Vector3(
            MathF.Cos(_menuCameraAngle * MathF.PI / 180f) * 15f,
            6f,
            MathF.Sin(_menuCameraAngle * MathF.PI / 180f) * 15f);
        // Look at origin
        var lookDir = Vector3.Normalize(-camera.Position);
        camera.Yaw = MathF.Atan2(lookDir.Z, lookDir.X) * 180f / MathF.PI;
        camera.Pitch = MathF.Asin(lookDir.Y / camera.Position.Length()) * 180f / MathF.PI;
    }

    private void UpdatePlaying(in GameTime time)
    {
        if (_input.IsKeyPressed(Key.Escape))
        {
            ChangeState(GameState.Paused);
            return;
        }

        ref var transform = ref _entities.GetComponent<TransformComponent>(_player);
        ref var health = ref _entities.GetComponent<HealthComponent>(_player);

        // ---- Mouse controls camera ----
        var mouseDelta = _input.MouseDelta;
        float yMul = _optionsController.InvertMouse ? -1f : 1f;
        _cameraYaw -= mouseDelta.X * _mouseSensitivity;
        _cameraPitch -= mouseDelta.Y * _mouseSensitivity * yMul;
        _cameraPitch = System.Math.Clamp(_cameraPitch, 2f, 50f);

        // ---- Camera controls player direction ----
        // Player always faces the camera's forward direction
        _playerYaw = _cameraYaw;

        // ---- WASD relative to player direction ----
        float playerYawRad = _playerYaw * MathF.PI / 180f;
        var forward = new Vector3(MathF.Sin(playerYawRad), 0, MathF.Cos(playerYawRad));
        var right = new Vector3(forward.Z, 0, -forward.X);

        var movement = Vector3.Zero;
        if (_input.IsKeyDown(Key.W) || _input.IsKeyDown(Key.Up)) movement += forward;
        if (_input.IsKeyDown(Key.S) || _input.IsKeyDown(Key.Down)) movement -= forward;
        if (_input.IsKeyDown(Key.A) || _input.IsKeyDown(Key.Left)) movement += right;
        if (_input.IsKeyDown(Key.D) || _input.IsKeyDown(Key.Right)) movement -= right;

        bool isMoving = movement.LengthSquared() > 0;

        if (isMoving && health.IsAlive)
        {
            // Diagonal normalization
            movement = Vector3.Normalize(movement);

            float speed = _moveSpeed;
            if (_input.IsKeyDown(Key.ShiftLeft))
                speed *= _sprintMultiplier;

            var velocity = _physics.Simulation.Bodies[_playerBody].Velocity.Linear;
            velocity.X = movement.X * speed;
            velocity.Z = movement.Z * speed;
            _physics.SetBodyVelocity(_playerBody, velocity);
        }
        else
        {
            var velocity = _physics.Simulation.Bodies[_playerBody].Velocity.Linear;
            velocity.X *= 0.85f;
            velocity.Z *= 0.85f;
            _physics.SetBodyVelocity(_playerBody, velocity);
        }

        // Apply player rotation
        transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _playerYaw * MathF.PI / 180f);
        transform.IsDirty = true;

        // ---- Camera auto-correct: when moving and mouse idle, drift camera behind player ----
        bool mouseIdle = MathF.Abs(mouseDelta.X) < 0.5f && MathF.Abs(mouseDelta.Y) < 0.5f;
        if (isMoving && mouseIdle)
        {
            // Smoothly bring camera yaw toward player yaw (they're equal now,
            // but this corrects after the user stops moving the mouse mid-turn)
            float diff = _playerYaw - _cameraYaw;
            while (diff > 180f) diff -= 360f;
            while (diff < -180f) diff += 360f;
            _cameraYaw += diff * _cameraAutoCorrectSpeed * time.DeltaTime;
        }

        // Jump
        var currentVel = _physics.Simulation.Bodies[_playerBody].Velocity.Linear;
        _isGrounded = transform.Position.Y <= 1.1f && MathF.Abs(currentVel.Y) < 0.5f;

        if (_input.IsKeyPressed(Key.Space) && _isGrounded && health.IsAlive)
            _physics.ApplyImpulse(_playerBody, new Vector3(0, _jumpForce * 70f, 0));

        // Damage cooldown
        health.UpdateCooldown(time.DeltaTime);

        // Hazard collision
        var playerPos = transform.Position;
        foreach (var (hazardEntity, hazardPos) in _hazards)
        {
            ref var hazard = ref _entities.GetComponent<HazardComponent>(hazardEntity);
            float dist = Vector3.Distance(
                new Vector3(playerPos.X, 0, playerPos.Z),
                new Vector3(hazardPos.X, 0, hazardPos.Z));
            if (dist < hazard.Range)
                health.TakeDamage(hazard.Damage);
        }

        // ---- Third-person camera: behind and above player ----
        var camera = _renderPipeline.Camera;
        float pitchRad = _cameraPitch * MathF.PI / 180f;
        float yawRad = _cameraYaw * MathF.PI / 180f;

        float horizDist = _cameraDistance * MathF.Cos(pitchRad);
        float vertDist = _cameraDistance * MathF.Sin(pitchRad);

        var lookTarget = transform.Position + new Vector3(0, _cameraHeightOffset, 0);
        camera.Position = lookTarget + new Vector3(
            -MathF.Sin(yawRad) * horizDist,
            vertDist,
            -MathF.Cos(yawRad) * horizDist);

        var lookDir = Vector3.Normalize(lookTarget - camera.Position);
        camera.Yaw = MathF.Atan2(lookDir.Z, lookDir.X) * 180f / MathF.PI;
        camera.Pitch = MathF.Asin(lookDir.Y) * 180f / MathF.PI;

        // Sync player light
        ref var lightTransform = ref _entities.GetComponent<TransformComponent>(_playerLight);
        lightTransform.Position = transform.Position + new Vector3(0, 2.0f, 0);
        lightTransform.IsDirty = true;

        UpdateThunder(time);
    }

    private void UpdateRain(in GameTime time)
    {
        if (_rain == null) return;

        if (_gameSceneBuilt && _state == GameState.Playing)
        {
            ref var transform = ref _entities.GetComponent<TransformComponent>(_player);
            _rain.PlayerPosition = transform.Position;
        }
        else
        {
            _rain.PlayerPosition = Vector3.Zero;
        }
        _rain.Update(time.DeltaTime);
    }

    private void UpdateThunder(in GameTime time)
    {
        _thunderTimer -= time.DeltaTime;

        if (_thunderTimer <= 0)
        {
            _thunderFlashAlpha = 1.0f;
            _thunderTimer = 8f + (float)_rng.NextDouble() * 15f;
        }

        if (_thunderFlashAlpha > 0)
        {
            _thunderFlashAlpha -= 3f * time.DeltaTime;
            if (_thunderFlashAlpha < 0) _thunderFlashAlpha = 0;

            float flashAmbient = _baseAmbientLevel + _thunderFlashAlpha * 0.5f;
            _renderPipeline.LightManager!.AmbientColor = new Vector3(flashAmbient);
        }
        else
        {
            _renderPipeline.LightManager!.AmbientColor = new Vector3(_baseAmbientLevel);
        }

        _hudController.SetThunderFlash(_thunderFlashAlpha);
    }

    // ======= Scene Building =======

    private void BuildTrees(int trunkMatH, int leafMatH)
    {
        Vector2[] housePositions = { new(-12, -8), new(12, -10), new(-5, 14) };

        // Interior trees
        for (int i = 0; i < 60; i++)
        {
            float x = ((float)_rng.NextDouble() - 0.5f) * 80f;
            float z = ((float)_rng.NextDouble() - 0.5f) * 80f;

            bool tooClose = false;
            foreach (var hp in housePositions)
            {
                if (Vector2.Distance(new Vector2(x, z), hp) < 8f)
                    tooClose = true;
            }
            if (MathF.Abs(x) < 4f && MathF.Abs(z) < 4f) tooClose = true;
            if (tooClose) continue;

            PlaceTree(x, z, trunkMatH, leafMatH);
        }

        // Dense border trees
        float borderInner = MapHalfSize - 12f;
        float borderOuter = MapHalfSize + 2f;
        for (int i = 0; i < 200; i++)
        {
            float angle = (float)_rng.NextDouble() * MathF.PI * 2f;
            float dist = borderInner + (float)_rng.NextDouble() * (borderOuter - borderInner);
            float x = MathF.Cos(angle) * dist;
            float z = MathF.Sin(angle) * dist;

            x = System.Math.Clamp(x, -borderOuter, borderOuter);
            z = System.Math.Clamp(z, -borderOuter, borderOuter);

            if (MathF.Abs(x) < borderInner && MathF.Abs(z) < borderInner) continue;

            PlaceTree(x, z, trunkMatH, leafMatH);
        }
    }

    private void PlaceTree(float x, float z, int trunkMatH, int leafMatH)
    {
        float trunkHeight = 2f + (float)_rng.NextDouble() * 2.5f;
        float treeScale = 0.8f + (float)_rng.NextDouble() * 0.5f;

        var trunk = _entities.CreateEntity();
        _entities.AddComponent(trunk, new TransformComponent
        {
            Position = new Vector3(x, trunkHeight / 2f, z),
            Rotation = Quaternion.Identity,
            Scale = new Vector3(0.2f * treeScale, trunkHeight, 0.2f * treeScale),
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(trunk, new RenderComponent
        {
            MeshHandle = _cylinderHandle, MaterialHandle = trunkMatH,
            Visible = true, CastShadow = true
        });

        var trunkShape = _physics.CreateBox(0.4f * treeScale, trunkHeight, 0.4f * treeScale);
        _physics.AddStaticBody(new Vector3(x, trunkHeight / 2f, z), Quaternion.Identity, trunkShape);

        int layers = 2 + _rng.Next(2);
        for (int j = 0; j < layers; j++)
        {
            float layerY = trunkHeight + j * 1.2f * treeScale;
            float layerScale = (layers - j) * 0.7f * treeScale;

            var foliage = _entities.CreateEntity();
            _entities.AddComponent(foliage, new TransformComponent
            {
                Position = new Vector3(x, layerY, z),
                Rotation = Quaternion.Identity,
                Scale = new Vector3(layerScale, 1.5f * treeScale, layerScale),
                LocalToWorld = Matrix4x4.Identity,
                IsDirty = true
            });
            _entities.AddComponent(foliage, new RenderComponent
            {
                MeshHandle = _pyramidHandle, MaterialHandle = leafMatH,
                Visible = true, CastShadow = true
            });
        }
    }

    private void BuildHouses(int houseMatH, int roofMatH, int windowMatH, int fenceMatH)
    {
        BuildHouse(new Vector3(-12, 0, -8), 0f, houseMatH, roofMatH, windowMatH, fenceMatH);
        BuildHouse(new Vector3(12, 0, -10), 90f, houseMatH, roofMatH, windowMatH, fenceMatH);
        BuildHouse(new Vector3(-5, 0, 14), -45f, houseMatH, roofMatH, windowMatH, fenceMatH);
    }

    private void BuildHouse(Vector3 pos, float rotDeg, int wallMatH, int roofMatH, int windowMatH, int fenceMatH)
    {
        float w = 5f, h = 3f, d = 4f;
        var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotDeg * MathF.PI / 180f);

        Vector3 Local(Vector3 local) => Vector3.Transform(local, rot) + pos;
        Quaternion LocalRot(Quaternion localRot) => localRot * rot;

        float wallThickness = 0.15f;

        CreateWall(Local(new Vector3(0, h / 2f, -d / 2f)), LocalRot(Quaternion.Identity),
            new Vector3(w, h, wallThickness), wallMatH);
        CreateWall(Local(new Vector3(-w / 2f, h / 2f, 0)), LocalRot(Quaternion.Identity),
            new Vector3(wallThickness, h, d), wallMatH);
        CreateWall(Local(new Vector3(w / 2f, h / 2f, 0)), LocalRot(Quaternion.Identity),
            new Vector3(wallThickness, h, d), wallMatH);
        CreateWall(Local(new Vector3(-w / 2f + 0.75f, h / 2f, d / 2f)), LocalRot(Quaternion.Identity),
            new Vector3(1.5f, h, wallThickness), wallMatH);
        CreateWall(Local(new Vector3(w / 2f - 0.75f, h / 2f, d / 2f)), LocalRot(Quaternion.Identity),
            new Vector3(1.5f, h, wallThickness), wallMatH);
        CreateWall(Local(new Vector3(0, h - 0.4f, d / 2f)), LocalRot(Quaternion.Identity),
            new Vector3(w - 3f, 0.8f, wallThickness), wallMatH);

        var roof = _entities.CreateEntity();
        _entities.AddComponent(roof, new TransformComponent
        {
            Position = Local(new Vector3(0, h + 0.15f, 0)),
            Rotation = LocalRot(Quaternion.Identity),
            Scale = new Vector3(w + 0.6f, 0.3f, d + 0.6f),
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(roof, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = roofMatH,
            Visible = true, CastShadow = true
        });

        CreateWall(Local(new Vector3(0, 0.05f, 0)), LocalRot(Quaternion.Identity),
            new Vector3(w, 0.1f, d), wallMatH);

        CreateWindow(Local(new Vector3(-w / 2f - 0.01f, h / 2f + 0.3f, 0)), LocalRot(Quaternion.Identity),
            new Vector3(0.05f, 1.0f, 1.2f), windowMatH);
        CreateWindow(Local(new Vector3(w / 2f + 0.01f, h / 2f + 0.3f, 0)), LocalRot(Quaternion.Identity),
            new Vector3(0.05f, 1.0f, 1.2f), windowMatH);
        CreateWindow(Local(new Vector3(0, h / 2f + 0.3f, -d / 2f - 0.01f)), LocalRot(Quaternion.Identity),
            new Vector3(1.2f, 1.0f, 0.05f), windowMatH);

        var interiorLight = _entities.CreateEntity();
        _entities.AddComponent(interiorLight, new TransformComponent
        {
            Position = Local(new Vector3(0, h - 0.5f, 0)),
            Rotation = Quaternion.Identity,
            Scale = Vector3.One,
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(interiorLight, LightComponent.Point(
            new Vector3(1.0f, 0.8f, 0.5f), 1.5f, 8f));

        float fenceH = 0.8f;
        float fenceR = 6f;

        CreateFenceSegment(Local(new Vector3(-fenceR / 2f - 0.5f, fenceH / 2f, fenceR / 2f)),
            LocalRot(Quaternion.Identity), new Vector3(fenceR - 1.5f, fenceH, 0.12f), fenceMatH);
        CreateFenceSegment(Local(new Vector3(fenceR / 2f + 0.5f, fenceH / 2f, fenceR / 2f)),
            LocalRot(Quaternion.Identity), new Vector3(fenceR - 1.5f, fenceH, 0.12f), fenceMatH);
        CreateFenceSegment(Local(new Vector3(0, fenceH / 2f, -fenceR / 2f)),
            LocalRot(Quaternion.Identity), new Vector3(fenceR, fenceH, 0.12f), fenceMatH);
        CreateFenceSegment(Local(new Vector3(-fenceR / 2f, fenceH / 2f, 0)),
            LocalRot(Quaternion.Identity), new Vector3(0.12f, fenceH, fenceR), fenceMatH);
        CreateFenceSegment(Local(new Vector3(fenceR / 2f, fenceH / 2f, 0)),
            LocalRot(Quaternion.Identity), new Vector3(0.12f, fenceH, fenceR), fenceMatH);
    }

    private void CreateWall(Vector3 pos, Quaternion rot, Vector3 scale, int matH)
    {
        var wall = _entities.CreateEntity();
        _entities.AddComponent(wall, new TransformComponent
        {
            Position = pos, Rotation = rot, Scale = scale,
            LocalToWorld = Matrix4x4.Identity, IsDirty = true
        });
        _entities.AddComponent(wall, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = matH,
            Visible = true, CastShadow = true
        });
        var shape = _physics.CreateBox(scale.X, scale.Y, scale.Z);
        _physics.AddStaticBody(pos, rot, shape);
    }

    private void CreateWindow(Vector3 pos, Quaternion rot, Vector3 scale, int matH)
    {
        var window = _entities.CreateEntity();
        _entities.AddComponent(window, new TransformComponent
        {
            Position = pos, Rotation = rot, Scale = scale,
            LocalToWorld = Matrix4x4.Identity, IsDirty = true
        });
        _entities.AddComponent(window, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = matH,
            Visible = true, CastShadow = false
        });
    }

    private void CreateFenceSegment(Vector3 pos, Quaternion rot, Vector3 scale, int matH)
    {
        var fence = _entities.CreateEntity();
        _entities.AddComponent(fence, new TransformComponent
        {
            Position = pos, Rotation = rot, Scale = scale,
            LocalToWorld = Matrix4x4.Identity, IsDirty = true
        });
        _entities.AddComponent(fence, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = matH,
            Visible = true, CastShadow = true
        });
        var shape = _physics.CreateBox(scale.X, scale.Y, scale.Z);
        _physics.AddStaticBody(pos, rot, shape);
    }

    private void BuildLampPosts(int poleMatH, int headMatH)
    {
        Vector3[] lampPositions =
        {
            new(5, 0, 0), new(-5, 0, 0),
            new(0, 0, 5), new(0, 0, -5),
            new(8, 0, 8), new(-8, 0, -8),
        };

        foreach (var pos in lampPositions)
            BuildSingleLampPost(pos, poleMatH, headMatH);
    }

    private void BuildSingleLampPost(Vector3 pos, int poleMatH, int headMatH)
    {
        float poleHeight = 4f;

        var pole = _entities.CreateEntity();
        _entities.AddComponent(pole, new TransformComponent
        {
            Position = pos + new Vector3(0, poleHeight / 2f, 0),
            Rotation = Quaternion.Identity,
            Scale = new Vector3(0.08f, poleHeight, 0.08f),
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(pole, new RenderComponent
        {
            MeshHandle = _cylinderHandle, MaterialHandle = poleMatH,
            Visible = true, CastShadow = true
        });

        var poleShape = _physics.CreateBox(0.16f, poleHeight, 0.16f);
        _physics.AddStaticBody(pos + new Vector3(0, poleHeight / 2f, 0), Quaternion.Identity, poleShape);

        var head = _entities.CreateEntity();
        _entities.AddComponent(head, new TransformComponent
        {
            Position = pos + new Vector3(0, poleHeight + 0.2f, 0),
            Rotation = Quaternion.Identity,
            Scale = new Vector3(0.4f, 0.3f, 0.4f),
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(head, new RenderComponent
        {
            MeshHandle = _cubeHandle, MaterialHandle = headMatH,
            Visible = true, CastShadow = false
        });

        var lampLight = _entities.CreateEntity();
        _entities.AddComponent(lampLight, new TransformComponent
        {
            Position = pos + new Vector3(0, poleHeight + 0.1f, 0),
            Rotation = Quaternion.Identity,
            Scale = Vector3.One,
            LocalToWorld = Matrix4x4.Identity,
            IsDirty = true
        });
        _entities.AddComponent(lampLight, LightComponent.Point(
            new Vector3(1.0f, 0.9f, 0.6f), 3.0f, 12f));

        _lampLights.Add(lampLight);
    }

    private void BuildHazards(int hazardMatH, int spikeMatH)
    {
        Vector3[] hazardPositions =
        {
            new( 4f, 0, -3f),
            new(-3f, 0,  5f),
            new( 7f, 0,  2f),
            new(-6f, 0, -4f),
            new( 2f, 0,  8f),
        };

        foreach (var hpos in hazardPositions)
        {
            var hazard = _entities.CreateEntity();
            _entities.AddComponent(hazard, new TransformComponent
            {
                Position = hpos + new Vector3(0, 0.35f, 0),
                Rotation = Quaternion.Identity,
                Scale = new Vector3(0.7f, 0.7f, 0.7f),
                LocalToWorld = Matrix4x4.Identity,
                IsDirty = true
            });
            _entities.AddComponent(hazard, new RenderComponent
            {
                MeshHandle = _cubeHandle, MaterialHandle = hazardMatH,
                Visible = true, CastShadow = true
            });
            _entities.AddComponent(hazard, HazardComponent.Create(15f, 1.5f));

            _hazards.Add((hazard, hpos));

            var hazardShape = _physics.CreateBox(0.7f, 0.7f, 0.7f);
            _physics.AddStaticBody(hpos + new Vector3(0, 0.35f, 0), Quaternion.Identity, hazardShape);

            var spike = _entities.CreateEntity();
            _entities.AddComponent(spike, new TransformComponent
            {
                Position = hpos + new Vector3(0, 0.9f, 0),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4f),
                Scale = new Vector3(0.35f, 0.35f, 0.35f),
                LocalToWorld = Matrix4x4.Identity,
                IsDirty = true
            });
            _entities.AddComponent(spike, new RenderComponent
            {
                MeshHandle = _cubeHandle, MaterialHandle = spikeMatH,
                Visible = true, CastShadow = true
            });
        }
    }

    private void BuildInvisibleWalls()
    {
        float wallHeight = 20f;
        float wallThickness = 1f;
        float wallLen = MapHalfSize * 2f;

        var shape = _physics.CreateBox(wallLen, wallHeight, wallThickness);
        _physics.AddStaticBody(new Vector3(0, wallHeight / 2f, -MapHalfSize), Quaternion.Identity, shape);
        _physics.AddStaticBody(new Vector3(0, wallHeight / 2f, MapHalfSize), Quaternion.Identity, shape);

        var shapeSide = _physics.CreateBox(wallThickness, wallHeight, wallLen);
        _physics.AddStaticBody(new Vector3(MapHalfSize, wallHeight / 2f, 0), Quaternion.Identity, shapeSide);
        _physics.AddStaticBody(new Vector3(-MapHalfSize, wallHeight / 2f, 0), Quaternion.Identity, shapeSide);
    }

    public void Shutdown()
    {
        _rain?.Dispose();
    }
}

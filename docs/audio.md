# Audio

The audio module provides sound playback via Silk.NET OpenAL.

## AudioEngine

The audio engine manages OpenAL context and sound playback:

```csharp
var audio = services.GetRequiredService<AudioEngine>();
```

## Audio Clips

Load audio files:

```csharp
var clip = audio.LoadClip("assets/audio/footstep.wav");
```

## Audio Sources

Create sources to play sounds in 3D space:

```csharp
var source = audio.CreateSource();
source.Clip = clip;
source.Position = new Vector3(10, 0, 5);
source.Volume = 0.8f;
source.Loop = true;
source.Play();
```

## 3D Positional Audio

Audio sources are positioned in world space. The listener follows the camera automatically. Sounds attenuate with distance.

```csharp
source.Position = entityPosition;  // Update each frame for moving sources
source.Play();
source.Stop();
source.Pause();
```

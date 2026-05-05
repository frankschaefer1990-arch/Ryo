# Implementation Plan: Centralized Audio Manager (with Volume Control)

This plan introduces a persistent `AudioManager` that automatically plays the correct music based on the current scene name and allows for dynamic overrides (e.g., for fight scenes).

## Key Assets & Context
- **Script**: `Assets/Skripts/AudioManager.cs`
- **Dependency**: Uses `GameManager.OnSystemsReady` signal to detect scene changes and update music.

## Implementation Steps

### 1. Create the AudioManager Script
Implement a Singleton-based `AudioManager` that:
- Maintains a list of Scene-to-AudioClip mappings.
- Has a persistent `AudioSource` for background music.
- **Volume Control**: Includes a global volume slider (0.0 to 1.0).
- Includes a method `PlayMusic(string clipName)` and `PlayMusic(AudioClip clip)`.
- Supports crossfading for smooth transitions between scenes.

### 2. Configure Scene Mapping
Add a serializable class `SceneMusic` to the manager so the user can drag and drop:
- `Scene Name`: "Temple" -> `Clip`: "Ascent_of_the_Bell_Tower"
- `Scene Name`: "Legend of Ryo" -> `Clip`: "Szene 1"

### 3. Integrate with GameManager
- In `OnEnable`, subscribe to `GameManager.OnSystemsReady`.
- When the signal fires, the `AudioManager` checks `SceneManager.GetActiveScene().name` and plays the corresponding clip.

### 4. Special Music (Fight Mode)
- Provide a public method `StartFightMusic(AudioClip fightClip)` to override the current scene music.
- Provide `EndFightMusic()` to return to the scene's default background music.

## Verification & Testing
1. **Scene Swap**: Move between 'Legend of Ryo' and 'Temple'. Confirm music switches correctly.
2. **Volume Test**: Adjust the volume slider in the Inspector during play mode to verify it affects the audio.
3. **Persistence**: Verify that the `AudioManager` object stays alive when loading scenes.

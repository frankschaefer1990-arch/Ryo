# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: Top-down RPG with an overworld and a separate turn-based battle system.
- Players: Single player.
- Render Pipeline: URP.
- Target Platform: PC (StandaloneWindows64).

# Game Mechanics
## Core Gameplay Loop
Exploration, dialogue-driven cutscenes, and transitioning into a battle scene for combat encounters.
## Controls and Input Methods
Standard WASD/Arrow keys for movement, UI interaction via mouse.

# UI
- Dialogue UI for cutscenes and in-battle messages.
- Battle UI for player and enemy stats.

# Key Asset & Context
- `Assets/Skripts/PlayerMovement.cs`: Controls the player and animator.
- `Assets/Skripts/GameManager.cs`: Manages persistence and scene transitions.
- `Assets/Skripts/TempleIntroController.cs`: Runs the cutscene in the Temple.
- `Assets/Scenes/Battle/BattleScene.unity`: The scene where the crash occurs.

# Implementation Steps

## 1. Fix Player Animation & Direction Persistence
**Goal:** Ensure the player starts in an `Idle` state facing the direction they were moving when they left the previous scene.
- **File:** `Assets/Skripts/PlayerMovement.cs`
- **Changes:**
    - Add a `public void ResetMovementState()` method.
    - Inside this method: set `movement = Vector2.zero`, `isMoving = false`.
    - Manually update the Animator: `animator.SetBool("isMoving", false)`, and set `MoveX`/`MoveY` using the current `lastMovement`.
    - Call `ResetMovementState()` in `Start()`.
- **File:** `Assets/Skripts/GameManager.cs`
- **Changes:**
    - In `OnSceneLoaded`, after finding the player and moving them to the spawn point, call `player.GetComponent<PlayerMovement>().ResetMovementState()`.

## 2. Fix Unity Crash during Battle Transition
**Goal:** Resolve the crash when loading `BattleScene` by preventing multiple `MainCamera` and `CinemachineBrain` conflicts.
- **File:** `Assets/Skripts/GameManager.cs`
- **Changes:**
    - In `CleanupDuplicates()`, add logic to detect and destroy duplicate objects tagged `MainCamera`.
    - If `mainCamera` (the persistent one) exists, destroy any other `MainCamera` in the scene.
- **File:** `Assets/Skripts/TempleIntroController.cs`
- **Changes:**
    - Add a private bool `hasStartedBattle` to ensure `LoadScene` is only called once.

## 3. Prevent Cutscene Animation Conflict
**Goal:** Ensure the cutscene doesn't fight with `PlayerMovement` for animator control.
- **File:** `Assets/Skripts/PlayerMovement.cs`
- **Changes:**
    - Modify `Update()` so that if `canMove` is false, it doesn't just `return` immediately. Instead, it should ensure `isMoving` is false *unless* some other script is explicitly setting it (which the cutscene does).
    - Actually, the current code already does `movement = Vector2.zero` and `return`. I will ensure that `isMoving` is set to false before returning if `canMove` was just disabled.

# Verification & Testing
1. **Direction Test:** Enter a scene walking Right. Verify the player spawns in the next scene in "Idle Right".
2. **Crash Test:** Play through the Temple dialogue and verify the transition to `BattleScene` no longer crashes Unity.
3. **Log Verification:** Check for "GameManager: Nuking duplicate Main Camera" to confirm the cleanup logic is working.

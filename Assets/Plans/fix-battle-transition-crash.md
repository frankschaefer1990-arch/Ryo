# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: Action RPG with transition from exploration to turn-based/QTE battles.
- Players: Single player
- Inspiration: Legend of Dragoon (QTE based battle system)
- Tone / Art Direction: Pixel Art / RPG
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation: Landscape 1920x1080
- Render Pipeline: URP

# Game Mechanics
## Core Gameplay Loop
The player explores the world (Legend of Ryo / Temple scenes). Upon reaching a story point (Skeleton dialogue), a transition to a dedicated BattleScene occurs. After the battle, the player returns to the world.

## Controls and Input Methods
Exploration: PlayerMovement (WASD/Arrows).
Battle: Menu-based selection + QTE (Q, W, E, R keys).

# UI
- Main HUD: Health/Mana bars.
- DialogueUI: Storytelling and battle status messages.
- BattleUI: Commands, QTE rings, and Enemy stats.

# Key Asset & Context
- **GameManager.cs**: Manages scene loading and persistent state. Currently has a logic flaw where it skips persistence if it's a child of "PersistentSystems".
- **DialogueUI.cs**: Used for both story and battle messages. Lacks persistence logic, leading to destruction during scene changes.
- **PersistentSystems**: A GameObject intended to hold all managers. Currently missing the `PersistentSystems.cs` script in the scene, causing it and its children to be destroyed during scene loads.
- **TempleIntroController.cs**: The script that triggers the battle transition after the skeleton dialogue.

# Implementation Steps
The root cause of the crash in builds is the destruction of the core management systems (`GameManager`, `DialogueUI`, `QuestManager` children) during the scene transition, which causes null reference exceptions or coroutine interruptions that Unity 6 builds handle as fatal crashes.

## Step 1: Fix Core Manager Persistence
We will modify the managers to ensure they survive the scene transition regardless of whether their parent is persistent.

1.  **Update `GameManager.cs`**:
    - Change `Awake` logic to always ensure persistence.
    - If it has a parent, detach it (`transform.SetParent(null)`) and call `DontDestroyOnLoad(gameObject)`.
    - This ensures that even if "PersistentSystems" is destroyed, the `GameManager` survives.

2.  **Update `DialogueUI.cs`**:
    - Add a singleton `Instance` check and `DontDestroyOnLoad(gameObject)`.
    - Detach from parent on `Awake`.
    - This is critical because `BattleManager` relies on `DialogueUI` immediately after loading.

3.  **Update `QuestManager.cs`**: (Already has detachment, but we will double check).

## Step 2: Robust Scene Loading in GameManager
- Ensure `LoadSceneAsync` is handled by the persistent `GameManager` instance.
- Add extra safety to `OnSceneLoaded` to handle cases where the player or camera might not be immediately available in the build.

## Step 3: BattleManager Stability
- Update `BattleManager.cs` to wait for all dependencies (`DialogueUI`, `PlayerStats`, `QuestManager`) to be fully ready before starting the battle sequence.
- Add a "Scene Stabilization" delay to allow Unity to finish cleaning up the previous scene's memory.

# Verification & Testing
1.  **Editor Test**: Verify that transitioning from Temple to BattleScene works without creating duplicates in the Hierarchy.
2.  **Build Test (Ctrl + B)**:
    - Play through the skeleton dialogue.
    - Confirm the transition to BattleScene occurs without a crash.
    - Verify that the BattleUI and DialogueUI are functional in the BattleScene.
3.  **Persistence Check**: Check that the `GameManager` and `DialogueUI` objects in the "DontDestroyOnLoad" scene are the same ones created in the `SplashScreen`.

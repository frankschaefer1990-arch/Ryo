# Project Overview
- Game Title: Legend of Ryo
- Issue: Build crashes immediately upon launch (Unity 6 Crash Reporter).
- Primary Cause: The `BattleScene` is set as the first scene (Index 0) in the Build Settings. This scene depends on singletons (like `GameManager` and `PlayerStats`) that are initialized in the `SplashScreen` or Main Menu scenes. Launching directly into `BattleScene` causes initialization failures and unhandled exceptions that crash the build.

# Game Mechanics
## Core Gameplay Loop
- Initialization -> Splash Screen -> Main Menu -> Gameplay (World/Battle).

# Implementation Steps
## 1. Fix Build Settings Scene Order
- **Task**: Reorder scenes so the `SplashScreen` is the entry point.
- **Details**: 
    1. Set `Assets/Scenes/SplashScreen.unity` to Index 0.
    2. Set `Assets/Scenes/Legend of Ryo.unity` to Index 1.
    3. Move `Assets/Scenes/Battle/BattleScene.unity` to Index 2 or later.

## 2. Robustness Improvements in Scripts
- **Task**: Add null checks and safety to scripts that run on startup.
- **Files**: `Assets/Skripts/Battle/BattleManager.cs`, `Assets/Skripts/Dialoge & Chat/DialogueUI.cs`
- **Details**:
    - `BattleManager.cs`: Add null check for `GameManager.Instance` in `TryRun` and `EndBattle`.
    - `DialogueUI.cs`: Improve the check for scene validity to prevent potential crashes during object validation.

## 3. Hierarchy Cleanup
- **Task**: Remove missing script references in `BattleScene`.
- **Details**:
    - Locate `Button_0` in `BattleCanvas/AngriffPanel/Viewport/Content/` and remove the broken script component.

# Verification & Testing
- **Editor Test**: Run the game starting from `SplashScreen`. Ensure it transitions to `Legend of Ryo` and then to other scenes correctly.
- **Build Test**: Perform a "Build and Run" (Ctrl+B). The game should now launch into the Splash Screen without crashing.

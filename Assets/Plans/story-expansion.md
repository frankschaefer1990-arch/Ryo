# Project Overview
- Game Title: Legend of Ryo
- Objective: Enhance game with cinematic cutscenes, story extensions, and fix Audio persistence without altering existing base structures.

# Game Mechanics
## Core Gameplay Loop
- Intro (Scene 1) -> Exploration (Scene 1) -> Temple Cutscene (Scene 2) -> Boss Battle -> Access Labyrinth (Scene 3).

# UI
- Black/White screen overlay for intro and transitions.
- Existing `DialogueUI` for character dialogue.

# Key Asset & Context
- `QuestManager`: Add `defeatedTempleBoss` flag.
- `FadeManager.cs`: Handles screen fading and eye-blink effects.
- `IntroCutscene.cs`: Sequence for Scene 1 start.
- `TempleCutscene.cs`: Sequence for Scene 2 entry.
- `StoryEvents.cs`: Central trigger to unlock bridge and handle scene-specific story logic.

# Implementation Steps
## 1. System Robustness & Persistence
- **Task**: Ensure `AudioManager` and `GameManager` correctly initialize and persist.
- **Details**: Verify `SplashScreen` (Index 0) has these managers. Ensure `QuestManager` is persistent.

## 2. Global Progress State
- **Task**: Update `QuestManager.cs` with `defeatedTempleBoss` flag.
- **Dependency**: Existing `QuestManager.cs`.

## 3. Fade & Visual Effects
- **Task**: Implement `FadeManager.cs` and add a Canvas overlay with a black/white image.
- **Details**:
    - Black Image (FullScreen).
    - White Image (FullScreen).
    - Methods: `FadeIn`, `FadeOut`, `PlayEyeBlink`.

## 4. Scene 1 Intro Extension
- **Task**: Create `IntroCutscene.cs` and trigger it on first start in Scene 1.
- **Sequence**:
    1. Screen starts black.
    2. White flash -> Black (Eye blink).
    3. Fade-In.
    4. Camera pan to Temple Entrance while playing "Screem" (Hollowschrei).
    5. Camera pan back to Ryo.
    6. Dialogue: Ryo: "Was war das???".
    7. Enable movement.

## 5. Scene 2 Temple Cutscene
- **Task**: Create `TempleCutscene.cs`.
- **Sequence**:
    1. Trigger on first entry.
    2. Camera focus: Injured Skeleton -> Master -> Ryo.
    3. Dialogue sequence as requested.
    4. Ryo moves towards Skeleton.
    5. Transition to Battle with "Gebrochener Knochenhollow" (3000 Max HP, 500 Start HP).

## 6. Scene 3 & Bridge Unlock
- **Task**: Update `BridgeBlocker.cs` or a new trigger to check `defeatedTempleBoss`.
- **Details**: If boss is defeated, hide bridge wall and allow access to Labyrinth.
- **Dialogue**: Add optional text for bridge and Scene 3 entry.

# Verification & Testing
- Start from `SplashScreen`.
- Verify Scene 1 Intro plays once.
- Enter Temple, verify Cutscene sequence.
- Finish Battle, return to Scene 1, verify Bridge is unlocked.
- Enter Labyrinth, verify dialogue.

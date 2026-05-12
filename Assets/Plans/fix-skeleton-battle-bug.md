# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: An RPG where the player (Ryo) explores a temple, fights a skeleton boss, and progresses through a story involving a "Soul Absorber".
- Players: Single player
- Inspiration: Legend of Dragoon (QTE system)
- Render Pipeline: UniversalRP
- Screen Orientation: Landscape 1920x1080 (PC Standalone)

# Game Mechanics
## Core Gameplay Loop
- Exploration in a top-down/2.5D environment.
- Turn-based combat with QTE (Quality Time Event) mechanics for attacks.
- Story progression via cutscenes and dialogues.

## Controls and Input Methods
- Keyboard for movement (WASD/Arrows) and combat interaction.
- Mouse for UI navigation.

# UI
- **BattleUI**: Displays HP/Mana, command panels, and QTE prompts.
- **DialogueUI**: Displays character dialogues with a typewriter effect.
- **MyUIManager**: Manages persistent UI panels (Inventory, etc.).

# Key Asset & Context
- **Assets/Skripts/Battle/BattleManager.cs**: Controls battle logic, state transitions, and victory/defeat sequences.
- **Assets/Skripts/TempleIntroController.cs**: Controls the intro and post-battle cutscenes in the Temple scene.
- **Assets/Skripts/Dialoge & Chat/DialogueUI.cs**: Handles on-screen text display.
- **Assets/Data/Battle/Enemy_Skelettkrieger.asset**: Data for the skeleton boss.

# Implementation Steps
## Phase 1: Fix Battle Scene Skeleton and Dialogue
1. **Modify `BattleManager.cs`**:
    - **Hide Enemy on Victory**: In the `EndBattle` coroutine, immediately deactivate `enemyPos.gameObject` when `state == BattleState.WON` to ensure the skeleton disappears visually.
    - **Improve Victory Dialogue**: Update `ShowBattleMessage` to use `BattleUI.Instance.ShowActionMessage` as a fallback. This ensures that even if `DialogueUI` is missing from the scene, the player sees the "Sieg!" message.
    - **Extend Duration**: Increase the wait time or the dialogue's visible duration to ensure the player can read the victory message before the scene transitions.

## Phase 2: Fix Temple Scene Post-Battle Sequence
1. **Modify `TempleIntroController.cs`**:
    - **Robust Skeleton Search**: Ensure the `skeleton` reference is reliably found by checking for "Skelett Krieger" and adding a fallback for other possible names or tags if necessary.
    - **Immediate Hide**: In `PostBattleSequence`, ensure the skeleton is hidden immediately if for some reason the fade coroutine fails.
    - **Quest Progress Check**: Verify that `defeatedTempleBoss` is checked correctly in `Start` to trigger the `PostBattleSequence`.

# Verification & Testing
1. **Battle Victory Test**: Enter the battle with the skeleton, defeat it, and verify that:
    - The skeleton sprite disappears immediately.
    - A victory message ("Sieg!") appears on screen.
    - After 2 seconds, the game transitions back to the Temple scene.
2. **Temple Transition Test**: Verify that upon returning to the Temple:
    - The `PostBattleSequence` starts automatically.
    - Ryo says "Ich... habe gewonnen...?".
    - The skeleton in the Temple scene fades out and disappears.
    - The Soul Absorption effect triggers.
3. **Dialogue Reliability**: Check the Unity Console for any "DialogueUI missing" or "Skeleton reference missing" errors.

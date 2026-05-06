# Project Overview
- Game Title: Legend of Ryo (Battle System)
- High-Level Concept: A turn-based battle system with QTE (Quick Time Event) combos.
- Players: Single player
- Inspiration: Classic JRPGs with interactive combat.
- Render Pipeline: URP

# Game Mechanics
## Core Gameplay Loop
- Player chooses an action (Attack/Spell/Item).
- Executing a skill triggers a visual effect and potentially a QTE combo.
- QTE requires the player to press Q, W, E, or R within a time limit to continue the combo.
- Success deals damage; failure ends the turn/skill execution.

## Controls and Input Methods
- Mouse for menu navigation.
- Keyboard (Q, W, E, R) for combos.

# UI
- **QTEPrompt**: A UI element that should display the key (Q, W, E, R) the player needs to press. Currently missing the text component.
- **Panels**: Command, Attack, Spell, and Item panels for selection.

# Key Asset & Context
- `BattleManager.cs`: Main logic for skill execution and state management.
- `BattleUI.cs`: Handles UI updates.
- `ComboSystem.cs`: Manages the QTE logic.
- `BattleSkill.cs`: ScriptableObject data for skills.
- `ProceduralSlash.cs`: Script for visual effects.

# Implementation Steps
## 1. Fix Combo UI & Logic
- **Task**: The `QTEPrompt` object in the hierarchy is empty. It needs a `TextMeshPro - Text (UI)` component to show the keys.
- **Files**: `BattleScene.unity`, `BattleUI.cs`
- **Details**:
    - Add a child `GameObject` to `QTEPrompt` named "PromptText" with a `TextMeshPro - Text (UI)` component.
    - Assign this component to the `qteText` field in the `BattleUI` script on the `BattleManagers` object.
    - Style the text (e.g., center-aligned, large font, bright color) so it's clearly visible.

## 2. Audio Support for Skills
- **Task**: Enable skills to play unique sounds.
- **Files**: `Assets/Skripts/Battle/BattleSkill.cs`, `Assets/Skripts/Battle/BattleManager.cs`
- **Details**:
    - Add `public AudioClip skillSound;` to `BattleSkill.cs`.
    - Add `public AudioSource audioSource;` to `BattleManager.cs`.
    - In `BattleManager.ExecuteSkill`, play `skill.skillSound` using the `audioSource` at the start of the effect.

## 3. Expose Visual Effects in Hierarchy
- **Task**: Move the "Blitz" and "Schlag" effects from purely code-generated to hierarchy-based objects for easy adjustment.
- **Files**: `BattleScene.unity`, `Assets/Skripts/Battle/ProceduralSlash.cs`, `Assets/Skripts/Battle/BattleManager.cs`
- **Details**:
    - **Hierarchy**: Create a parent GameObject "Effects" under `BattleManagers`. Create two children: `SlashEffect` and `LightningEffect`.
    - **Components**: Add a `LineRenderer` to both. Add the `ProceduralSlash` script to both.
    - **Script Modification**: Update `ProceduralSlash.cs` to use the `LineRenderer` component already present on the object instead of adding one via code.
    - **BattleManager Update**: Replace the single `slashEffect` reference with `public ProceduralSlash slashEffect;` and `public ProceduralSlash lightningEffect;`.
    - **ExecuteSkill Logic**: Update the logic to choose the correct effect:
        - If `skill.isSpell` is true, call `lightningEffect.PlaySlash(...)`.
        - Else, call `slashEffect.PlaySlash(...)`.
    - **Positioning**: Ensure the effects appear at the `enemyPos` (or `playerPos` when hit).

## 4. Final Wiring & Configuration
- **Task**: Assign all references and assets.
- **Details**:
    - Drag the new hierarchy objects into the `BattleManager` slots.
    - Update the `BattleSkill` assets (`Skill_Blitzstrahl`, `Skill_WildeSchlaege`) with the user's sounds once they are identified.
    - Configure the `LineRenderer` settings (Width, Material, Gradient) on the hierarchy objects so the user can see their "Blitz" and "Schlag" visuals and tweak them.

# Verification & Testing
- **Combo Test**: Select "Wilde Schläge". Verify that a Q/W/E/R letter appears on screen. Pressing it should continue the animation; pressing nothing or the wrong key should show "Combo unterbrochen!".
- **Sound Test**: Use "Blitzstrahl". Verify the lightning sound plays.
- **Visual Test**: Adjust the `LightningEffect` color or width in the inspector and verify the changes appear in-game during the spell execution.

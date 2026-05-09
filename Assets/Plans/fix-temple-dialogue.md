# Project Overview
    - Game Title: FS Gaming - Legend of Ryo
    - High-Level Concept: Top-down 2D action/adventure game.
    - Players: Single player
    - Render Pipeline: UniversalRP

# Game Mechanics 
## Core Gameplay Loop
The player explores, interacts with dialogue, and enters combat.

# Key Asset & Context
- `DialogueUI.cs`: Main script for the chatbox.
- `GameManager.cs`: Handles global state and cleanup.
- `TempleIntroController.cs`: Manages the temple cutscene.

# Implementation Steps
1. **DialogueUI.cs Transformation**:
    - Convert `DialogueUI` to a strictly local-scene component.
    - Remove `ReconnectUI` global searching.
    - Change `Awake` to not use `DontDestroyOnLoad`.
    - Ensure `Instance` is managed correctly for single-scene usage.
2. **GameManager.cs Cleanup**:
    - Remove `DialogueUI` from `CleanupDuplicates`.
3. **Temple Scene Correction**:
    - Programmatically fix references on the `DialogueUI` component in the Temple scene.
4. **TempleIntroController.cs Timing Fix**:
    - Ensure it waits for the local `DialogueUI` and triggers messages only when ready.
    - Use exact dialogue strings: "Der Meister hat ihn geschwächt, jetzt ist meine Stunde!" and "Sirb die Made!".

# Verification & Testing
- Test scene transition from Szene 1 to Temple.
- Verify Dialogue appears in Temple.
- Verify Ryo's walk timing.

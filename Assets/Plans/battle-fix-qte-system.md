# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: Dark Fantasy JRPG with QTE combat.
- Players: Single player
- Reference: Legend of Dragoon

# Game Mechanics
## Core Gameplay Loop
Exploration -> Battle -> QTE Additions -> Rewards.

# Key Asset & Context
- **QTE Assets Found**:
    - `Assets/Data/Battle/qte_outer_ring.png`
    - `Assets/Data/Battle/qte_shrink_ring.png`
    - `Assets/Data/Battle/qte_button_core.png`
- **Missing References Found**:
    - `blitzAnimationObject` is unassigned in `BattleScene.unity`.
    - `BattleManagers` GameObject is set to Inactive in the scene.

# Implementation Steps

## Step 1: Overworld -> Battle Fix (The "Ghost Player" Problem)
- **Problem**: Persistent Player stays active and controllable in BattleScene.
- **Solution**: Create `BattleStateController.cs` to disable/enable Player components based on scene name.
- **Files**: `Assets/Skripts/Battle/BattleStateController.cs`

## Step 2: Fix Sound & Blitz Visual
- **Sound Fix**:
    - Ensure `BattleManagers` object is active.
    - Add fallback `AudioSource` detection.
    - Update `BattleManager.cs` to play sound even if QTE starts.
- **Blitz Fix**:
    - Create a simple Blitz prefab or dynamically assign the `BlitzAnimation.png` to a sprite object.
    - Wire `blitzAnimationObject` in the `BattleManager`.

## Step 3: Legend of Dragoon QTE System
- **QTEManager**: Central logic for ring shrinking and timing.
- **QTERingController**: Handles the visual scaling and "Hook" effect (slowdown near target).
- **Key Display**: Uses TMP_Text to show Q/W/E/R.
- **Timing Windows**: 
    - Perfect: 0.9 - 1.1 scale.
    - Good: 0.8 - 1.2 scale.
- **Visual Polish**: Flash effect on hit, shake on Perfect.

## Step 4: UI / Dialog Improvement
- **TMP Settings**: Force "Auto Size", "Wrapping", and "Overflow: Truncate".
- **Speaker Name**: Ensure name scaling matches.

# Verification & Testing
1. **Transition**: Check if player is hidden in battle and reappears in Overworld.
2. **Sound**: Verify skill sound plays on "Wilde Schläge" and "Blitzstrahl".
3. **Blitz**: Verify the lightning sprite appears when using "Blitzstrahl".
4. **QTE**: Test the "Hook" effect and timing windows.

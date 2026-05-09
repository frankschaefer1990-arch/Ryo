# Implementation Plan - Core Initialization & Transition Cleanup

This plan focuses on creating a rock-solid initialization in the `SplashScreen` and ensuring a clean transition to `Legend of Ryo` and subsequent scenes. It addresses UI scaling, software cursor stability, and functionality of buttons/chatboxes.

## Project Overview
- **Objective**: Stabilize the persistent systems (Managers, Canvas, EventSystem) starting from `SplashScreen`.
- **Key Issues**: Software cursor scaling on click, non-functional UI buttons/chatboxes after scene load, redundant "Name" displays.

## Proposed Changes

### 1. Centralized Persistent Systems (Bootstrap)
- I will ensure `SplashScreen` contains a single root-level object (e.g., `PersistentSystems`) that holds all core managers.
- This object will be initialized once and survive all scene loads.

### 2. Canvas & EventSystem Persistence
- **One Canvas**: I will ensure only one main UI Canvas persists. If a new scene provides a better one (like a Battle UI), it will be handled as an addition, not a replacement that breaks core systems.
- **One EventSystem**: I will enforce a single global `EventSystem`. Any `EventSystem` found in a newly loaded scene will be immediately removed to prevent input conflicts.

### 3. Software Cursor Fix
- **Absolute Size**: The software cursor will be placed on a dedicated `Overlay Canvas` created in `SplashScreen`. 
- **Scale Locking**: I will use a more aggressive approach to lock its scale to (1,1,1) and its size to `cursorSize` (80px), ensuring no external factor (like layout groups or UI scaling) can alter it.
- **Raycast Neutral**: The cursor canvas will have its `GraphicRaycaster` removed (if any) to ensure it never blocks clicks.

### 4. Dialogue UI & Chatbox Robustness
- **Dynamic Re-linking**: `DialogueUI` will be modified to "search and find" the chatbox in the active scene every time a message is requested, rather than relying on stale references.
- **Redundant UI Hiding**: It will explicitly disable any objects named "Name" or "TextPlayerName" that are not the currently designated speaker field.

### 5. GameManager Cleanup Refinement
- I will rewrite the `CleanupDuplicates` logic to be less destructive. It will prioritize the managers from `SplashScreen` and gracefully clean up scene-specific duplicates without nuking important GameObjects.

## Implementation Steps

### Step 1: MyUIManager Stabilization
- **File**: `Assets/Skripts/MyUIManager.cs`
- **Change**: Ensure the Software Cursor Canvas is created once and is absolutely independent of scene UI. Lock scale and size strictly.
- **Dependency**: None

### Step 2: DialogueUI Robustness
- **File**: `Assets/Skripts/Dialoge & Chat/DialogueUI.cs`
- **Change**: Improve the `ReconnectUI` logic to be scene-aware. Ensure it doesn't block raycasts.
- **Dependency**: Step 1

### Step 3: GameManager Foundation
- **File**: `Assets/Skripts/GameManager.cs`
- **Change**: Rewrite persistence logic to favor `SplashScreen` initialization. Fix `EventSystem` handling.
- **Dependency**: Step 2

### Step 4: SplashScreen Finalization
- **Action**: Use a script to organize `SplashScreen` roots into a clean hierarchy.
- **Dependency**: Step 3

## Verification & Testing
1.  **Cursor Test**: Click everywhere in `Legend of Ryo` and check if the cursor stays at 80px.
2.  **Transition Test**: Start from `SplashScreen`, move to `Legend of Ryo`, then to `Temple`. Check if stats and inventory are correct.
3.  **Interaction Test**: Open Backpack, talk to Merchant, and trigger the Skeleton dialogue. All should display correctly without redundant "Name" boxes.
4.  **Battle Test**: Enter battle and verify buttons are clickable.

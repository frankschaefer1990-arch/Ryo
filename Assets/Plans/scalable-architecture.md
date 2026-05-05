# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: A 2D RPG with exploration, inventory management, and quest systems.
- Players: Single player
- Target Platform: PC (Standalone Windows)
- Render Pipeline: URP

# Game Mechanics
## Core Gameplay Loop
The player explores the world (scenes like Temple, Forest, Village), interacts with NPCs and objects (Dialogue System), manages resources (Gold and Potions), and improves their character (Attribute System).

## Controls and Input Methods
- Movement: WASD / Arrow Keys
- Interaction: R Key
- Inventory: I Key
- Backpack: B Key
- Consumption: Right-click in inventory
- Selection: Left-click in inventory/shop

# Implementation Steps (Architecture Cleanup & Scalability)

## Phase 1: The Orchestrator (GameManager Sync)
1. **Modify [Assets/Skripts/GameManager.cs]**:
    - Add `public static System.Action OnSystemsReady;`.
    - Invoke `OnSystemsReady?.Invoke();` at the very end of `OnSceneLoaded`, after `CleanupDuplicates`, `ReconnectCoreReferences`, and `MovePlayerToSpawn`.
    - This ensures all core objects (Player, Canvas) are 100% stable before other scripts try to access them.

## Phase 2: Decoupling Managers (Subscriber Pattern)
*Note: The internal logic of these functions (how they find panels, how they slice arrays) will NOT be changed.*

1. **Update [Assets/Skripts/Inventar/InventoryManager.cs]**:
    - Add `OnEnable` and `OnDisable` methods.
    - Subscribe/Unsubscribe `RefreshInventory` to `GameManager.OnSystemsReady`.
    - Remove the `SceneManager.sceneLoaded` listener to avoid redundant/out-of-sync calls.

2. **Update [Assets/Skripts/Dialoge & Chat/DialogueUI.cs]**:
    - Add `OnEnable` and `OnDisable` methods.
    - Subscribe/Unsubscribe `ReconnectUI` to `GameManager.OnSystemsReady`.

3. **Update [Assets/Skripts/Inventar/PlayerStats.cs]**:
    - Add `OnEnable` and `OnDisable` methods.
    - Subscribe/Unsubscribe `ReconnectUI` to `GameManager.OnSystemsReady`.
    - Remove the `SceneManager.sceneLoaded` listener.

4. **Update [Assets/Skripts/MyUIManager.cs]**:
    - Add `OnEnable` and `OnDisable` methods.
    - Subscribe/Unsubscribe `ReconnectUIFromGameManager` to `GameManager.OnSystemsReady`.

5. **Update [Assets/Skripts/Inventar/ShopManager.cs]**:
    - Add `OnEnable` and `OnDisable` methods.
    - Subscribe/Unsubscribe `ReconnectShop` to `GameManager.OnSystemsReady`.

## Phase 3: Cleanup & Scaling
1. **Refactor [Assets/Skripts/GameManager.cs]**:
    - Remove the hardcoded calls inside `ReconnectSystems()` (since scripts now register themselves).
    - This allows adding new scenes (Forest, Castle) and new systems (Quest Log, Bestiary) without ever touching the `GameManager` again.

# Verification & Testing
1. **Scene Transition Test**: Travel from 'Legend of Ryo' to 'Temple' and back. 
    - Check Console for: "GameManager: Sende Signal OnSystemsReady".
    - Verify that Inventory, Shop, and Dialogue still work (visual check).
2. **Persistent Value Test**: Ensure Gold and Potion counts are preserved across transitions.
3. **Manual Trigger Check**: Verify House Interaction (R) still triggers after returning from Temple.

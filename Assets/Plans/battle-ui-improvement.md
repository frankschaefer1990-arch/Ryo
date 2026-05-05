# Project Overview
- Game Title: Legend of Ryo (Context inferred)
- High-Level Concept: RPG Battle System with QTE elements and persistent player stats.
- Players: Single player.
- Render Pipeline: UniversalRP.
- Screen Orientation: Landscape (800x600 reference).

# Game Mechanics
## Core Gameplay Loop
- Turn-based combat where player stats (HP, Mana, Strength) are persistent across scenes.
- Enemies have variable HP and names.
- UI provides feedback on health and interaction.

## Controls and Input Methods
- Mouse/Touch interaction with UI Buttons (Angriff, Zauber, Items, Flucht).
- Visual feedback on buttons to indicate selection/hover.

# UI
- **HP Bars**: Both player and enemy bars will now show numeric values (Current/Max).
- **Enemy Name**: Displayed in the top right of the Chatbox area.
- **Button Feedback**: Subtle white-transparent highlight on hover/selection.
- **Font**: Consistent use of `MedievalSharp-Regular SDF 1`.

# Key Asset & Context
- `Assets/Skripts/Battle/BattleUI.cs`: Main UI controller.
- `Assets/Skripts/Battle/BattleManager.cs`: Handles battle logic and UI updates.
- `Assets/Skripts/Inventar/PlayerStats.cs`: Source of player data.
- `Assets/Fronts/MedievalSharp-Regular SDF 1.asset`: Shared font asset.

# Implementation Steps
## 1. Script Updates
- **BattleUI.cs**: 
    - Update to use `TMPro` for all text fields.
    - Add fields for `playerHPText`, `enemyHPText`, and `enemyNameText`.
    - Update HP methods to accept numeric values for string formatting.
- **BattleManager.cs**:
    - Update `SetupBattle` and damage/heal calls to pass numeric HP values to `BattleUI`.
    - Call `SetEnemyName` during setup.

## 2. Scene UI Hierarchy (Non-Destructive)
- **HP Bar Numbers**: 
    - Create `TMP_Text` objects as children of `PlayerHPBar` and `EnemyHPBar`.
    - Center them and apply the Medieval font.
- **Enemy Name**:
    - Create a `TMP_Text` object inside the `Chatbox`.
    - Anchor it to the Top-Right.
- **Buttons**:
    - Find all buttons in `CommandPanel`.
    - Set `Transition` to `Color Tint`.
    - Set `Highlighted` and `Selected` colors to `#FFFFFF` with ~40 Alpha.

## 3. Wiring and Font Application
- Assign the new Text components to the `BattleUI` instance in the scene.
- Ensure all text elements use the `MedievalSharp` SDF font.

# Verification & Testing
- **Visual Check**: Open `BattleScene`. Ensure text is centered on HP bars and name is in the chatbox.
- **Functionality**:
    - Start a battle: Verify player HP matches the global stats.
    - Take damage/Deals damage: Verify numbers update (e.g., "274/300").
    - Hover/Click buttons: Verify the subtle white glow.
    - Check Enemy Name: Verify it matches the `EnemyData` assigned to the manager.

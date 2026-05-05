# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: Turn-based RPG combat inspired by Pokémon Red/Blue (Back-view Player, Front-view Enemy).
- Players: Single player
- Inspiration: Shinigami-themed (Copyright-friendly original names).
- Tone / Art Direction: 2D Pixel Art with Procedural FX.
- Target Platform: PC.
- Scene Path: `Assets/Scenes/Battle/BattleScene.unity`

# Game Mechanics
## Core Gameplay Loop
1. **Transition**: World -> Battle (Fade effect, variable background).
2. **Turn Start**: Display "Enemy [Name] appeared!".
3. **Player Phase**: Select Command (Attack, Spells, Items, Run).
4. **Combo System (QTE)**: During attacks like "Wild Strikes", random keys (Q, W, E, R) appear. The player has ~0.8s per key. Success triggers extra hits.
5. **Enemy Phase**: Enemy attacks with basic or special moves.
6. **Victory/Defeat**: Grant XP/Gold or return to last checkpoint.

## Combat Actions
- **Attack (Wild Strikes)**: 3-hit combo potential. White slash.
- **Attack (Spirit Slash)**: 4-hit combo, purple slash effect, higher damage.
- **Spells (Shinigami-themed)**:
    - *Flash Bolt*: White energy strike (High crit).
    - *Soul Bind*: Chance to stun enemy for 1 turn.
    - *Azure Impact*: Blue energy blast (High base damage).
- **Items**: Only usable items (Potions) from `InventoryManager`.

# UI Structure (Pokemon Style)
- **Background Layer**: Variable sprite (`Hintergrund BattleSzene Tempel.png`).
- **Chatbox Layer**: Static sprite (`Battledzene Chatbox.png`) at the bottom.
- **Enemy Box (Top-Left)**: Name and HP Bar.
- **Player Box (Bottom-Right)**: Buttons for Attack, Spells, Items, Run.
- **Player Sprite**: Back-view (Bottom-Left).
- **Enemy Sprite**: Front-view (Top-Right/Center).
- **Combo Popup**: Centered Q/W/E/R prompt with a timer ring.

# Key Asset & Context
- **Global Systems (Reused)**:
    - `GameManager`: Scene loading.
    - `PlayerStats`: Persistent HP, XP, Strength.
    - `InventoryManager`: Access to Potions.
    - `DialogueUI`: Log messages in the Chatbox.
- **New Assets**:
    - `EnemyData.cs`: ScriptableObject for Enemy Stats & Sprite.
    - `BattleManager.cs`: Handles State Machine and Turn logic.
    - `ComboSystem.cs`: Manages the QTE prompts.
    - `ProceduralSlash.cs`: Scripted effect using LineRenderer/Particles.
    - `BattleUI.cs`: Manages the Pokemon-style UI layout.

# Implementation Steps
1. **Scene & UI Setup**: 
    - Construct hierarchy in `BattleScene.unity`.
    - Set up `Canvas` with Pokemon-style layout and `Battledzene Chatbox.png`.
2. **Data & Logic**:
    - Implement `EnemyData` and `BattleManager`.
    - Filter items in `InventoryManager` to show only Potions.
3. **QTE & FX**:
    - Implement `ComboSystem` with 0.8s timing.
    - Create `ProceduralSlash` for dynamic white/purple effects.
4. **Animation**:
    - Script the "Lean forward" movement for the Player sprite.
5. **Integration**:
    - Connect to `DialogueUI` for combat log feedback.

# Verification & Testing
- **Combo Test**: Verify hits only trigger on correct key press within 0.8s.
- **Item Filtering**: Ensure swords/equipment are NOT shown in battle.
- **Visuals**: Verify HP bars and the procedural slash colors.

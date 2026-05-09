# Project Overview
- Game Title: Legend of Ryo
- High-Level Concept: Top-down RPG.
- Scenario: Intro in the "Legend of Ryo" scene.

# Game Mechanics
## Core Gameplay Loop
The game starts with an intro camera pan and a dialogue.

# Key Asset & Context
- `Assets/Skripts/IntroCutscene.cs`: Manages the intro sequence.
- `Assets/Skripts/PlayerMovement.cs`: Handles player input.

# Implementation Steps

## 1. Fix Player Lock in IntroCutscene
**Goal:** Ensure the player is successfully locked at the start of the intro.
- **File:** `Assets/Skripts/IntroCutscene.cs`
- **Gedanken (Thoughts):** 
    - Derzeit versucht das Skript nur ein einziges Mal in `Start()`, die `PlayerMovement`-Komponente zu finden. Falls der Spieler (der persistent ist) zu diesem Zeitpunkt noch nicht voll initialisiert oder in der Szene registriert ist, schlägt der Lock fehl.
    - Ich werde eine kleine Schleife hinzufügen, die wartet, bis der Spieler gefunden wurde.
    - Außerdem werde ich sicherstellen, dass `canMove` erst am Ende der Coroutine (nach dem Dialog) wieder auf `true` gesetzt wird.
- **Änderungen (Changes):**
    - Überarbeitung der `PlayIntro`-Coroutine: Hinzufügen einer Warteschleife für `playerMove`.
    - Sicherstellen, dass der Lock aktiv bleibt, bis die "Was war das?" Sprechblase geschlossen wurde.

# Verification & Testing
1. **Intro Test:** Starte die Szene "Legend of Ryo". Versuche während der Kamerafahrt und während des Dialogs den Charakter zu bewegen. Er sollte stillstehen.
2. **Post-Intro Test:** Überprüfe, ob die Bewegung nach dem Dialog ("Was war das?") wieder freigegeben wird.

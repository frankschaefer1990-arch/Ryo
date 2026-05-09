# Implementation Plan - Mana Bar and Build Fixes

## 1. Add Mana Bar to Battle UI
- Update `BattleUI.cs` to include references for `playerManaFill` (Image) and `playerManaText` (TMP_Text).
- Add a method `UpdatePlayerMana(float ratio, int curr, int max)` to update the visuals.
- The user will need to manually hook these up in the Unity Editor for the `BattleUI` prefab/object.

## 2. Mana Regeneration in Battle
- Update `BattleManager.cs` to regenerate 5 mana at the start of the `PlayerTurn()` method.
- Call `UpdatePlayerMana` in `BattleUI` whenever mana changes.
- Ensure `PlayerStats.Instance` is used to manage the actual mana values.

## 3. Fix Standalone Build Crash & Buying Issues
- **GameManager Safety**: Refine `CleanupDuplicates` to be even more defensive. Avoid destroying objects that are marked as persistent if they were already correctly setup.
- **InventoryManager Initialization**: Ensure `slotOccupied` is initialized with a default size if it's null during `Awake` or `AddPotion`.
- **UI Persistence**: In `MyUIManager`, ensure it doesn't try to reconnect to a Canvas that is currently being destroyed or replaced.

## 4. Implementation Steps
### Step 1: Update BattleUI.cs
- Add `public Image playerManaFill;` and `public TMP_Text playerManaText;`.
- Add `UpdatePlayerMana` method.

### Step 2: Update BattleManager.cs
- Modify `PlayerTurn()` to add `PlayerStats.Instance.RestoreMana(5);`.
- Update the UI call in `SetupBattle` and `PlayerTurn` to show current Mana.

### Step 3: Refine GameManager.cs
- Ensure `Canvas` management is robust against Splash screen transitions.

### Step 4: Refine InventoryManager.cs
- Add more robust checks in `AddPotion` to handle missing sprites or uninitialized arrays more gracefully.

## 5. Verification & Testing
- Build the game (`Ctrl+B`).
- Verify the Mana bar appears and updates.
- Verify Potion purchase works.
- Verify the Skeleton-to-Battle transition doesn't crash.

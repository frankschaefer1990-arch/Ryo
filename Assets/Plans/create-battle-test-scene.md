# Plan: Create Battle Test Scene and Fix Crashes

## Analysis
The user wants a secondary Battle Scene (`BattleTestScene`) to experiment with fixes without breaking the main one.
Suspected issues:
1.  **Redundant EventSystem**: The scene has its own EventSystem, while the `GameManager` carries a persistent one.
2.  **Missing AudioListener**: The `Main Camera` in `BattleScene` lacks an AudioListener.
3.  **Player Transfer**: Conflict between the persistent Player object and scene-specific testing objects.

## Proposed Fixes
1.  **Duplicate Asset**: Copy `BattleScene.unity` to `BattleTestScene.unity`.
2.  **Scene Optimization (to be done via Editor Script/RunCommand once in Agent Mode)**:
    *   Remove `EventSystem` from `BattleTestScene`.
    *   Add `AudioListener` to the scene's `Main Camera`.
    *   Ensure `PlayerStats_BattleTest` doesn't conflict with the persistent singleton.
3.  **Script Update**:
    *   `TempleIntroController.cs`: Change `LoadScene("BattleScene")` to `LoadScene("BattleTestScene")`.
    *   `GameManager.cs`: Refine `OnSceneLoaded` to be even more aggressive with cleanup if needed.

## Implementation Steps
1.  **Plan**: Write this plan and get approval.
2.  **Execution (Agent Mode)**:
    *   Call `AssetDatabase.CopyAsset`.
    *   Update Build Settings.
    *   Use a script to modify the scene objects.
    *   Apply `CodeEdit` to `TempleIntroController.cs`.

## Verification
1.  **Build (Ctrl + B)**: Trigger the transition from Temple.
2.  **Check Console**: Look for "Multiple EventSystems" or "Zero Cameras" errors.
3.  **Functionality**: Verify the battle starts correctly in the test scene.

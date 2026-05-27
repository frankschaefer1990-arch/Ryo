# Plan: Fix Idol System

1.  **Script Updates:** Update `StoneIdol.cs` and `IdolPuzzleManager.cs` to handle serialized references and more reliable animator updates.
2.  **Collider Fix:** Ensure exactly 2 colliders per idol: one small physical, one slightly larger interaction trigger.
3.  **Radial Menu Fix:** Ensure buttons in the Radial Menu correctly call the rotation method and are visually opaque.
4.  **Audio Setup:** Assign the generated `StoneSlide.wav` to all idols.
5.  **Waterfall Solution:** Implement the waterfall opening visual and logic.
6.  **Consistency Check:** Verify and potentially regenerate directional animations to ensure it's always the same gargoyle.

# Project Overview
- Game Title: Legend of Ryo (Battle System)
- Goal: Integrate user-provided strike animations into the "Wilde Schläge" skill.

# Implementation Steps

## 1. Modify BattleManager.cs to support custom animations
The current code only calls the `ProceduralSlash` component and skips activating the GameObject if that component is found. To support the user's children animations, we need to ensure the parent object is activated.

- **File**: `Assets/Skripts/Battle/BattleManager.cs`
- **Change**: In `ExecuteSkill`, ensure `ShowEffectBriefly` is called regardless of whether `ProceduralSlash` exists, so that children GameObjects are visible.

```csharp
// Current Logic
if (ps != null) ps.PlaySlash(enemyPos.position, skill.effectColor);
else StartCoroutine(ShowEffectBriefly(strikeObj, 0.3f));

// Proposed Logic
StartCoroutine(ShowEffectBriefly(strikeObj, 0.5f)); // Use a slightly longer duration for animations
if (ps != null) ps.PlaySlash(enemyPos.position, skill.effectColor);
```

## 2. (Optional) Cleanup Procedural Components
If the user prefers their own animations over my "line" effect, I will suggest removing the `ProceduralSlash` component from the `Strike_1-3` objects.

# Verification & Testing
1. Perform "Wilde Schläge" in-game.
2. Verify that the user's "SchlagAnimation" children appear for each strike.
3. Verify that the timing feels right (0.5s duration).

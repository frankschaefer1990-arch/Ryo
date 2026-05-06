using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Battle/Skill")]
public class BattleSkill : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public int hitCount = 1;
    public float damageMultiplier = 1f;
    public bool isSpell = false;
    public int manaCost = 0;
    public Color effectColor = Color.white;
    public AudioClip skillSound;
    
    // For QTE
    public bool hasCombo = false;
}

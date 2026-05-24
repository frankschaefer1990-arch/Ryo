using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHP;
    public int attack;
    public int defense;
    public int xpReward;
    public int startHP; 
    public int maxMana = 100;
    public int startMana = 100;
    public bool isBoss;
public Sprite enemySprite;
    public AudioClip attackSound;
    public System.Collections.Generic.List<BattleSkill> skills;
    }

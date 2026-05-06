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
    public bool isBoss;
    public Sprite enemySprite;
    public AudioClip attackSound;
    }

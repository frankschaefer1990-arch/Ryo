using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Main Quest Progress")]
    public bool introSeen = false;
    public bool visitedTemple = false;
    public bool defeatedTempleBoss = false;

    [Header("Battle Setup")]
    public EnemyData nextBattleEnemy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
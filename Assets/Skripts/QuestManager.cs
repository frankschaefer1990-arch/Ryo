using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Main Quest Progress")]
    public bool introSeen = false;
    public bool visitedTemple = false;
    public bool defeatedTempleBoss = false;
    public bool finishedTempleSequence = false;

    public void SetQuestData(bool intro, bool visited, bool defeated, bool finished)
    {
        introSeen = intro;
        visitedTemple = visited;
        defeatedTempleBoss = defeated;
        finishedTempleSequence = finished;
    }

    [Header("Battle Setup")]
    public EnemyData nextBattleEnemy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
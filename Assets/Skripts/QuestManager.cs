using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Main Quest Progress")]
    public bool introSeen = false;
    public bool visitedTemple = false;
    public bool defeatedTempleBoss = false;
    public bool finishedTempleSequence = false;
    public bool labyrinthDialogueSeen = false;
    public bool masterHouseMessageSeen = false;

    [Header("Krypta Quest")]
    public bool kryptaIntroSeen = false;
    public bool zombie1Defeated = false;
    public bool zombie2Defeated = false;
    public bool kryptaBossDefeated = false;
    public bool defeatedKryptaBossReturn = false;
    
    [Header("Waterfall Puzzle")]
    public bool waterfallPuzzleSolved = false;
    public bool[] waterfallLevers = new bool[4];

    public void SetQuestData(bool intro, bool visited, bool defeated, bool finished, bool labyrinth, bool houseMsg, bool kIntro = false, bool z1 = false, bool z2 = false, bool kBoss = false, bool wfSolved = false)
    {
        introSeen = intro;
        visitedTemple = visited;
        defeatedTempleBoss = defeated;
        finishedTempleSequence = finished;
        labyrinthDialogueSeen = labyrinth;
        masterHouseMessageSeen = houseMsg;
        kryptaIntroSeen = kIntro;
        zombie1Defeated = z1;
        zombie2Defeated = z2;
        kryptaBossDefeated = kBoss;
        waterfallPuzzleSolved = wfSolved;
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
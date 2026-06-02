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
    public bool schleierpfadIntroSeen = false;

    [Header("Krypta Quest")]
    public bool kryptaIntroSeen = false;
    public bool zombie1Defeated = false;
    public bool zombie2Defeated = false;
    public bool kryptaBossDefeated = false;
    public bool defeatedKryptaBossReturn = false;
    
    [Header("Waterfall Puzzle")]
    public bool waterfallPuzzleSolved = false;
    public bool waterfallPuzzle2Solved = false;
    public bool[] waterfallLevers = new bool[4];
    public bool defeatedWassergeist = false;
    public bool returningFromWassergeist = false;
    public bool level2IntroSeen = false;
    public bool villageFreeMessageSeen = false;

    [Header("Persistence")]
    public System.Collections.Generic.List<string> openedChests = new System.Collections.Generic.List<string>();
    public int[] echoLevels = new int[9]; // store highest unlocked level for each of the 9 echo slots

    public void SetQuestData(bool intro, bool visited, bool defeated, bool finished, bool labyrinth, bool houseMsg, bool kIntro = false, bool z1 = false, bool z2 = false, bool kBoss = false, bool wfSolved = false, bool wDefeated = false, bool wf2Solved = false, bool wReturn = false, bool l2Intro = false, bool vFreeMsg = false, bool sIntro = false)
    {
        introSeen = intro;
        visitedTemple = visited;
        defeatedTempleBoss = defeated;
        finishedTempleSequence = finished;
        labyrinthDialogueSeen = labyrinth;
        masterHouseMessageSeen = houseMsg;
        schleierpfadIntroSeen = sIntro;
        kryptaIntroSeen = kIntro;
        zombie1Defeated = z1;
        zombie2Defeated = z2;
        kryptaBossDefeated = kBoss;
        waterfallPuzzleSolved = wfSolved;
        defeatedWassergeist = wDefeated;
        waterfallPuzzle2Solved = wf2Solved;
        returningFromWassergeist = wReturn;
        level2IntroSeen = l2Intro;
        villageFreeMessageSeen = vFreeMsg;

        // Initialize echoLevels if they are all 0 (new game or first time)
        bool allZero = true;
        foreach (int i in echoLevels) if (i != 0) { allZero = false; break; }
        if (allZero) {
            for (int i = 0; i < echoLevels.Length; i++) echoLevels[i] = 1;
        }
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

            // Ensure levels start at 1
            if (echoLevels == null || echoLevels.Length == 0) echoLevels = new int[9];
            for (int i = 0; i < echoLevels.Length; i++)
            {
                if (echoLevels[i] == 0) echoLevels[i] = 1;
            }
        }
        else if (Instance != this)
{
            Destroy(gameObject);
        }
    }
}
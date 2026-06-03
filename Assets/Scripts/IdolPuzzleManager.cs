using UnityEngine;
using UnityEngine.Events;

public class IdolPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public struct DirectionTarget
    {
        public StoneIdol idol;
        public StoneIdol.Direction requiredDirection;
    }

    public DirectionTarget[] targets;
    public WaterfallMaster waterfallMaster; 
    public Sprite interactionPortrait;
    public AudioClip solveSound;
    public UnityEvent OnPuzzleSolved;
    
    private AudioSource audioSource;
    private bool isSolved = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Check persistent state if QuestManager exists (using flag 2 for Level 2)
        if (QuestManager.Instance != null && QuestManager.Instance.waterfallPuzzle2Solved)
        {
            isSolved = true;
            ApplySolvedState(true);
        }

        // Show collapse dialogue after teleport
        if (QuestManager.Instance != null && QuestManager.Instance.defeatedWassergeist && !QuestManager.Instance.bossChamberCollapsedDialogueSeen)
        {
            StartCoroutine(ShowCollapsedDialogue());
        }
    }

    private System.Collections.IEnumerator ShowCollapsedDialogue()
    {
        yield return new WaitForSeconds(1.0f); // Wait for scene to settle
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Die Boss Kammer ist eingestürzt.", interactionPortrait);
            QuestManager.Instance.bossChamberCollapsedDialogueSeen = true;
        }
    }

    public void CheckPuzzle()
    {
        if (isSolved) return;

        bool allCorrect = true;
        for (int i = 0; i < targets.Length; i++)
        {
            var target = targets[i];
            if (target.idol != null)
            {
                bool match = target.idol.currentDirection == target.requiredDirection;
                if (!match)
                {
                    allCorrect = false;
                    Debug.Log("[Puzzle] Statue " + target.idol.name + " is still facing " + target.idol.currentDirection + " but needs " + target.requiredDirection);
                }
            }
        }

        if (allCorrect)
        {
            isSolved = true;
            Debug.Log("[Puzzle] ALL CORRECT! Idol Puzzle Solved!");
            
            if (QuestManager.Instance != null)
                QuestManager.Instance.waterfallPuzzle2Solved = true;

            if (audioSource != null && solveSound != null)
                audioSource.PlayOneShot(solveSound, 1.5f);

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Ryo", "Der Wasserfall ist verschwunden... Das muss der Weg sein.", interactionPortrait, 1.8f);
            }

            ApplySolvedState(false);
            OnPuzzleSolved.Invoke();
        }
    }

    private void ApplySolvedState(bool immediate)
    {
        // Find all objects named IdolPuzzleWall and deactivate them
        GameObject[] extraWalls = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var wall in extraWalls)
        {
            if (wall.name == "IdolPuzzleWall") wall.SetActive(false);
        }
        
        if (waterfallMaster != null)
        {
            waterfallMaster.SetSolved(true);
        }
    }
}

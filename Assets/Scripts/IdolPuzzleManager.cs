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
    public GameObject wallToDeactivate;
    public WaterfallMaster waterfallMaster; 
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
                DialogueUI.Instance.ShowMessage("Ryo", "Der Wasserfall ist verschwunden... Das muss der Weg sein.", 3.5f);
            }

            ApplySolvedState(false);
            OnPuzzleSolved.Invoke();
        }
    }

    private void ApplySolvedState(bool immediate)
    {
        if (wallToDeactivate != null) wallToDeactivate.SetActive(false);
        
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

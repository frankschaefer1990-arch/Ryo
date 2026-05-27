using UnityEngine;
using UnityEngine.Events;

public class IdolPuzzleManager : MonoBehaviour
{
    public DirectionTarget[] targets;
    public GameObject wallToDeactivate;
    public WaterfallMaster waterfallMaster; // Reference to master for fading
    public UnityEvent OnPuzzleSolved;
    
    [System.Serializable]
    public struct DirectionTarget
    {
        public StoneIdol idol;
        public StoneIdol.Direction requiredDirection;
    }

    private bool isSolved = false;

    private void Start()
    {
        // Check persistent state if QuestManager exists
        if (QuestManager.Instance != null && QuestManager.Instance.waterfallPuzzleSolved)
        {
            isSolved = true;
            ApplySolvedState(true);
        }
    }

    public void CheckPuzzle()
    {
        if (isSolved) return;

        bool allCorrect = true;
        foreach (var target in targets)
        {
            if (target.idol != null && target.idol.currentDirection != target.requiredDirection)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            isSolved = true;
            Debug.Log("Idol Puzzle Solved!");
            
            if (QuestManager.Instance != null)
                QuestManager.Instance.waterfallPuzzleSolved = true;

            ApplySolvedState(false);
            OnPuzzleSolved.Invoke();
        }
    }

    private void ApplySolvedState(bool immediate)
    {
        if (wallToDeactivate != null) wallToDeactivate.SetActive(false);
        
        if (waterfallMaster != null)
        {
            waterfallMaster.SetSolved(true);
        }
    }
}

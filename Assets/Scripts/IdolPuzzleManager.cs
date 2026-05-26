using UnityEngine;
using UnityEngine.Events;

public class IdolPuzzleManager : MonoBehaviour
{
    public DirectionTarget[] targets;
    public GameObject wallToDeactivate;
    public UnityEvent OnPuzzleSolved;
    
    [System.Serializable]
    public struct DirectionTarget
    {
        public StoneIdol idol;
        public StoneIdol.Direction requiredDirection;
    }

    private bool isSolved = false;

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
            Debug.Log("Puzzle Solved!");
            if (wallToDeactivate != null) wallToDeactivate.SetActive(false);
            OnPuzzleSolved.Invoke();
        }
    }
}

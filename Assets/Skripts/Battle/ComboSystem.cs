using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    private KeyCode[] possibleKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R };
    private bool isWaitingForInput = false;
    private KeyCode currentTargetKey;
    private float timer = 0f;
    private float timeLimit = 1.5f; // Increased from 0.8f for easier reaction

    private void Awake()
    {
        Instance = this;
    }

    public void StartQTE(System.Action<bool> onResult)
    {
        StartCoroutine(QTERoutine(onResult));
    }

    private IEnumerator QTERoutine(System.Action<bool> onResult)
    {
        currentTargetKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        Debug.Log("QTE START: Press " + currentTargetKey);
        
        BattleUI.Instance.ShowComboPrompt(currentTargetKey.ToString());

        isWaitingForInput = true;
        timer = 0f;
        bool success = false;

        while (timer < timeLimit)
        {
            timer += Time.deltaTime;

            if (Input.GetKeyDown(currentTargetKey))
            {
                success = true;
                break;
            }
            else if (Input.anyKeyDown)
            {
                // Wrong key
                success = false;
                break;
            }

            yield return null;
        }

        BattleUI.Instance.HideComboPrompt();
        isWaitingForInput = false;
onResult?.Invoke(success);
    }
}

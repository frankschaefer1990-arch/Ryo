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
    private float timeLimit = 1.2f; // Increased for more reaction time

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

            // Animate Ring: Shrink from 3.0 down to 0.0 over time
            if (BattleUI.Instance.qteShrinkingRing != null)
            {
                float t = timer / timeLimit;
                float scale = Mathf.Lerp(3.0f, 0.0f, t);
                BattleUI.Instance.qteShrinkingRing.localScale = Vector3.one * scale;
            }

            if (Input.GetKeyDown(currentTargetKey))
            {
                // Success window: scale 1.0 is perfect.
                // Loosened significantly: 0.3 to 2.0. This allows early and late hits.
                float currentScale = BattleUI.Instance.qteShrinkingRing != null ? BattleUI.Instance.qteShrinkingRing.localScale.x : 1.0f;
                
                if (currentScale >= 0.3f && currentScale <= 2.2f)
                {
                    success = true;
                }
                else
                {
                    Debug.Log("QTE: Failed! Scale: " + currentScale);
                    success = false;
                }
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

        BattleUI.Instance.SetQTEFeedback(success);
        
        // Show the result color for a brief moment
        yield return new WaitForSeconds(0.3f);
        BattleUI.Instance.HideComboPrompt();

        isWaitingForInput = false;
        onResult?.Invoke(success);
    }
}

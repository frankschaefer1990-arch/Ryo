using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum QTEResult { FAIL, SUCCESS, PERFECT }

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    private KeyCode[] possibleKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R };
    private bool isWaitingForInput = false;
    private KeyCode currentTargetKey;
    private float timer = 0f;
    private float timeLimit = 1.2f; // Increased for more reaction time

    [Header("QTE Settings")]
    public AudioClip qteHintSound;
    [Range(0f, 1f)]
    public float qteVolume = 1.0f; // New volume control
    public float pauseDuration = 0.15f;
    public float perfectScale = 1.0f;
    private bool hintPlayed = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartQTE(System.Action<QTEResult> onResult, float customTimeLimit = -1f)
    {
        StartCoroutine(QTERoutine(onResult, customTimeLimit));
    }

    private IEnumerator QTERoutine(System.Action<QTEResult> onResult, float customTimeLimit)
    {
        float activeTimeLimit = customTimeLimit > 0 ? customTimeLimit : timeLimit;
        currentTargetKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        BattleUI.Instance.ShowComboPrompt(currentTargetKey.ToString());
        
        isWaitingForInput = true;
        timer = 0f;
        QTEResult result = QTEResult.FAIL;
        hintPlayed = false;
        bool hasSnapped = false;

        while (timer < activeTimeLimit)
        {
            float t = timer / activeTimeLimit;
            float scale = Mathf.Lerp(5.0f, 0.0f, t);

            // 1. Sound Hint
            if (!hintPlayed && scale <= 2.2f)
            {
                hintPlayed = true;
                if (qteHintSound != null && BattleManager.Instance != null && BattleManager.Instance.audioSource != null)
                {
                    BattleManager.Instance.audioSource.PlayOneShot(qteHintSound, qteVolume);
                }
            }

            // 2. Snapping Logic (Brief pause at perfect scale)
            if (!hasSnapped && scale <= perfectScale)
            {
                hasSnapped = true;
                // Snap visual
                BattleUI.Instance.qteShrinkRing.rectTransform.localScale = Vector3.one * perfectScale;
                
                // Pause for a moment
                float pauseTimer = 0;
                while (pauseTimer < pauseDuration)
                {
                    pauseTimer += Time.deltaTime;
                    if (Input.GetKeyDown(currentTargetKey))
                    {
                        result = QTEResult.PERFECT;
                        break;
                    }
                    yield return null;
                }
                if (result != QTEResult.FAIL) break;
            }

            // Update Visuals if not snapped or after snap
            if (BattleUI.Instance.qteShrinkRing != null)
                BattleUI.Instance.qteShrinkRing.rectTransform.localScale = Vector3.one * scale;

            if (Input.GetKeyDown(currentTargetKey))
            {
                // If scale is already smaller than perfectScale, it's too late for Success.
                if (scale > perfectScale)
                {
                    // Allow a small window before the snap
                    if (scale <= perfectScale + 0.5f) result = QTEResult.SUCCESS;
                    else result = QTEResult.FAIL;
                }
                else
                {
                    // Too late
                    result = QTEResult.FAIL;
                }
                break;
            }
            else if (Input.anyKeyDown && !Input.GetKeyDown(currentTargetKey))
            {
                result = QTEResult.FAIL;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        BattleUI.Instance.SetQTEFeedback(result != QTEResult.FAIL);
        yield return new WaitForSeconds(0.3f);
        BattleUI.Instance.HideComboPrompt();
        isWaitingForInput = false;
        onResult?.Invoke(result);
    }
    }

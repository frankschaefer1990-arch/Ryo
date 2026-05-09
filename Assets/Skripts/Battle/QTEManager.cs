using UnityEngine;
using System.Collections;

/// <summary>
/// Zentrales Management für das Quick Time Event (Legend of Dragoon Style).
/// </summary>
public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance;

    [Header("Settings")]
    public float shrinkSpeed = 1.2f;
    public float startScale = 1.8f;
    public float targetScale = 1.0f;
    
    [Header("Timing Windows")]
    public float perfectThreshold = 0.08f; // Abweichung von 1.0
    public float goodThreshold = 0.15f;    // Etwas großzügiger

    private KeyCode[] possibleKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R };
    private KeyCode currentTargetKey;
    private bool isWaiting = false;

    // Events für das Combo-System
    public System.Action OnPerfect;
    public System.Action OnGood;
    public System.Action OnMiss;

    private void Awake()
    {
        Instance = this;
    }

    public void StartQTE(System.Action<bool> callback)
    {
        StopAllCoroutines();
        StartCoroutine(QTERoutine(callback));
    }

    private IEnumerator QTERoutine(System.Action<bool> callback)
    {
        isWaiting = true;
        currentTargetKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        
        // UI Initialisieren
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.ShowComboPrompt(currentTargetKey.ToString());
        }

        float currentScale = startScale;
        bool result = false;

        // Visual Controller informieren
        QTERingController ringVisual = GetComponentInChildren<QTERingController>();
        if (ringVisual != null) ringVisual.ResetRing(startScale);

        while (currentScale > 0.4f)
        {
            float dt = Time.deltaTime;
            
            // "Hook" Effekt: Kurz vor der 1.0 etwas langsamer werden
            float speedMultiplier = 1.0f;
            if (currentScale > 0.95f && currentScale < 1.15f)
            {
                speedMultiplier = 0.6f; 
            }

            currentScale -= dt * shrinkSpeed * speedMultiplier;

            if (ringVisual != null) ringVisual.UpdateScale(currentScale);

            // Input Check
            if (Input.GetKeyDown(currentTargetKey))
            {
                float diff = Mathf.Abs(currentScale - targetScale);

                if (diff <= perfectThreshold)
                {
                    OnPerfect?.Invoke();
                    result = true;
                    if (ringVisual != null) ringVisual.TriggerFlash(Color.white, true);
                }
                else if (diff <= goodThreshold)
                {
                    OnGood?.Invoke();
                    result = true;
                    if (ringVisual != null) ringVisual.TriggerFlash(new Color(1, 1, 1, 0.5f), false);
                }
                else
                {
                    OnMiss?.Invoke();
                    result = false;
                }
                break;
            }
            else if (Input.anyKeyDown)
            {
                // Falsche Taste
                OnMiss?.Invoke();
                result = false;
                break;
            }

            yield return null;
        }

        if (currentScale <= 0.4f && !result)
        {
            OnMiss?.Invoke();
            result = false;
        }

        // Kurze Pause für das visuelle Feedback
        yield return new WaitForSeconds(0.1f);
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.HideComboPrompt();
        }

        isWaiting = false;
        callback?.Invoke(result);
    }
}

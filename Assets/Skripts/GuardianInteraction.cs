using UnityEngine;
using System.Collections;

public class GuardianInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3.5f;
public KeyCode interactKey = KeyCode.R;

    [Header("Dialogue Content")]
    public string guardianName = "Echohüter";
    public string playerName = "Ryo";

    private bool isInteracting = false;
    private Transform player;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.transform;
        }
    }

    private void Update()
    {
        if (isInteracting) return;

        // Use overlap circle to detect player, matching the tighter prompt radius
        float interactionRadius = 1.8f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        
        bool foundPlayer = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.GetComponentInParent<PlayerMovement>() != null)
            {
                foundPlayer = true;
                break;
            }
        }

        if (foundPlayer && Input.GetKeyDown(interactKey))
        {
            if (DialogueUI.Instance == null || !DialogueUI.Instance.IsDialogueActive())
            {
                StartCoroutine(StartDialogueSequence());
            }
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        isInteracting = true;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;

        // Lock player movement
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.canMove = false;

        // Sequence (Shortened as requested)
        yield return ShowLine(playerName, "Was ist das für ein Ort...?", 1.5f);
        yield return ShowLine(guardianName, "Die Halle der Echos.", 1.5f);
        yield return ShowLine(guardianName, "Ich bin ihr Hüter. Shinigami kamen einst her, um zu lernen.", 2.2f);
        yield return ShowLine(playerName, "Zu lernen?", 1.2f);
        yield return ShowLine(guardianName, "Besiegte Feind lassen Spuren zurück – Echos ihrer Kraft und Fehler.", 2.2f);

        yield return ShowLine(playerName, "Und diese Plätze?", 1.5f);
        yield return ShowLine(guardianName, "Echo-Statuen. Jede bewahrt die Erinnerung eines bezwungenen Bosses.", 2.5f);
        yield return ShowLine(guardianName, "Besiege einen mächtigen Gegner, und sein Echo erscheint hier.", 2.5f);

        yield return ShowLine(playerName, "Das hier ist also der Skelettkrieger?", 1.8f);
        yield return ShowLine(guardianName, "Dein erstes Echo.", 1.5f);

        // --- Kamera-Schwenk zum Skelettkrieger ---
        CameraFollow cam = Object.FindAnyObjectByType<CameraFollow>();
        GameObject statue = GameObject.Find("EchoStatue_0");
        float originalSmoothTime = 0.15f;
        if (cam != null) originalSmoothTime = cam.smoothTime;

        if (cam != null && statue != null)
        {
            cam.targetOverride = statue.transform;
            cam.smoothTime = 1.0f;
        }

        yield return ShowLine(guardianName, "Dem Skelettkrieger.", 1.5f);
        yield return new WaitForSeconds(0.7f);

        // Zurück zu Ryo
        if (cam != null)
        {
            cam.targetOverride = null;
            StartCoroutine(ResetCameraSmoothness(cam, originalSmoothTime, 1.5f));
        }

        yield return ShowLine(guardianName, "Sein Körper ist fort, doch sein Geist besteht.", 1.8f);
        yield return ShowLine(playerName, "Ich kann gegen diese Erinnerungen kämpfen?", 1.8f);
        yield return ShowLine(guardianName, "Dafür ist die Halle da. Um Feinde zu studieren und dich vorzubereiten.", 2.5f);
        yield return ShowLine(guardianName, "Die Weisen trainierten, die Narren starben.", 1.8f);
        yield return ShowLine(guardianName, "Dein Echo wartet. Berühre die Statue und lerne.", 2.2f);

        // Unlock player movement and UI
        if (pm != null) pm.canMove = true;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        isInteracting = false;
    }

    private IEnumerator ResetCameraSmoothness(CameraFollow cam, float targetValue, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cam != null) cam.smoothTime = targetValue;
    }

    private IEnumerator ShowLine(string speaker, string message, float duration)
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speaker, message, duration);
            // Wait for the message to be displayed (typewriter) + duration
            // DialogueUI.ShowMessage already handles the duration and HideAll internally,
            // but we need to wait here so the next message doesn't overlap or skip.
            // DialogueUI.Instance.IsDialogueActive() returns true while showing.
            
            // Wait until it starts showing
            yield return new WaitUntil(() => DialogueUI.Instance.IsDialogueActive());
            // Wait until it finishes
            yield return new WaitWhile(() => DialogueUI.Instance.IsDialogueActive());
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
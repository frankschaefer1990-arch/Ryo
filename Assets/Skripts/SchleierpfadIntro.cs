using UnityEngine;
using System.Collections;

public class SchleierpfadIntro : MonoBehaviour
{
    [Header("Settings")]
    public float initialDelay = 1.0f;
    public float secondLineTriggerDistance = 5.0f;
    public string playerName = "Ryo";

    private bool firstLineDone = false;
    private bool secondLineDone = false;
    private Vector3 startPosition;
    private Transform player;

    private void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.schleierpfadIntroSeen)
        {
            gameObject.SetActive(false);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.transform;
            startPosition = player.position;
        }

        StartCoroutine(TriggerFirstLine());
    }

    private IEnumerator TriggerFirstLine()
    {
        yield return new WaitForSeconds(initialDelay);
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(playerName, "Dieser Ort fühlt sich... seltsam an.", 1.8f);
        }

        firstLineDone = true;
    }

    private void Update()
    {
        if (!firstLineDone || secondLineDone) return;

        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                player = GameManager.Instance.player.transform;
            return;
        }

        float distanceMoved = Vector3.Distance(player.position, startPosition);
        if (distanceMoved >= secondLineTriggerDistance)
        {
            TriggerSecondLine();
        }
    }

    private void TriggerSecondLine()
    {
        secondLineDone = true;
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(playerName, "Als würde mich etwas rufen.", 1.8f);
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.schleierpfadIntroSeen = true;
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.Save();
        }
    }

    // Optional: Trigger-based second line if distance isn't preferred
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!firstLineDone || secondLineDone) return;
        if (other.CompareTag("Player"))
        {
            TriggerSecondLine();
        }
    }
}
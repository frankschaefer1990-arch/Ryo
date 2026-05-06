using UnityEngine;
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    public Transform templeEntrance;
    public AudioClip hollowScreem;
    public float panSpeed = 2f;

    private void Start()
    {
        if (QuestManager.Instance != null && !QuestManager.Instance.introSeen)
        {
            StartCoroutine(PlayIntro());
        }
    }

    private IEnumerator PlayIntro()
    {
        // Lock player
        PlayerMovement playerMove = FindFirstObjectByType<PlayerMovement>();
        if (playerMove != null) playerMove.canMove = false;

        // Eye blink effect
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.PlayRealisticBlink());
        }

        yield return new WaitForSeconds(0.5f);

        // Camera Pan to Temple
        CameraFollow camFollow = FindFirstObjectByType<CameraFollow>();
        Transform playerTransform = null;
        if (camFollow != null)
        {
            playerTransform = camFollow.player;
            camFollow.enabled = false; // Take control of camera
            
            Vector3 startPos = camFollow.transform.position;
            Vector3 targetPos = new Vector3(templeEntrance.position.x, templeEntrance.position.y, startPos.z);

            // Pan to temple
            float elapsed = 0;
            float duration = 2.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                camFollow.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }

            // Play Screem
            if (hollowScreem != null)
            {
                AudioSource.PlayClipAtPoint(hollowScreem, templeEntrance.position);
            }
            yield return new WaitForSeconds(1.5f);

            // Pan back to player
            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                camFollow.transform.position = Vector3.Lerp(targetPos, new Vector3(playerTransform.position.x, playerTransform.position.y, startPos.z), elapsed / duration);
                yield return null;
            }

            camFollow.enabled = true;
        }

        // Dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Was war das???");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        // Finish
        if (QuestManager.Instance != null) QuestManager.Instance.introSeen = true;
        if (playerMove != null) playerMove.canMove = true;
    }
}

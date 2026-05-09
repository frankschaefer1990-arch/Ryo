using UnityEngine;
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    public Transform templeEntrance;
    public AudioClip hollowScreem;
    [Range(0f, 1f)] public float screamVolume = 1.0f;
    public float panSpeed = 2f;

    private void Start()
    {
        if (QuestManager.Instance != null && !QuestManager.Instance.introSeen)
        {
            StartCoroutine(PlayIntro());
        }
        else
        {
            // Ensure CinemachineBrain is disabled if intro is skipped
            var brain = Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = false;
        }
    }

    private IEnumerator PlayIntro()
    {
        // 1. Lock UI first and close everything
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.CloseAllPanels();
            MyUIManager.Instance.isLocked = true;
        }

        // 2. Wait for the CORRECT player from GameManager
        GameObject playerObj = null;
        float timeout = 3.0f;
        while (playerObj == null && timeout > 0)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                playerObj = GameManager.Instance.player;
            else
                playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj == null)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }
        }

        PlayerMovement playerMove = null;
        if (playerObj != null)
        {
            playerMove = playerObj.GetComponent<PlayerMovement>();
            if (playerMove != null) playerMove.canMove = false;
        }
        else
        {
            Debug.LogWarning("IntroCutscene: Player not found!");
        }

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

            // Play Screem - Loudest possible intensity
            if (hollowScreem != null)
            {
                // Play twice at listener for max impact
                AudioSource.PlayClipAtPoint(hollowScreem, Camera.main.transform.position, screamVolume);
                AudioSource.PlayClipAtPoint(hollowScreem, Camera.main.transform.position, screamVolume);
            }
            yield return new WaitForSeconds(1.5f);

            // Pan back to player
            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (playerTransform != null && playerTransform.gameObject != null)
                {
                    camFollow.transform.position = Vector3.Lerp(targetPos, new Vector3(playerTransform.position.x, playerTransform.position.y, startPos.z), elapsed / duration);
                }
                else {
                    // Try to recover player from GameManager if reference was lost
                    if (GameManager.Instance != null && GameManager.Instance.player != null)
                        playerTransform = GameManager.Instance.player.transform;
                }
                yield return null;
            }

            camFollow.enabled = true;
            
            // Disable CinemachineBrain to let CameraFollow take over
            var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = false;
            }

            // Dialogue
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Ryo", "Was war das?");
                while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            }

        // Finish
        if (QuestManager.Instance != null) QuestManager.Instance.introSeen = true;
        if (playerMove != null) playerMove.canMove = true;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        }
        }

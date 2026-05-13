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
            var brain = Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = false;
        }
    }

    private IEnumerator PlayIntro()
    {
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.CloseAllPanels();
            MyUIManager.Instance.isLocked = true;
        }

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

        /*
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.PlayRealisticBlink());
        }

        yield return new WaitForSeconds(0.5f);
        */

        CameraFollow camFollow = FindFirstObjectByType<CameraFollow>();
        Transform playerTransform = null;
        if (camFollow != null)
        {
            playerTransform = camFollow.player;
            camFollow.enabled = false; 
            
            Vector3 startPos = camFollow.transform.position;
            Vector3 targetPos = new Vector3(templeEntrance.position.x, templeEntrance.position.y, startPos.z);

            float elapsed = 0;
            float duration = 2.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                camFollow.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }

            if (hollowScreem != null)
            {
                AudioSource.PlayClipAtPoint(hollowScreem, Camera.main.transform.position, screamVolume);
                AudioSource.PlayClipAtPoint(hollowScreem, Camera.main.transform.position, screamVolume);
            }
            yield return new WaitForSeconds(1.5f);

            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (playerTransform != null && playerTransform.gameObject != null)
                {
                    camFollow.transform.position = Vector3.Lerp(targetPos, new Vector3(playerTransform.position.x, playerTransform.position.y, startPos.z), elapsed / duration);
                }
                else {
                    if (GameManager.Instance != null && GameManager.Instance.player != null)
                        playerTransform = GameManager.Instance.player.transform;
                }
                yield return null;
            }

            camFollow.enabled = true;
            var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = false;
        }

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Was war das?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        if (QuestManager.Instance != null) QuestManager.Instance.introSeen = true;
        if (playerMove != null) playerMove.canMove = true;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
    }
}

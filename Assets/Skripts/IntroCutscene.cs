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
        Debug.Log("IntroCutscene: PlayIntro started.");
        
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.CloseAllPanels();
            MyUIManager.Instance.isLocked = true;
        }

        GameObject playerObj = null;
        float waitTime = 0;
        // Wait for player to be spawned AND adopted by GameManager
        while (playerObj == null && waitTime < 5.0f)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                playerObj = GameManager.Instance.player;
            }

            if (playerObj == null)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
        }

        if (playerObj == null)
        {
            Debug.LogError("IntroCutscene: Player not found after 5 seconds!");
        }
        else
        {
            Debug.Log("IntroCutscene: Player ready: " + playerObj.name + " at " + playerObj.transform.position);
            playerObj.SetActive(true);
            var sr = playerObj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) 
            {
                sr.enabled = true;
                sr.color = Color.white;
                sr.sortingOrder = 20; // Ensure he is on top
            }
        }

        PlayerMovement playerMove = playerObj?.GetComponent<PlayerMovement>();
        if (playerMove != null) playerMove.canMove = false;

        CameraFollow camFollow = FindAnyObjectByType<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.enabled = false; 
            
            Vector3 startPos = camFollow.transform.position;
            startPos.z = -10f; // Force correct Z
            
            Vector3 targetPos = (templeEntrance != null) ? new Vector3(templeEntrance.position.x, templeEntrance.position.y, -10f) : startPos;

            float elapsed = 0;
            float duration = 2.5f;
            Debug.Log($"IntroCutscene: Panning to {((templeEntrance != null) ? templeEntrance.name : "StartPos")} at {targetPos}");
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                camFollow.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }

            if (hollowScreem != null)
            {
                AudioSource.PlayClipAtPoint(hollowScreem, Camera.main.transform.position, screamVolume);
            }
            yield return new WaitForSeconds(1.5f);

            // PAN BACK TO PLAYER
            Debug.Log("IntroCutscene: Panning back to Player...");
            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // RE-FETCH PLAYER POSITION EVERY FRAME
                Vector3 playerPos = (GameManager.Instance != null && GameManager.Instance.player != null) 
                    ? GameManager.Instance.player.transform.position 
                    : Vector3.zero;
                
                Vector3 backTarget = new Vector3(playerPos.x, playerPos.y, -10f);
                camFollow.transform.position = Vector3.Lerp(targetPos, backTarget, elapsed / duration);
                yield return null;
            }

            camFollow.enabled = true;
            Debug.Log("IntroCutscene: Pan back complete. Camera snap to player.");
            
            var brain = Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
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
        
        if (playerObj != null)
        {
            Debug.Log($"IntroCutscene: Sequence finished. Player '{playerObj.name}' active: {playerObj.activeInHierarchy} at {playerObj.transform.position}");
        }
        else
        {
            Debug.LogWarning("IntroCutscene: Sequence finished but playerObj was lost!");
        }
        }
        }

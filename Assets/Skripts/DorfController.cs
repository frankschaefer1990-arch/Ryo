using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DorfController : MonoBehaviour
{
    public GameObject bgFlooded;
    public GameObject bgFree;
    public GameObject exitToCity;
    public GameObject exitToKreuzung;
    
    private void Start()
    {
        // Auto-assign references if missing
        if (bgFlooded == null || bgFree == null)
        {
            GameObject bgs = GameObject.Find("[Backgrounds]") ?? GameObject.Find("Backgrounds");
            if (bgs != null)
            {
                // Recursive search for children
                Transform tf = bgs.transform.Find("Background_Flooded");
                if (tf == null) tf = RecursiveFind(bgs.transform, "Background_Flooded");
                if (bgFlooded == null && tf != null) bgFlooded = tf.gameObject;

                tf = bgs.transform.Find("Background_Free");
                if (tf == null) tf = RecursiveFind(bgs.transform, "Background_Free");
                if (bgFree == null && tf != null) bgFree = tf.gameObject;
            }
        }
        
        // Final fallback: try finding by name in whole scene if still null
        if (bgFlooded == null) bgFlooded = GameObject.Find("Background_Flooded");
        if (bgFree == null) bgFree = GameObject.Find("Background_Free");

        bool isSolved = false;
        if (QuestManager.Instance != null)
        {
            isSolved = QuestManager.Instance.defeatedWassergeist;
        }

        if (bgFlooded != null) bgFlooded.SetActive(!isSolved);
        if (bgFree != null) bgFree.SetActive(isSolved);
        
        Debug.Log($"DorfController: isSolved={isSolved}, bgFlooded={(bgFlooded != null ? bgFlooded.activeSelf.ToString() : "null")}, bgFree={(bgFree != null ? bgFree.activeSelf.ToString() : "null")}");
        
        // One-time message when village is free
        if (isSolved && QuestManager.Instance != null && !QuestManager.Instance.villageFreeMessageSeen)
        {
            QuestManager.Instance.villageFreeMessageSeen = true;
            StartCoroutine(ShowVillageFreeMessage());
        }

        // Portals only active when free
        if (exitToCity != null) exitToCity.SetActive(isSolved);
        if (exitToKreuzung != null) exitToKreuzung.SetActive(isSolved);

        // Ensure camera follows Ryo immediately
        StartCoroutine(SetupCameraFollow());

        if (!isSolved)
        {
            StartCoroutine(FloodedCutscene());
        }
    }

    private IEnumerator ShowVillageFreeMessage()
    {
        yield return new WaitForSeconds(1.0f);
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Das Wasser hat sich zurückgezogen. Ich kann die Brücke passieren.", 1.0f);
}
    }

    private Transform RecursiveFind(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = RecursiveFind(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private IEnumerator SetupCameraFollow()
    {
        GameObject player = null;
        float timeout = 5f;
        while (player == null && timeout > 0)
        {
            player = GameObject.FindWithTag("Player") ?? GameObject.Find("Ryo") ?? GameObject.Find("Player");
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
            if (follow != null) follow.player = player.transform;
        }
    }

    private IEnumerator FloodedCutscene()
    {
        // 1. Wait for player to spawn
        GameObject player = null;
        float timeout = 5f;
        while (player == null && timeout > 0)
        {
            player = GameObject.FindWithTag("Player") ?? GameObject.Find("Ryo") ?? GameObject.Find("Player");
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (player == null) yield break;

        // Ensure camera follows Ryo immediately
        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
        if (follow != null) follow.player = player.transform;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        Animator anim = player.GetComponentInChildren<Animator>();

        // 2. Lock player and UI
        if (pm != null) pm.canMove = false;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;

        yield return new WaitForSeconds(1.0f);

        // 3. Dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Die Brücke ist überflutet... Ich kann nicht passieren.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // 4. Turn and Walk left
        if (pm != null && anim != null)
        {
            // Set this early so PlayerMovement doesn't overwrite our manual Animator settings
            pm.isCutsceneMoving = true;

            // First turn to Idle Left
            anim.SetBool("isMoving", false);
            anim.SetFloat("MoveX", -1f);
            anim.SetFloat("MoveY", 0f);
            pm.SetFacingDirection(Vector2.left);
            
            yield return new WaitForSeconds(0.8f); // Pause to show the turn

            // Then start walking
            anim.SetBool("isMoving", true);

            Vector3 startPos = player.transform.position;
            Vector3 targetPos = startPos + Vector3.left * 6.0f; // Walk a bit further left

            float walkDuration = 2.0f;
            float elapsed = 0;
            while (elapsed < walkDuration)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, pm.baseMoveSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            anim.SetBool("isMoving", false);
            pm.isCutsceneMoving = false;
        }

        if (pm != null) pm.canMove = true;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;

        // 5. Load Kreuzung
        if (GameManager.Instance != null)
        {
            GameManager.NextSpawnFacing = Vector2.left; // Face left when entering Kreuzung
            GameManager.Instance.LoadScene("Kreuzung", "SpawnFromDorf");
        }
else
        {
            SceneManager.LoadScene("Kreuzung");
        }
    }
}
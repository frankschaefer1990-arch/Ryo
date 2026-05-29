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
        bool isSolved = false;
        if (QuestManager.Instance != null)
        {
            isSolved = QuestManager.Instance.defeatedWassergeist;
        }

        if (bgFlooded != null) bgFlooded.SetActive(!isSolved);
        if (bgFree != null) bgFree.SetActive(isSolved);
        
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

        yield return new WaitForSeconds(0.5f);

        // 4. Walk left and exit
        if (pm != null && anim != null)
        {
            pm.isCutsceneMoving = true;
            anim.SetBool("isMoving", true);
            anim.SetFloat("MoveX", -1f);
            anim.SetFloat("MoveY", 0f);

            float walkDuration = 0.8f;
            float elapsed = 0;
            while (elapsed < walkDuration)
            {
                player.transform.Translate(Vector3.left * pm.baseMoveSpeed * Time.deltaTime);
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
            GameManager.Instance.LoadScene("Kreuzung", "SpawnFromDorf");
        }
        else
        {
            SceneManager.LoadScene("Kreuzung");
        }
    }
}
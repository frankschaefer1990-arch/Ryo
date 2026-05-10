using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TempleIntroController : MonoBehaviour
{
    public CinemachineCamera introCam;
    public CinemachineCamera playerCam;
    public float waitAtSkeleton = 1.5f; 
    
    [Header("Cutscene References")]
    public Transform skeleton;
    public EnemyData bossData;

    private bool hasStartedBattle = false;

    private void Awake()
{
        // Auto-assign if missing (Searching including inactive)
        if (introCam == null)
        {
            var cams = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cams) if (c.name == "CM_Intro") { introCam = c; break; }
        }
        if (playerCam == null)
        {
            var cams = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cams) if (c.name == "CM_Player") { playerCam = c; break; }
        }
        
        // Find skeleton if not assigned
        if (skeleton == null)
        {
            GameObject skelObj = GameObject.Find("Skelett Krieger");
            if (skelObj == null) skelObj = GameObject.Find("Skeleton");
            if (skelObj != null) skeleton = skelObj.transform;
        }

        // Disable conflicting legacy TempleCutscene
        var cutscene = GetComponent<TempleCutscene>();
        if (cutscene != null) cutscene.enabled = false;
    }

    private void Start()
    {
        // Enable CinemachineBrain for this cutscene
        var brain = Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null) brain.enabled = true;

        // Disable legacy scripts that fight for control
        Camera main = Camera.main;
        if (main != null)
        {
            var follow = main.GetComponent("CameraFollow");
            if (follow != null) (follow as MonoBehaviour).enabled = false;
            
            var intro = main.GetComponent("TempleCameraIntro");
            if (intro != null) (intro as MonoBehaviour).enabled = false;

            main.backgroundColor = Color.black;
            main.clearFlags = CameraClearFlags.SolidColor;
        }

        if (introCam == null || playerCam == null)
        {
            Debug.LogError("TempleIntroController: Cameras are not assigned!");
            return;
        }

        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        // Ensure background stays black
        Camera main = Camera.main;
        if (Time.timeSinceLevelLoad < 3f && main != null)
        {
            main.backgroundColor = Color.black;
            main.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private IEnumerator IntroSequence()
    {
        // 1. Lock Player Movement (search for player first)
        GameObject player = null;
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player;
        }

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("Ryo");
            if (player != null) break;
            yield return null; // Faster than 0.1s
        }

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) 
        {
            pm.canMove = false;
            pm.ResetMovementState(); // Force reset immediately
        }

        // 1.5 Lock UI panels
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.CloseAllPanels();
            MyUIManager.Instance.isLocked = true;
        }

        // 2. Focus Skeleton
introCam.Priority.Value = 30;
        playerCam.Priority.Value = 10;
        playerCam.Follow = player.transform; 

        // Wait at skeleton
        yield return new WaitForSeconds(waitAtSkeleton);

        // 3. Switch to Player (This triggers the Cinemachine pan)
        introCam.Priority.Value = 10;
        playerCam.Priority.Value = 30;

        // Wait for pan to finish (based on CinemachineBrain blend time)
        yield return new WaitForSeconds(2.0f);

        // 4. Dialogues
        DialogueUI di = null;
        float timeout = 3f;
        while (di == null && timeout > 0)
        {
            di = DialogueUI.Instance;
            if (di == null)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
        }

        if (di != null)
        {
            Debug.Log("TempleIntroController: Starting sequence.");
            
            // 1. Ryo: Meister!?
            di.ShowMessage("Ryo", "Meister!?");
            yield return new WaitForSeconds(0.3f); 
            float dTimeout = 5f;
            while (di.IsDialogueActive() && dTimeout > 0) { dTimeout -= Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.5f);

            // 2. Meister: Lauf Ryo...!
            di.ShowMessage("Meister", "Lauf Ryo...!");
            yield return new WaitForSeconds(0.3f);
            dTimeout = 5f;
            while (di.IsDialogueActive() && dTimeout > 0) { dTimeout -= Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.5f);

            // 3. Ryo: Der Meister hat ihn geschwächt...
            di.ShowMessage("Ryo", "Der Meister hat ihn geschwächt, jetzt ist meine Stunde!");
            yield return new WaitForSeconds(0.3f);
            dTimeout = 5f;
            while (di.IsDialogueActive() && dTimeout > 0) { dTimeout -= Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("TempleIntroController: First part of dialogue finished. Player walking.");

            // 4. Player walk to skeleton
            if (pm != null && skeleton != null)
            {
                float walkTime = 3.0f;
                float elapsed = 0;
                Vector3 startPos = pm.transform.position;
                Vector3 targetPos = skeleton.position + (startPos - skeleton.position).normalized * 1.5f;
                
                Animator anim = pm.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    anim.SetBool("isMoving", true);
                    Vector3 dir = (targetPos - startPos).normalized;
                    anim.SetFloat("MoveX", dir.x);
                    anim.SetFloat("MoveY", dir.y);
                    // Check if state exists would be better but try-catch or just Play is okay if handled by Unity
                    try { anim.Play("Walk Up"); } catch {}
                }

                while (elapsed < walkTime)
                {
                    elapsed += Time.deltaTime;
                    pm.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / walkTime);
                    yield return null;
                }

                if (anim != null) 
                {
                    anim.SetBool("isMoving", false);
                    anim.SetFloat("MoveY", 1);
                }
            }

            // 5. Skelett: Sirb du Wurm!
            di.ShowMessage("Skelettkrieger", "Sirb du Wurm!");
            yield return new WaitForSeconds(0.3f);
            dTimeout = 5f;
            while (di.IsDialogueActive() && dTimeout > 0) { dTimeout -= Time.deltaTime; yield return null; }
            
            Debug.Log("TempleIntroController: Dialogue sequence finished. Starting battle.");
            }
            else
            {
            Debug.LogError("TempleIntroController: DialogueUI konnte nicht gefunden werden!");
            }
 
            // 6. Transition to Battle
            if (hasStartedBattle) yield break;
            hasStartedBattle = true;

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.visitedTemple = true;
                if (bossData != null) 
                {
                    QuestManager.Instance.nextBattleEnemy = bossData;
                    Debug.Log($"TempleIntroController: Assigned boss data '{bossData.enemyName}' to QuestManager.");
                }
                else 
                {
                    Debug.LogWarning("TempleIntroController: bossData is NULL!");
                }
            }

        Debug.Log("TempleIntroController: Loading BattleScene...");
        if (GameManager.Instance != null) GameManager.Instance.LoadScene("BattleScene");
        else SceneManager.LoadScene("BattleScene");
        }
}
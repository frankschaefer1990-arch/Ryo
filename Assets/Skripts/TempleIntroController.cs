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
        if (Time.timeSinceLevelLoad < 3f && Camera.main != null)
        {
            Camera.main.backgroundColor = Color.black;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private IEnumerator IntroSequence()
    {
        // 1. Lock Player Movement (search for player first)
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("Ryo");
            if (player != null) break;
            yield return new WaitForSeconds(0.1f);
        }

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.canMove = false;

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
            
            // Erster Dialog: Ryo
            di.ShowMessage("Ryo", "Der Meister hat ihn geschwächt, jetzt ist meine Stunde!");
            yield return new WaitForSeconds(0.3f); 
            while (di.IsDialogueActive()) yield return null;
            yield return new WaitForSeconds(0.5f);

            // Zweiter Dialog: Skelett
            di.ShowMessage("Skelettkrieger", "Sirb die Made!");
            yield return new WaitForSeconds(0.3f);
            while (di.IsDialogueActive()) yield return null;
            
            Debug.Log("TempleIntroController: Dialogue sequence finished. Player should walk now.");
        }
        else
        {
            Debug.LogError("TempleIntroController: DialogueUI konnte nicht gefunden werden!");
        }

        // 5. Player walk to skeleton
        if (pm != null && skeleton != null)
        {
            Debug.Log("TempleIntroController: Ryo walking to skeleton.");
            float walkTime = 3.5f; // Etwas langsamer für die Animation
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
                
                // Falls die Parameter nicht reichen, erzwingen wir den Walk-State
                if (dir.y > 0.5f) anim.Play("Walk Up");
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
                anim.SetFloat("MoveX", 0);
                anim.SetFloat("MoveY", 1); // Stehen bleiben und nach oben schauen
            }
        }

        // 6. Transition to Battle
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.visitedTemple = true;
            if (bossData != null) QuestManager.Instance.nextBattleEnemy = bossData;
            Debug.Log($"TempleIntroController: Setting boss data: {bossData?.enemyName}");
        }
        
        Debug.Log("TempleIntroController: Loading BattleScene...");
        if (GameManager.Instance != null) GameManager.Instance.LoadScene("BattleScene");
        else SceneManager.LoadScene("BattleScene");
        }
}
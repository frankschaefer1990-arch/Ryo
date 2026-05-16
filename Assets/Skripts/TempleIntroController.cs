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
    public Transform meister;
    public GameObject soulBallObject; 
    public EnemyData bossData;
    public AudioClip templeMusic;

    private bool hasStartedBattle = false;

    private void Awake()
    {
        Debug.Log("TempleIntroController: Awake.");
        
        // Lock UI early to prevent triggers firing before cutscene starts
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();

        if (introCam == null) {
            var cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cams) if (c.name == "CM_Intro") { introCam = c; break; }
        }
        if (playerCam == null) {
            var cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cams) if (c.name == "CM_Player") { playerCam = c; break; }
        }
        
        FindSkeleton();

        if (meister == null) {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in all) {
                if ((obj.name == "Toter Meister" || obj.name == "Meister") && obj.scene.isLoaded) { 
                    meister = obj.transform; break; 
                }
            }
        }

        if (soulBallObject == null) {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in all) if (obj.name == "SoulBall" && obj.scene.isLoaded) { soulBallObject = obj; break; }
        }

        if (soulBallObject != null) {
            soulBallObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            soulBallObject.SetActive(false);
        }

        var cutscene = GetComponent<TempleCutscene>();
        if (cutscene != null) cutscene.enabled = false;
    }

    private void FindSkeleton()
    {
        if (skeleton != null && skeleton.gameObject != null) return;
        string[] possibleNames = { "Skelett Krieger", "Skeleton", "Enemy", "Boss" };
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (string n in possibleNames) {
            foreach (var obj in allObjects) {
                if (obj.name == n && obj.scene.isLoaded) { skeleton = obj.transform; return; }
            }
        }
        GameObject tagged = GameObject.FindGameObjectWithTag("Enemy");
        if (tagged != null) skeleton = tagged.transform;
    }

    private void Start()
    {
        Debug.Log("TempleIntroController: Start.");
        
        if (AudioManager.Instance != null && templeMusic != null) AudioManager.Instance.PlayOverrideMusic(templeMusic);
        
        var brain = Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null) brain.enabled = true;

        Camera main = Camera.main;
        if (main != null) {
            var follow = main.GetComponent("CameraFollow");
            if (follow != null) (follow as MonoBehaviour).enabled = false;
            var intro = main.GetComponent("TempleCameraIntro");
            if (intro != null) (intro as MonoBehaviour).enabled = false;
            main.backgroundColor = Color.black;
            main.clearFlags = CameraClearFlags.SolidColor;
        }

        if (soulBallObject != null) {
            soulBallObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            soulBallObject.SetActive(false);
        }

        if (QuestManager.Instance != null && QuestManager.Instance.defeatedTempleBoss && !QuestManager.Instance.finishedTempleSequence)
            StartCoroutine(PostBattleSequence());
        else if (QuestManager.Instance != null && !QuestManager.Instance.visitedTemple)
            StartCoroutine(IntroSequence());
        else {
            Debug.Log("TempleIntroController: Visit flags don't match sequence conditions. Enabling Free Play.");
            EnableFreePlay();
        }
    }

    private IEnumerator IntroSequence()
    {
        Debug.Log("TempleIntroController: IntroSequence started.");
        GameObject player = GetPlayer();
        while (player == null) { player = GetPlayer(); yield return null; }
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { pm.canMove = false; pm.ResetMovementState(); }

        if (MyUIManager.Instance != null) { MyUIManager.Instance.CloseAllPanels(); MyUIManager.Instance.isLocked = true; }
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();

        if (skeleton != null) { introCam.Follow = skeleton; introCam.LookAt = skeleton; }
        introCam.Priority.Value = 30; playerCam.Priority.Value = 10;
        playerCam.Follow = player.transform; playerCam.LookAt = player.transform;
        yield return new WaitForSeconds(waitAtSkeleton);
        introCam.Priority.Value = 5; playerCam.Priority.Value = 40; 
        yield return new WaitForSeconds(2.0f);

        DialogueUI di = DialogueUI.Instance;
        while (di == null) { di = DialogueUI.Instance; yield return null; }
        
        Debug.Log("TempleIntroController: Playing Intro Dialogues.");
        di.ShowMessage("Ryo", "Meister!?"); yield return WaitForDialogue(di);
        di.ShowMessage("Meister", "Lauf Ryo...!"); yield return WaitForDialogue(di);
        di.ShowMessage("Ryo", "Der Meister hat ihn geschwächt, jetzt ist meine Stunde!"); yield return WaitForDialogue(di);

        if (pm != null && skeleton != null) {
            Debug.Log("TempleIntroController: Ryo walking to skeleton.");
            yield return StartCoroutine(WalkToTarget(player, skeleton.position + (player.transform.position - skeleton.position).normalized * 1.5f));
        }

        di.ShowMessage("Skelettkrieger", "Stirb du Wurm!"); yield return WaitForDialogue(di);
        
        if (hasStartedBattle) yield break;
        hasStartedBattle = true;
        
        Debug.Log("TempleIntroController: Loading Battle Scene.");
        if (QuestManager.Instance != null) { QuestManager.Instance.visitedTemple = true; QuestManager.Instance.nextBattleEnemy = bossData; }
        if (GameManager.Instance != null) GameManager.Instance.LoadScene("BattleScene");
        else SceneManager.LoadScene("BattleScene");
    }

    private IEnumerator PostBattleSequence()
    {
        Debug.Log("TempleIntroController: PostBattleSequence started.");
        GameObject player = GetPlayer();
        while (player == null) { player = GetPlayer(); yield return null; }
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { pm.canMove = false; pm.ResetMovementState(); }

        if (MyUIManager.Instance != null) { MyUIManager.Instance.CloseAllPanels(); MyUIManager.Instance.isLocked = true; }
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();

        playerCam.Priority.Value = 40; playerCam.Follow = player.transform; playerCam.LookAt = player.transform;
        yield return new WaitForSeconds(1.0f);

        FindSkeleton();
        if (soulBallObject == null) {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in all) if (obj.name == "SoulBall" && obj.scene.isLoaded) { soulBallObject = obj; break; }
        }

        DialogueUI di = DialogueUI.Instance;
        while (di == null) { di = DialogueUI.Instance; yield return null; }

        di.ShowMessage("Ryo", "Ich... habe gewonnen...?", 1.5f); yield return WaitForDialogue(di);

        if (skeleton != null) {
            di.ShowMessage("Skelettkrieger", "Du... bist... der...", 2.5f); yield return WaitForDialogue(di);
            
            yield return StartCoroutine(FadeOutSkeleton(4.0f));
            
            if (soulBallObject != null) {
                soulBallObject.transform.SetParent(null);
                soulBallObject.transform.position = skeleton.position + Vector3.up;
                soulBallObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f); 
                soulBallObject.SetActive(true);
            }
            skeleton.gameObject.SetActive(false);
            
            di.ShowMessage("Ryo", "Was ist das?", 1.5f); yield return WaitForDialogue(di);
            
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(SoulAbsorptionEffect(player.transform));
            di.ShowMessage("System", "Seele von Skelettkrieger wurde verschlungen... Ryo wurde verflucht!", 2.5f); yield return WaitForDialogue(di);
            }

            di.ShowMessage("Stimme / ???", "Hunger...", 4.0f); yield return WaitForDialogue(di);
        di.ShowMessage("Ryo", "Wer ist da?!", 1.5f); yield return WaitForDialogue(di);

        if (pm != null && meister != null) {
            Vector3 targetPos = meister.position + Vector3.down * 1.5f;
            Vector3 intermediatePos = new Vector3(targetPos.x, player.transform.position.y, player.transform.position.z);
            yield return StartCoroutine(WalkToTarget(player, intermediatePos));
            yield return StartCoroutine(WalkToTarget(player, targetPos));
        }

        if (meister != null) {
            di.ShowMessage("Meister", "Sie suchen... den Seelenverschlinger...", 2.5f); yield return WaitForDialogue(di);
            di.ShowMessage("Meister", "Ryo... du... bi...", 2.5f); yield return WaitForDialogue(di);
            SpriteRenderer msr = meister.GetComponentInChildren<SpriteRenderer>();
            if (msr != null) msr.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        if (QuestManager.Instance != null) QuestManager.Instance.finishedTempleSequence = true;
        EnableFreePlay();
    }

    private IEnumerator FadeOutSkeleton(float duration)
    {
        if (skeleton == null) yield break;
        SpriteRenderer[] renderers = skeleton.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers) { if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; } }
        float elapsed = 0;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            foreach (var sr in renderers) if (sr != null) { Color c = sr.color; c.a = alpha; sr.color = c; }
            yield return null;
        }
    }

    private void EnableFreePlay()
    {
        Debug.Log("TempleIntroController: Enabling Free Play.");
        GameObject player = GetPlayer();
        if (player != null) { PlayerMovement pm = player.GetComponent<PlayerMovement>(); if (pm != null) pm.canMove = true; }
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        var brain = Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>(FindObjectsInactive.Include);
        if (follow == null && Camera.main != null) follow = Camera.main.GetComponent<CameraFollow>();
        if (follow != null) {
            follow.enabled = true;
            if (player != null) {
                follow.player = player.transform;
                follow.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, follow.transform.position.z);
            }
            follow.UpdateBounds(); 
        }
        Camera c = Camera.main;
        if (c != null) { c.clearFlags = CameraClearFlags.Skybox; c.backgroundColor = Color.black; }
        if (skeleton != null && QuestManager.Instance != null && QuestManager.Instance.defeatedTempleBoss)
            skeleton.gameObject.SetActive(false);
    }

    private IEnumerator SoulAbsorptionEffect(Transform target)
    {
        GameObject ball = soulBallObject;
        if (ball == null) yield break;
        ball.SetActive(true);
        Vector3 startPos = ball.transform.position;
        Vector3 startScale = new Vector3(0.2f, 0.2f, 1f);
        Vector3 targetScale = startScale * 0.5f; 
        float duration = 2.0f; float elapsed = 0;
        while (elapsed < duration) {
            elapsed += Time.deltaTime; float t = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, target.position + Vector3.up, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f; 
            ball.transform.position = currentPos;
            ball.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        ball.SetActive(false);
    }

    private IEnumerator WalkToTarget(GameObject p, Vector3 targetPos)
    {
        float speed = 3.5f; Vector3 startPos = p.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        if (distance < 0.05f) yield break;
        float walkTime = distance / speed; float elapsed = 0;
        Animator anim = p.GetComponentInChildren<Animator>();
        if (anim != null) {
            anim.SetBool("isMoving", true);
            Vector3 dir = (targetPos - startPos).normalized;
            anim.SetFloat("MoveX", dir.x); anim.SetFloat("MoveY", dir.y);
        }
        while (elapsed < walkTime) {
            elapsed += Time.deltaTime;
            p.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / walkTime);
            yield return null;
        }
        p.transform.position = targetPos;
        if (anim != null) anim.SetBool("isMoving", false);
    }

    private IEnumerator WaitForDialogue(DialogueUI di)
    {
        yield return new WaitForSeconds(0.4f);
        float timeout = 12f;
        while (di.IsDialogueActive() && timeout > 0) { timeout -= Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
    }

    private GameObject GetPlayer()
    {
        GameObject p = null;
        if (GameManager.Instance != null && GameManager.Instance.player != null) p = GameManager.Instance.player;
        if (p == null) p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p == null) p = GameObject.Find("Ryo");
        return p;
    }
}

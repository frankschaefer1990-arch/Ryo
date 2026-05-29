using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class KryptaController : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineCamera introCam;
    public CinemachineCamera playerCam;

    [Header("Objects")]
    public Transform sargGross;
    public Transform sargKlein1;
    public Transform sargKlein2;
    public SpriteRenderer saule1;
    public SpriteRenderer saule2;

    [Header("Enemy Data")]
    public EnemyData zombieData;
    public EnemyData skeletonMageData;

    [Header("Audio")]
    public AudioClip heartbeatSFX;

    [Header("Sequence Objects")]
    public GameObject soulBallObject;
    public GameObject bossObject; // The visual representation in Krypta

    private bool isCutsceneRunning = false;

    private void Awake()
{
        if (QuestManager.Instance != null && !QuestManager.Instance.kryptaIntroSeen)
        {
            if (introCam != null) introCam.Priority.Value = 100;
            if (playerCam != null) playerCam.Priority.Value = 10;
        }

        if (bossObject != null) bossObject.SetActive(false);
        if (soulBallObject != null) soulBallObject.SetActive(false);
    }

    private void Start()
    {
        InitializeStates();

        if (QuestManager.Instance != null)
        {
            if (QuestManager.Instance.kryptaBossDefeated && QuestManager.Instance.zombie1Defeated && QuestManager.Instance.zombie2Defeated)
            {
                // Boss already defeated, just enable free play
                if (bossObject != null) bossObject.SetActive(false);
                EnableFreePlay();
            }
            else if (QuestManager.Instance.defeatedKryptaBossReturn && !QuestManager.Instance.kryptaBossDefeated)
            {
                // Returning from battle to play post-battle sequence
                StartCoroutine(PostBossBattleSequence());
            }
            else if (!QuestManager.Instance.kryptaIntroSeen)
            {
                StartCoroutine(PlayIntro());
            }
            else if (QuestManager.Instance.zombie1Defeated && QuestManager.Instance.zombie2Defeated && !QuestManager.Instance.kryptaBossDefeated)
            {
                // If both zombies are defeated but boss is not
                StartCoroutine(PlayBossCutscene());
            }
            else
            {
                EnableFreePlay();
            }
        }
        else
        {
            EnableFreePlay();
        }
    }

    private void InitializeStates()
    {
        // Setup Pillars based on QuestManager
        if (QuestManager.Instance != null)
        {
            if (saule1 != null) saule1.color = QuestManager.Instance.zombie1Defeated ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            if (saule2 != null) saule2.color = QuestManager.Instance.zombie2Defeated ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // Setup Interactors
        SetupInteractable(sargGross, OnInteractSargGross);
        SetupInteractable(sargKlein1, OnInteractSargKlein1);
        SetupInteractable(sargKlein2, OnInteractSargKlein2);
    }

    private void SetupInteractable(Transform t, System.Action action)
    {
        if (t == null) return;
        var interactable = t.GetComponent<KryptaInteractable>();
        if (interactable == null) interactable = t.gameObject.AddComponent<KryptaInteractable>();
        interactable.OnInteract = action;
    }

    private IEnumerator PlayIntro()
    {
        isCutsceneRunning = true;
        GameObject player = GetPlayer();
        while (player == null) { player = GetPlayer(); yield return null; }
        
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { 
            pm.canMove = false; 
            pm.ResetMovementState(); 
            // Force lastMovement to Down so Idle state persists
            var field = typeof(PlayerMovement).GetField("lastMovement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(pm, Vector2.down);
        }

        // Force Idle Down animation state
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null) {
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", -1);
            anim.SetBool("isMoving", false);
            // Re-sync animator to ensure it takes effect immediately
            anim.Update(0f);
        }

        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        
        // 1. Setup Camera Targets
        if (sargGross != null) { 
            introCam.Follow = sargGross; 
            introCam.LookAt = sargGross; 
        }
        playerCam.Follow = player.transform;
        playerCam.LookAt = player.transform;

        // Ensure Brain starts on introCam
        introCam.Priority.Value = 100;
        playerCam.Priority.Value = 10;
        
        var brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = true;

        // Brief yield to ensure Cinemachine has updated state before switching
        yield return new WaitForSeconds(0.1f);

        // 2. Dialogue while looking at Sarg
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Von diesem Sarg geht eine unheimliche Energie aus...");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        // 3. Pan to Ryo (Switch priorities)
        introCam.Priority.Value = 10;
        playerCam.Priority.Value = 50;

        // Wait for pan to finish (default blend is 2s)
        yield return new WaitForSeconds(2.5f);

        if (QuestManager.Instance != null) QuestManager.Instance.kryptaIntroSeen = true;
        isCutsceneRunning = false;
        EnableFreePlay();
    }

    private IEnumerator PlayBossCutscene()
    {
        isCutsceneRunning = true;
        GameObject player = GetPlayer();
        while (player == null) { player = GetPlayer(); yield return null; }
        
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { pm.canMove = false; pm.ResetMovementState(); }

        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        
        yield return new WaitForSeconds(1.0f);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Beide Säulen sind aktiviert... der große Sarg öffnet sich!");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        // 2. Spawn Skeleton Mage in the middle of Sarg Gross
        if (bossObject != null && sargGross != null)
        {
            bossObject.transform.position = sargGross.position;
            bossObject.SetActive(true);
        }

        // Walk to Sarg Gross in an L-shape (Increased distance to 2.8f to avoid collider overlap)
        if (sargGross != null)
        {
            Vector3 startPos = player.transform.position;
            Vector3 targetPos = sargGross.position + Vector3.down * 2.8f; 
            
            // 1. Move Down first
            Vector3 intermediatePos = new Vector3(startPos.x, targetPos.y, startPos.z);
            if (Vector3.Distance(startPos, intermediatePos) > 0.1f)
            {
                yield return StartCoroutine(WalkToTarget(player, intermediatePos));
            }

            // 2. Move Left (or Right) to target
            if (Vector3.Distance(intermediatePos, targetPos) > 0.1f)
            {
                yield return StartCoroutine(WalkToTarget(player, targetPos));
            }

            // Final look at boss (Up)
            if (pm != null)
            {
                var field = typeof(PlayerMovement).GetField("lastMovement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(pm, Vector2.up);
                
                Animator anim = player.GetComponentInChildren<Animator>();
                if (anim != null) {
                    anim.SetFloat("MoveX", 0);
                    anim.SetFloat("MoveY", 1);
                    anim.SetBool("isMoving", false);
                }
            }
            }

        yield return new WaitForSeconds(0.5f);
        
        StartBossBattle();
    }

    private IEnumerator PostBossBattleSequence()
    {
        isCutsceneRunning = true;
        GameObject player = GetPlayer();
        while (player == null) { player = GetPlayer(); yield return null; }
        
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { pm.canMove = false; pm.ResetMovementState(); }

        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        
        if (bossObject != null) 
        {
            bossObject.SetActive(true);
            bossObject.transform.position = sargGross.position;
        }

        yield return new WaitForSeconds(1.0f);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Skelett Magier", "Du Wicht! Wie konntest du nur gewinnen?!");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Skelett Magier", "Was passiert gerade...? Nein! Meine Seele!");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        // Fade out boss
        if (bossObject != null)
        {
            yield return StartCoroutine(FadeOutBoss(3.0f));
            
            if (soulBallObject != null)
            {
                soulBallObject.SetActive(true);
                soulBallObject.transform.position = bossObject.transform.position + Vector3.up;
                yield return new WaitForSeconds(1.0f);
                yield return StartCoroutine(SoulAbsorptionEffect(player.transform));
            }
            bossObject.SetActive(false);
        }

        // Heartbeats
        if (heartbeatSFX != null)
        {
            AudioSource audio = gameObject.AddComponent<AudioSource>();
            audio.clip = heartbeatSFX;
            audio.spatialBlend = 0f;
            audio.volume = 1.0f;
            
            for (int i = 0; i < 3; i++)
            {
                audio.PlayOneShot(heartbeatSFX, 1.5f);
                audio.PlayOneShot(heartbeatSFX, 1.5f);
                audio.PlayOneShot(heartbeatSFX, 1.5f); // Triple layering
                yield return new WaitForSeconds(1.2f);
            }
            
            Destroy(audio, 1.0f);
        }

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Stimme / ???", "Mehr Seelen!");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;

            DialogueUI.Instance.ShowMessage("Ryo", "Je mehr Seelen ich absorbiere, desto mehr merke ich, wie etwas Dunkles in mir wächst...", 4.5f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        if (QuestManager.Instance != null) 
        {
            QuestManager.Instance.kryptaBossDefeated = true;
            QuestManager.Instance.defeatedKryptaBossReturn = false;
        }
        
        isCutsceneRunning = false;
        EnableFreePlay();
    }

    private IEnumerator FadeOutBoss(float duration)
    {
        SpriteRenderer[] renderers = bossObject.GetComponentsInChildren<SpriteRenderer>(true);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            foreach (var sr in renderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            yield return null;
        }
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

    private void EnableFreePlay()
    {
        if (isCutsceneRunning) return;

        GameObject player = GetPlayer();
        if (player != null) {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.canMove = true;
        }
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        
        var brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        CameraFollow follow = FindFirstObjectByType<CameraFollow>();
        if (follow != null && player != null)
        {
            follow.enabled = true;
            follow.player = player.transform;
        }
    }

    private void OnInteractSargGross()
    {
        if (isCutsceneRunning) return;

        if (QuestManager.Instance != null && QuestManager.Instance.kryptaBossDefeated)
        {
            DialogueUI.Instance?.ShowMessage("Ryo", "Dieser Sarg ist nun still.");
            return;
        }

        if (QuestManager.Instance != null && QuestManager.Instance.zombie1Defeated && QuestManager.Instance.zombie2Defeated)
        {
            StartCoroutine(PlayBossCutscene());
        }
        else
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Ryo", "Dieser Sarg schreit förmlich nach Seelen...");
            }
        }
    }

    private void OnInteractSargKlein1()
    {
        if (isCutsceneRunning) return;
        if (QuestManager.Instance != null && QuestManager.Instance.zombie1Defeated)
        {
            DialogueUI.Instance?.ShowMessage("Ryo", "Dieser Sarg ist nun still.");
            return;
        }
        StartBattle(zombieData, 1);
    }

    private void OnInteractSargKlein2()
    {
        if (isCutsceneRunning) return;
        if (QuestManager.Instance != null && QuestManager.Instance.zombie2Defeated)
        {
            DialogueUI.Instance?.ShowMessage("Ryo", "Dieser Sarg ist nun still.");
            return;
        }
        StartBattle(zombieData, 2);
    }

    private void StartBattle(EnemyData enemy, int zombieIndex)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.nextBattleEnemy = enemy;
            PlayerPrefs.SetInt("LastZombieFight", zombieIndex);
            GameManager.Instance?.LoadScene("BattleScene");
        }
    }

    private void StartBossBattle()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.nextBattleEnemy = skeletonMageData;
            GameManager.Instance?.LoadScene("BattleScene");
        }
    }

    private GameObject GetPlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null) return GameManager.Instance.player;
        return GameObject.FindGameObjectWithTag("Player");
    }

    private IEnumerator WalkToTarget(GameObject p, Vector3 targetPos)
    {
        float speed = 3.0f;
        Vector3 startPos = p.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        if (distance < 0.05f) yield break;
        float walkTime = distance / speed;
        float elapsed = 0;
        
        PlayerMovement pm = p.GetComponent<PlayerMovement>();
        if (pm != null) pm.isCutsceneMoving = true;

        Animator anim = p.GetComponentInChildren<Animator>();
        Vector2 moveDir = Vector2.zero;

        if (anim != null || pm != null) {
            Vector3 diff = targetPos - startPos;
            // Strict cardinal animation
            if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x)) {
                moveDir = new Vector2(0, diff.y > 0 ? 1 : -1);
            } else {
                moveDir = new Vector2(diff.x > 0 ? 1 : -1, 0);
            }

            if (anim != null) {
                anim.SetBool("isMoving", true);
                anim.SetFloat("MoveX", moveDir.x);
                anim.SetFloat("MoveY", moveDir.y);
            }

            // Fix moonwalk: Update lastMovement in PlayerMovement via reflection
            if (pm != null) {
                var field = typeof(PlayerMovement).GetField("lastMovement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(pm, moveDir);
            }
        }

        while (elapsed < walkTime) {
            elapsed += Time.deltaTime;
            p.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / walkTime);
            yield return null;
        }
        p.transform.position = targetPos;
        if (anim != null) anim.SetBool("isMoving", false);
        if (pm != null) pm.isCutsceneMoving = false;
    }
}

using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class WassergeistBossController : MonoBehaviour
{
    public GameObject bossObject;
    public GameObject soulBallPrefab;
    public AudioClip heartbeatSFX;
    public AudioClip waterDrainSFX;
    
    private void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.returningFromWassergeist)
        {
            StartCoroutine(PostBattleSequence());
        }
    }

    private IEnumerator PostBattleSequence()
    {
        Debug.Log("[WassergeistBoss] Starting PostBattleSequence");
        QuestManager.Instance.returningFromWassergeist = false;
        
        GameObject player = null;
        while (player == null) {
            player = GameObject.FindWithTag("Player") ?? GameObject.Find("Ryo") ?? GameObject.Find("Player");
            if (player == null) Debug.Log("[WassergeistBoss] Waiting for player...");
            yield return null;
        }

        Debug.Log("[WassergeistBoss] Player found: " + player.name);
        
        // Move player to the exact spot where the fight was triggered
        if (GameManager.Instance != null && GameManager.Instance.lastGameplayPosition != Vector3.zero)
        {
            player.transform.position = GameManager.Instance.lastGameplayPosition;
            Debug.Log("[WassergeistBoss] Player moved to trigger position: " + player.transform.position);
        }

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) { pm.canMove = false; pm.ResetMovementState(); }

        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        
        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
        if (follow != null) {
            follow.player = player.transform;
            Debug.Log("[WassergeistBoss] Camera follow set to player");
        }

        if (bossObject != null) {
            bossObject.SetActive(true);
            Debug.Log("[WassergeistBoss] Boss object activated");
        }

        yield return new WaitForSeconds(1.0f);

        if (DialogueUI.Instance != null)
        {
            Debug.Log("[WassergeistBoss] Showing first dialogue");
            DialogueUI.Instance.ShowMessage("Wassergeist", "Ich habe... verloren...", 2.0f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Der Kampf ist vorbei.", 2.0f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Nein... Meine Zeit ist vorbei.", 2.0f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        Debug.Log("[WassergeistBoss] Starting Boss Fade");
        // Fade out boss
        if (bossObject != null)
        {
            SpriteRenderer[] renderers = bossObject.GetComponentsInChildren<SpriteRenderer>(true);
            float duration = 3.0f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                foreach (var sr in renderers) {
                    if (sr != null) {
                        Color c = sr.color;
                        c.a = alpha;
                        sr.color = c;
                    }
                }
                yield return null;
            }
            
            // Finalize fade
            foreach (var sr in renderers) if (sr != null) sr.enabled = false;
            
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Wassergeist", "Nun verstehe ich... weshalb ich deine Seele nicht erkennen konnte.", 2.5f);
                while (DialogueUI.Instance.IsDialogueActive()) yield return null;
                
                DialogueUI.Instance.ShowMessage("Ryo", "Wovon redest du?", 2.0f);
                while (DialogueUI.Instance.IsDialogueActive()) yield return null;
                
                DialogueUI.Instance.ShowMessage("Wassergeist", "Möge die Quelle dir vergeben...", 2.0f);
                while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            }

            Debug.Log("[WassergeistBoss] Fade complete. Spawning SoulBall.");
            // Spawn SoulBall AFTER fade
            if (soulBallPrefab != null)
            {
                GameObject ball = Instantiate(soulBallPrefab, bossObject.transform.position + Vector3.up, Quaternion.identity);
                ball.transform.localScale = new Vector3(0.2f, 0.2f, 1f); // Match Temple scale
                yield return StartCoroutine(SoulAbsorptionEffect(ball, player.transform));
                Destroy(ball);
            }
            bossObject.SetActive(false);
        }

        // Heartbeats (Using original sound with layering for volume)
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
                audio.PlayOneShot(heartbeatSFX, 1.5f); // Triple layering for maximum impact
                yield return new WaitForSeconds(1.2f);
            }
            Destroy(audio, 1.0f);
        }

        Debug.Log("[WassergeistBoss] Showing final dialogue");
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Stimme / ???", "Meeehr Seelen!", 2.5f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Ugh...!", 1.5f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        Debug.Log("[WassergeistBoss] Finishing sequence and loading level 2");
        QuestManager.Instance.defeatedWassergeist = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("Wasserfälle von Chlorix LvL 2", "ExitToBossraum");
        }
    }

    private IEnumerator SoulAbsorptionEffect(GameObject ball, Transform target)
    {
        Vector3 startPos = ball.transform.position;
        Vector3 startScale = ball.transform.localScale;
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
    }
}
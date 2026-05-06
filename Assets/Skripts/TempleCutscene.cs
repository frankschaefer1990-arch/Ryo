using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TempleCutscene : MonoBehaviour
{
    public Transform master;
    public Transform skeleton;
    public EnemyData bossData;

    private void Start()
    {
        if (QuestManager.Instance != null && !QuestManager.Instance.visitedTemple)
        {
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        PlayerMovement playerMove = FindFirstObjectByType<PlayerMovement>();
        if (playerMove != null) playerMove.canMove = false;

        CameraFollow camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.enabled = false;
            
            // Focus Skeleton
            yield return StartCoroutine(PanCamera(camFollow.transform, skeleton.position, 2.0f));
            yield return new WaitForSeconds(1.5f);

            // Focus Ryo (Slowly)
            yield return StartCoroutine(PanCamera(camFollow.transform, playerMove.transform.position, 2.0f));
        }

        // Dialogues
        DialogueUI di = DialogueUI.Instance;
        if (di != null)
        {
            Debug.Log("TempleCutscene: Starting dialogues.");
            
            di.ShowMessage("Ryo", "Meister…?!");
            yield return new WaitForSeconds(0.5f); 
            while (di.IsDialogueActive()) yield return null;
            yield return new WaitForSeconds(0.5f);

            di.ShowMessage("Meister", "…Lauf…");
            yield return new WaitForSeconds(0.5f);
            while (di.IsDialogueActive()) yield return null;
            yield return new WaitForSeconds(0.5f);

            di.ShowMessage("Skelettkrieger", "Noch ein Welpe des Ordens?");
            yield return new WaitForSeconds(0.5f);
            while (di.IsDialogueActive()) yield return null;
            yield return new WaitForSeconds(0.5f);

            di.ShowMessage("Ryo", "Der Meister hat ihn geschwächt, jetzt ist meine Stunde!");
            yield return new WaitForSeconds(0.5f);
            while (di.IsDialogueActive()) yield return null;
            yield return new WaitForSeconds(0.5f);

            di.ShowMessage("Skelettkrieger", "Hah… Stirb, du Made.");
            yield return new WaitForSeconds(0.5f);
            while (di.IsDialogueActive()) yield return null;
            
            Debug.Log("TempleCutscene: Dialogues finished.");
        }

        // Ryo walks towards skeleton (Slowly)
        if (playerMove != null && skeleton != null)
        {
            Debug.Log("TempleCutscene: Ryo walking to skeleton slowly.");
            float walkTime = 2.0f; // Slower walk
            float elapsed = 0;
            Vector3 startPos = playerMove.transform.position;
            Vector3 targetPos = skeleton.position + (startPos - skeleton.position).normalized * 1.2f;
            
            Animator anim = playerMove.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("isMoving", true);
                Vector3 dir = (targetPos - startPos).normalized;
                anim.SetFloat("MoveX", dir.x);
                anim.SetFloat("MoveY", dir.y);
            }

            while (elapsed < walkTime)
            {
                elapsed += Time.deltaTime;
                playerMove.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / walkTime);
                yield return null;
            }

            if (anim != null) anim.SetBool("isMoving", false);
        }

        // Transition to Battle
        Debug.Log("TempleCutscene: Starting battle transition.");
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.visitedTemple = true; // Mark that we reached this point
            QuestManager.Instance.nextBattleEnemy = bossData;
        }
        
        if (GameManager.Instance != null) GameManager.Instance.LoadScene("BattleScene");
        else SceneManager.LoadScene("BattleScene");
    }

    private IEnumerator PanCamera(Transform cam, Vector3 target, float duration)
    {
        Vector3 start = cam.position;
        Vector3 target3D = new Vector3(target.x, target.y, start.z);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.position = Vector3.Lerp(start, target3D, elapsed / duration);
            yield return null;
        }
    }
}

using UnityEngine;
using System.Collections;

public class LabyrinthEntryDialogue : MonoBehaviour
{
    void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.defeatedTempleBoss && !QuestManager.Instance.labyrinthDialogueSeen)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    IEnumerator ShowDialogue()
    {
        // Block movement immediately
        PlayerMovement pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = false;

        yield return new WaitForSeconds(0.5f);

        if (DialogueUI.Instance != null)
        {
            QuestManager.Instance.labyrinthDialogueSeen = true;
            string msg = "Ich werde dem Ganzen ein Ende bereiten und meinen Meister rächen!";
            float duration = 2.5f;
            DialogueUI.Instance.ShowMessage("Ryo", msg, duration);
            
            // Wait for typing + duration
            float waitTime = (msg.Length * 0.035f) + duration + 0.5f;
            yield return new WaitForSeconds(waitTime);
        }

        // Unblock movement
        if (pm != null) pm.canMove = true;
    }
}
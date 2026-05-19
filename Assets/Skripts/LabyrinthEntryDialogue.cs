using UnityEngine;
using System.Collections;

public class LabyrinthEntryDialogue : MonoBehaviour
{
    void Start()
    {
        if (QuestManager.Instance != null && !QuestManager.Instance.labyrinthDialogueSeen)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    IEnumerator ShowDialogue()
    {
        yield return new WaitForSeconds(1f);
        if (DialogueUI.Instance != null)
        {
            QuestManager.Instance.labyrinthDialogueSeen = true;
            DialogueUI.Instance.ShowMessage("Ryo", "Ich werde dem Ganzen ein Ende bereiten und meinen Meister rächen!");
        }
    }
}
using UnityEngine;
using System.Collections;

public class LabyrinthEntryDialogue : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ShowDialogue());
    }

    IEnumerator ShowDialogue()
    {
        yield return new WaitForSeconds(1f);
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Ich werde dem Ganzen ein Ende bereiten und meinen Meister rächen!");
        }
    }
}
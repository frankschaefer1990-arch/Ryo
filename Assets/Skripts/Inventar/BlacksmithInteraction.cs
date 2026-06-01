using UnityEngine;

public class BlacksmithInteraction : MonoBehaviour
{
    public string speakerName = "Schmied";
    public Sprite portrait;
    public string welcomeMessage = "Wenn du Verbesserungen benötigst, bin ich dein Mann!";
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.R;

    private Transform player;
    private bool isTalking = false;

    private void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= interactionRange)
        {
            if (Input.GetKeyDown(interactKey) && !isTalking)
            {
                StartCoroutine(TalkAndOpen());
            }
        }
    }

    private System.Collections.IEnumerator TalkAndOpen()
    {
        isTalking = true;
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speakerName, welcomeMessage, portrait, 1.5f);
            yield return new WaitForSeconds(1.6f);
        }

        if (BlacksmithManager.Instance != null)
        {
            BlacksmithManager.Instance.OpenPanel();
        }
        isTalking = false;
    }
}

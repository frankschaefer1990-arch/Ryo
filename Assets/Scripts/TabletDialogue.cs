using UnityEngine;

public class TabletDialogue : MonoBehaviour
{
    [TextArea(3, 5)]
    public string message;
    public float interactionDistance = 1.2f;
    
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player") ?? GameObject.Find("Ryo") ?? GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player") ?? GameObject.Find("Ryo") ?? GameObject.Find("Player");
            if (p != null) player = p.transform;
            return;
        }

        if (Vector2.Distance(transform.position, player.position) <= interactionDistance)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
                {
                    DialogueUI.Instance.ShowMessage("Steintafel", message, 4.0f);
                }
            }
        }
    }
}
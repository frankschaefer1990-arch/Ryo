using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] duplicates = GameObject.FindGameObjectsWithTag(gameObject.tag);

        foreach (GameObject obj in duplicates)
        {
            if (obj != gameObject && obj.name == gameObject.name)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
    }
}
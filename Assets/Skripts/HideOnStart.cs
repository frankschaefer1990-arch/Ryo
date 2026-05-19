using UnityEngine;

public class HideOnStart : MonoBehaviour
{
    void Awake()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.enabled = false;
        
        var tm = GetComponent<UnityEngine.Tilemaps.Tilemap>();
        if (tm != null)
        {
            Color c = tm.color;
            c.a = 0f;
            tm.color = c;
        }
    }
}
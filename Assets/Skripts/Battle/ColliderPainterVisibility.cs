using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class ColliderPainterVisibility : MonoBehaviour
{
    public Color editorColor = new Color(1, 0, 0, 0.4f);
    public bool visibleInGame = false;

    private Tilemap tilemap;

    private void OnEnable()
    {
        tilemap = GetComponent<Tilemap>();
        UpdateVisibility();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateVisibility();
        }
    }

    public void UpdateVisibility()
    {
        if (tilemap == null) tilemap = GetComponent<Tilemap>();
        if (tilemap == null) return;

        if (!Application.isPlaying)
        {
            tilemap.color = editorColor;
        }
        else
        {
            Color c = tilemap.color;
            c.a = visibleInGame ? editorColor.a : 0f;
            tilemap.color = c;
        }
    }

    private void OnValidate()
    {
        UpdateVisibility();
    }
}

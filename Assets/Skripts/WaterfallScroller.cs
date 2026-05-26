using UnityEngine;

public class WaterfallScroller : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    public string texturePropertyName = "_MainTex";
    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Use material (instance) at runtime
            mat = rend.material;
        }
    }

    void Update()
    {
        if (mat != null)
        {
            Vector2 offset = mat.GetTextureOffset(texturePropertyName);
            offset.y -= Time.deltaTime * scrollSpeed;
            mat.SetTextureOffset(texturePropertyName, offset);

            // Also try _BaseMap for URP compatibility
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTextureOffset("_BaseMap", offset);
            }
        }
    }
}
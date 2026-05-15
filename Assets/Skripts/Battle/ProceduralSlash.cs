using UnityEngine;
using System.Collections;

public class ProceduralSlash : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;
    public float duration = 0.2f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            
            Shader s = Shader.Find("Sprites/Default");
            if (s == null) s = Shader.Find("UI/Default");
            if (s == null) s = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (s != null) lineRenderer.material = new Material(s);
        }
        lineRenderer.enabled = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        // Force high visibility settings
        spriteRenderer.sortingOrder = 500; 
        spriteRenderer.enabled = false;
    }

    public void PlaySlash(Vector3 center, Color color, Sprite customSprite = null, float customDuration = -1f, Vector3 offset = default, Vector3 scale = default, bool randomRot = true)
    {
        float dur = customDuration > 0 ? customDuration : duration;
        
        // Stop any currently running slash animation to avoid overlapping alpha fades
        StopAllCoroutines();
        
        // Reset renderer states
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (lineRenderer != null) lineRenderer.enabled = false;

        if (customSprite != null)
        {
            Vector3 s = (scale == Vector3.zero || scale == Vector3.one) ? Vector3.one * 3f : scale;
            StartCoroutine(SpriteSlashRoutine(center, color, customSprite, dur, offset, s, randomRot));
        }
        else
        {
            StartCoroutine(SlashRoutine(center, color, dur));
        }
    }

    private IEnumerator SpriteSlashRoutine(Vector3 center, Color color, Sprite sprite, float dur, Vector3 offset, Vector3 scale, bool randomRot)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        
        // Use a very high sorting order to be sure it's on top
        spriteRenderer.sortingOrder = 1000;
        
        // Position it at the enemy center + manual offset
        // We force a Z coordinate that is likely in front of everything in a 2D scene
        Vector3 spawnPos = center + offset;
        spawnPos.z = -5f; // Bring it closer to camera
        
        transform.position = spawnPos;
        transform.localScale = scale;
        
        if (randomRot)
        {
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
        
        float elapsed = 0;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float alpha = 1.0f - (elapsed / dur);
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        spriteRenderer.enabled = false;
    }

    private IEnumerator SlashRoutine(Vector3 center, Color color, float dur)
    {
        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = new Color(color.r, color.g, color.b, 0);

        // Random slash direction
        Vector3 offset = Random.insideUnitCircle.normalized * 1.5f;
        lineRenderer.SetPosition(0, center - offset);
        lineRenderer.SetPosition(1, center + offset);

        yield return new WaitForSeconds(dur);
        lineRenderer.enabled = false;
    }
}

using UnityEngine;
using System.Collections;

public class ProceduralSlash : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float duration = 0.2f;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.enabled = false;
    }

    public void PlaySlash(Vector3 center, Color color)
    {
        StartCoroutine(SlashRoutine(center, color));
    }

    private IEnumerator SlashRoutine(Vector3 center, Color color)
    {
        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = new Color(color.r, color.g, color.b, 0);

        // Random slash direction
        Vector3 offset = Random.insideUnitCircle.normalized * 1.5f;
        lineRenderer.SetPosition(0, center - offset);
        lineRenderer.SetPosition(1, center + offset);

        yield return new WaitForSeconds(duration);
        lineRenderer.enabled = false;
    }
}

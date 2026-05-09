using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Steuert die visuellen Aspekte der QTE-Ringe.
/// </summary>
public class QTERingController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform outerRing;
    public RectTransform shrinkRing;
    public Image flashEffect;
    
    private Coroutine flashRoutine;

    public void ResetRing(float startScale)
    {
        if (shrinkRing != null)
        {
            shrinkRing.localScale = Vector3.one * startScale;
            shrinkRing.gameObject.SetActive(true);
        }
        if (flashEffect != null)
        {
            flashEffect.gameObject.SetActive(false);
            flashEffect.color = new Color(1, 1, 1, 0);
        }
    }

    public void UpdateScale(float scale)
    {
        if (shrinkRing != null)
        {
            shrinkRing.localScale = Vector3.one * scale;
        }
    }

    public void TriggerFlash(Color color, bool shake)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(color));
        
        if (shake)
        {
            StartCoroutine(ShakeRoutine());
        }
    }

    private IEnumerator FlashRoutine(Color color)
    {
        if (flashEffect == null) yield break;
        
        flashEffect.gameObject.SetActive(true);
        flashEffect.color = color;
        
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * 4f;
            Color c = color;
            c.a = Mathf.Lerp(color.a, 0, t);
            flashEffect.color = c;
            yield return null;
        }
        
        flashEffect.gameObject.SetActive(false);
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0;
        float duration = 0.15f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    public Image blackOverlay;
    public Image whiteOverlay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        if (whiteOverlay != null) whiteOverlay.gameObject.SetActive(false);
    }

    public IEnumerator FadeIn(float duration)
    {
        if (blackOverlay == null) yield break;
        blackOverlay.gameObject.SetActive(true);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackOverlay.color = new Color(0, 0, 0, 1 - (elapsed / duration));
            yield return null;
        }
        blackOverlay.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut(float duration)
    {
        if (blackOverlay == null) yield break;
        blackOverlay.gameObject.SetActive(true);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackOverlay.color = new Color(0, 0, 0, elapsed / duration);
            yield return null;
        }
    }

    public IEnumerator PlayRealisticBlink()
    {
        if (blackOverlay == null || whiteOverlay == null) yield break;
        
        // 1. Initial Black
        blackOverlay.gameObject.SetActive(true);
        whiteOverlay.gameObject.SetActive(false);
        blackOverlay.color = Color.black;
        yield return new WaitForSeconds(1.5f); // Longer wait in darkness

        // 2. Eyes open slightly (Milky/Blurred)
        whiteOverlay.gameObject.SetActive(true);
        float elapsed = 0;
        float duration = 2.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Black fades out, white (milky) is at ~0.5 alpha
            blackOverlay.color = new Color(0, 0, 0, 1 - t);
            whiteOverlay.color = new Color(1, 1, 1, t * 0.4f); 
            yield return null;
        }

        // 3. Close eyes again (Slowly)
        elapsed = 0;
        duration = 1.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            blackOverlay.color = new Color(0, 0, 0, t);
            whiteOverlay.color = new Color(1, 1, 1, 0.4f * (1 - t));
            yield return null;
        }
        whiteOverlay.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.4f);

        // 4. Open fully (Milky -> Clear)
        whiteOverlay.gameObject.SetActive(true);
        elapsed = 0;
        duration = 2.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Black stays out, White fades from 0.5 to 0
            blackOverlay.color = Color.clear;
            whiteOverlay.color = new Color(1, 1, 1, 0.4f * (1 - t));
            yield return null;
        }

        blackOverlay.gameObject.SetActive(false);
        whiteOverlay.gameObject.SetActive(false);
    }
    }

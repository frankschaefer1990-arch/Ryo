using UnityEngine;
using System.Collections.Generic;

public class WaterfallMaster : MonoBehaviour
{
    public bool isLevel2Waterfall = false; // Toggle to use independent flag
    public GameObject waterfallVisual; // The waterfall to disable
    public Collider2D pathBlocker;     // The collider to disable
    public UnityEngine.Tilemaps.TilemapRenderer mainWaterTilemap;
    public List<WaterfallInteraction> levers = new List<WaterfallInteraction>();
    public GameObject exitToNextLevel; // Exit to Wasserfälle von Chlorius 2
    public float fadeSpeed = 1f;
    public AudioClip finalDrainSound;

    private float targetAlpha = 1f;
    private Renderer[] wfRenderers;
    private Material mainWaterMat;
    private AudioSource audioSource;
    private Collider2D mainWaterCollider;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (waterfallVisual != null) wfRenderers = waterfallVisual.GetComponentsInChildren<Renderer>();
        if (mainWaterTilemap != null) 
        {
            mainWaterMat = mainWaterTilemap.sharedMaterial;
            mainWaterCollider = mainWaterTilemap.GetComponent<Collider2D>();
        }

        // Ensure initial state
        if (exitToNextLevel != null) exitToNextLevel.SetActive(false);
        CheckLevers();

        // Immediate alpha set
        bool alreadySolved = false;
        if (QuestManager.Instance != null)
        {
            alreadySolved = isLevel2Waterfall ? QuestManager.Instance.waterfallPuzzle2Solved : QuestManager.Instance.waterfallPuzzleSolved;
        }

        if (alreadySolved)
        {
            targetAlpha = 0f;
            SetAlpha(0f);
            if (mainWaterCollider != null) mainWaterCollider.enabled = false;
        }
    }

    private void Update()
    {
        float currentAlpha = GetCurrentAlpha();
        if (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            float nextAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(nextAlpha);

            // Handle collider state during fade
            if (targetAlpha == 0f && nextAlpha < 0.1f && mainWaterCollider != null && mainWaterCollider.enabled)
            {
                mainWaterCollider.enabled = false;
            }

            // Turn off visual completely when fully faded
            if (nextAlpha <= 0f)
            {
                if (waterfallVisual != null) waterfallVisual.SetActive(false);
                if (mainWaterTilemap != null) mainWaterTilemap.gameObject.SetActive(false);
            }
        }
    }

    public void CheckLevers()
    {
        // If already solved permanently, don't check
        if (QuestManager.Instance != null)
        {
            bool alreadySolved = isLevel2Waterfall ? QuestManager.Instance.waterfallPuzzle2Solved : QuestManager.Instance.waterfallPuzzleSolved;
            if (alreadySolved)
            {
                SetPuzzleSolved(true);
                return;
            }
        }

        int activeCount = 0;
        foreach (var lever in levers)
        {
            if (lever != null && lever.IsActive()) activeCount++;
        }

        // Must be exactly 4 for this specific puzzle
        bool allActive = (activeCount >= 4);

        if (allActive)
        {
            if (QuestManager.Instance != null)
            {
                bool alreadySolved = isLevel2Waterfall ? QuestManager.Instance.waterfallPuzzle2Solved : QuestManager.Instance.waterfallPuzzleSolved;
                if (!alreadySolved)
                {
                    if (isLevel2Waterfall) QuestManager.Instance.waterfallPuzzle2Solved = true;
                    else QuestManager.Instance.waterfallPuzzleSolved = true;

                    if (finalDrainSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(finalDrainSound);
                    }
                }
            }
            SetPuzzleSolved(true);
            Debug.Log("Waterfall Master: Puzzle Solved! All 4 levers active.");
        }
        else
        {
            SetPuzzleSolved(false);
        }
    }

    public void SetSolved(bool solved)
    {
        SetPuzzleSolved(solved);
    }

    private void SetPuzzleSolved(bool solved)
{
        targetAlpha = solved ? 0f : 1f;
        
        if (solved)
        {
            if (pathBlocker != null) pathBlocker.enabled = false;
            if (exitToNextLevel != null) exitToNextLevel.SetActive(true);
        }
        else
        {
            // Reactivate objects if not solved
            if (waterfallVisual != null) waterfallVisual.SetActive(true);
            if (mainWaterTilemap != null) mainWaterTilemap.gameObject.SetActive(true);
            if (pathBlocker != null) pathBlocker.enabled = true;
            if (exitToNextLevel != null) exitToNextLevel.SetActive(false);
        }
    }

    private float GetCurrentAlpha()
    {
        if (wfRenderers != null && wfRenderers.Length > 0)
        {
            Renderer r = wfRenderers[0];
            if (r is SpriteRenderer sr) return sr.color.a;
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Color")) return r.sharedMaterial.color.a;
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseColor")) return r.sharedMaterial.GetColor("_BaseColor").a;
        }
        if (mainWaterMat != null) return mainWaterMat.color.a;
        return targetAlpha;
    }

    private void SetAlpha(float a)
    {
        if (wfRenderers != null)
        {
            foreach (var r in wfRenderers)
            {
                if (r == null) continue;
                if (r is SpriteRenderer sr)
                {
                    Color c = sr.color;
                    c.a = a;
                    sr.color = c;
                }
                else
                {
                    // For Meshes, we usually need to modify the material instance or shared material in editor
                    // Since it's a runtime fade, material property block or instance is better
                    Material m = Application.isPlaying ? r.material : r.sharedMaterial;
                    if (m != null)
                    {
                        if (m.HasProperty("_Color"))
                        {
                            Color c = m.color;
                            c.a = a;
                            m.color = c;
                        }
                        else if (m.HasProperty("_BaseColor"))
                        {
                            Color c = m.GetColor("_BaseColor");
                            c.a = a;
                            m.SetColor("_BaseColor", c);
                        }
                    }
                }
            }
        }
        if (mainWaterMat != null)
        {
            Color c = mainWaterMat.color;
            c.a = a;
            mainWaterMat.color = c;
        }
    }
}
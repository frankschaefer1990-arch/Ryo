using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    public void ShowTooltip(string content)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            tooltipPanel.transform.SetAsLastSibling(); // Ensure it's on top of everything in the same Canvas
            if (tooltipText != null) tooltipText.text = content;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            Canvas canvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            
            // Determine camera for raycast/positioning
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (uiCam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) uiCam = Camera.main;
            
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Input.mousePosition, uiCam, out worldPoint))
            {
                RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
                
                // Pivot at (1, 0) means the bottom-right corner of the tooltip is at the mouse.
                // We add a tiny offset so it doesn't clip with the cursor.
                rt.pivot = new Vector2(1f, 0f);
                rt.position = worldPoint + new Vector3(-0.1f, 0.1f, 0); // Offset to the Top-Left
            }
}
    }
    }
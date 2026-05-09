using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TMP_Text tooltipText;

    private void Awake()
    {
        Instance = this;
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
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, uiCam, out localPoint))
            {
                RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
                
                // Adjust pivot for Top-Left expansion
                // Pivot X: 1.0 (Right edge at mouse -> expands Left)
                // Pivot Y: 0.0 (Bottom edge at mouse -> expands Up)
                float pivotX = 1.0f; 
                float pivotY = 0.0f;
                
                // Safety check: if it goes off the left edge, flip to Right
                if (Input.mousePosition.x < 300f) pivotX = 0f;
                // Safety check: if it goes off the top edge, flip to Down
                if (Input.mousePosition.y > Screen.height - 200f) pivotY = 1f;

                rt.pivot = new Vector2(pivotX, pivotY);
                
                rt.anchoredPosition = localPoint;
            }
}
    }
}

public class TooltipTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string content;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.ShowTooltip(content);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDestroy()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }
}
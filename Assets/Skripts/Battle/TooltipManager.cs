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
                
                // Adjust pivot to keep it on screen
                float pivotX = (Input.mousePosition.x > Screen.width * 0.7f) ? 1.1f : -0.1f;
                float pivotY = (Input.mousePosition.y > Screen.height * 0.7f) ? 1.1f : -0.1f;
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
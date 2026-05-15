using UnityEngine;
using TMPro;

public class BattleTooltipManager : MonoBehaviour
{
    public static BattleTooltipManager Instance;

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
            tooltipPanel.transform.SetAsLastSibling(); 
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
            
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (uiCam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) uiCam = Camera.main;
            
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Input.mousePosition, uiCam, out worldPoint))
            {
                RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
                rt.pivot = new Vector2(1f, 0f);
                rt.position = worldPoint + new Vector3(-0.1f, 0.1f, 0); 
            }
        }
    }
}

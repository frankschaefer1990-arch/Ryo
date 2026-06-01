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

    private Vector2 currentPivot = new Vector2(1f, 0f);

    public void ShowTooltip(string content, Vector2? pivot = null)
    {
        if (tooltipPanel != null)
        {
            currentPivot = pivot ?? new Vector2(1f, 0f);
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
                
                rt.pivot = currentPivot;
                float offsetX = (currentPivot.x > 0.5f) ? -0.1f : 0.1f;
                float offsetY = (currentPivot.y > 0.5f) ? -0.1f : 0.1f;
                rt.position = worldPoint + new Vector3(offsetX, offsetY, 0); 
            }
        }
    }
    }
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private PolygonCollider2D polygon;
    private RectTransform rectTransform;

    void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();
        rectTransform = GetComponent<RectTransform>();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (this == null || !gameObject.activeInHierarchy) return false;
        if (polygon == null || rectTransform == null) return true;

        Vector2 localPoint;
        try 
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPoint))
            {
                Vector3 worldPoint = transform.TransformPoint(localPoint);
                return polygon.OverlapPoint(worldPoint);
            }
        }
        catch (System.Exception)
        {
            return false;
        }
        return false;
    }
    }

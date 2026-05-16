using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private static int sortingOrder;

    public static DamagePopup Create(Vector3 position, int baseAmount, int bonusAmount, TMP_FontAsset font)
    {
        GameObject damagePopupTransform = new GameObject("DamagePopup", typeof(DamagePopup), typeof(TextMeshPro));
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(baseAmount, bonusAmount, font);
        damagePopup.transform.position = position;
        return damagePopup;
    }

    public static DamagePopup Create(Vector3 position, int totalAmount, TMP_FontAsset font, Color? overrideColor = null)
    {
        GameObject damagePopupTransform = new GameObject("DamagePopup", typeof(DamagePopup), typeof(TextMeshPro));
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(totalAmount, font, overrideColor);
        damagePopup.transform.position = position;
        return damagePopup;
    }

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();
    }

    public void Setup(int baseAmount, int bonusAmount, TMP_FontAsset font)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();
        
        if (font != null) textMesh.font = font;
        textMesh.fontSize = 4; // Slightly smaller
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Ensure it's in front of everything
        textMesh.sortingOrder = 500; 
        
        // Format: 15 (Red) +Bonus (Green)
        textMesh.text = $"<color=red>{baseAmount}</color> <color=green>+{bonusAmount}</color>";
        
        textColor = Color.white;
        disappearTimer = 1f;

        moveVector = new Vector3(0, 1.5f) * 1.5f;
    }

    public void Setup(int totalAmount, TMP_FontAsset font, Color? overrideColor = null)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();

        if (font != null) textMesh.font = font;
        textMesh.fontSize = 4; 
        textMesh.alignment = TextAlignmentOptions.Center;
        
        textMesh.sortingOrder = 500;

        textMesh.text = totalAmount.ToString() + " Dmg";
        
        Color c = overrideColor ?? Color.red;
        textMesh.color = c;
        textColor = c;
        disappearTimer = 1f;

        moveVector = new Vector3(0, 1.5f) * 1.5f;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 4f * Time.deltaTime;

        if (disappearTimer > 0.5f)
        {
            // First half of the life
            float scaleAmount = 1f;
            transform.localScale += Vector3.one * scaleAmount * Time.deltaTime;
        }
        else
        {
            // Second half of the life
            float scaleAmount = 1f;
            transform.localScale -= Vector3.one * scaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer <= 0)
        {
            // Start fading out
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

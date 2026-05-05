using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    [Header("HP Bars")]
    public Image playerHPFill;
    public Image enemyHPFill;

    [Header("QTE")]
    public Text qteText;

    [Header("Panels")]
    public GameObject commandPanel;
    public GameObject itemPanel;
    public Text itemButtonText; // To show potion count

    private void Awake()
    {
        Instance = this;
    }

    public void ShowItemPanel()
    {
        if (itemPanel != null)
        {
            itemPanel.SetActive(true);
            int count = InventoryManager.Instance.GetPotionCount();
            if (itemButtonText != null) itemButtonText.text = "Heiltrank (" + count + ")";
        }
    }

    public void HideItemPanel()
    {
        if (itemPanel != null) itemPanel.SetActive(false);
    }

    public void UpdatePlayerHP(float ratio)
{
        if (playerHPFill != null) playerHPFill.fillAmount = ratio;
    }

    public void UpdateEnemyHP(float ratio)
    {
        if (enemyHPFill != null) enemyHPFill.fillAmount = ratio;
    }

    public void ShowComboPrompt(string keyName)
    {
        if (qteText != null) qteText.text = keyName;
    }

    public void HideComboPrompt()
    {
        if (qteText != null) qteText.text = "";
    }

    public void ToggleCommandPanel(bool show)
    {
        if (commandPanel != null) commandPanel.SetActive(show);
    }
}

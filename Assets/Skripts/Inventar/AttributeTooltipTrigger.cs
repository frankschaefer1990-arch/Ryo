using UnityEngine;
using UnityEngine.EventSystems;

public class AttributeTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum AttributeType { Strength, Vitality, Intelligence, Curse }
    public AttributeType type;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            string content = "";
            switch (type)
            {
                case AttributeType.Strength:
                    content = "<b>Stärke</b>\nErhöht den Schaden deiner physischen Angriffe um +1 pro Punkt.";
                    break;
                case AttributeType.Vitality:
                    content = "<b>Vitalität</b>\nErhöht deine maximalen Lebenspunkte um +10 pro Punkt.";
                    break;
                case AttributeType.Intelligence:
                    content = "<b>Intelligenz</b>\nErhöht dein maximales Mana um +10, deine Manaregeneration pro Zug um +1 und den Schaden deiner Zauber um +2 pro Punkt.";
                    break;
                case AttributeType.Curse:
                    content = "<b>Fluch</b>\nVerstärkt die Wirkung all deiner passiven Fluch-Fähigkeiten und skaliert deren Boni.";
                    break;
            }
            TooltipManager.Instance.ShowTooltip(content);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }
}
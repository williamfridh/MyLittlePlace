using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemScript : MonoBehaviour
{
    [Header("Target Game Objects")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemTitleText;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    public void Draw(Sprite icon, string displayName, int amount, string description)
    {
        itemIconImage.sprite = icon;
        itemTitleText.text = displayName;
        itemAmountText.text = $"x{amount}";
        itemDescriptionText.text = description;
    }
}

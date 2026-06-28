using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemScript : MonoBehaviour
{
    [Header("Target Game Objects")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemTitleText;
    [SerializeField] private TextMeshProUGUI itemAmountText;

    public void Draw(Sprite icon, string displayName, int amount)
    {
        itemIconImage.sprite = icon;
        itemTitleText.text = displayName;
        itemAmountText.text = $"x{amount}";
    }
}

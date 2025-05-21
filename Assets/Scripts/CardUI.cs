using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Text nameText;
    public Image artworkImage;
    public Text damageText;

    public void Setup(CardData data)
    {
        nameText.text = data.cardName;
        artworkImage.sprite = data.cardImage;
        damageText.text = data.damage.ToString();
    }
}

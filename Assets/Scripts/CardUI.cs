using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Text nameText;
    public Image artworkImage;
    public Text damageText;
    private CardData data;

    public void Setup(CardData data)
    {
        this.data = data;
        nameText.text = data.cardName;
        artworkImage.sprite = data.cardImage;
        damageText.text = data.damage.ToString();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        var controllerObj = GameObject.Find("GameControllerObject");
        if (controllerObj == null)
        {
            return;
        }

        var controller = controllerObj.GetComponent<GameController>();
        if (controller == null || controller.isBusy)
        {
            return;
        }

        var hero = GameObject.FindGameObjectWithTag("Hero");
        if (hero == null)
        {
            return;
        }

        var action = hero.GetComponent<FighterAction>();
        if (action == null)
        {
            return;
        }

        action.PlayCard(data);
    }
}

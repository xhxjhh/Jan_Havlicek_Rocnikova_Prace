using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public List<CardData> deck; // celý balíček
    public List<CardData> hand = new List<CardData>();

    public GameObject cardPrefab;
    public Transform handPanel;
    public int handSize = 3;
    public Vector2 cardPreferredSize = new Vector2(220, 300);

    void Awake()
    {
        if (deck == null)
        {
            deck = new List<CardData>();
        }

        if (deck.Count == 0)
        {
            deck.Add(new CardData { cardName = "Stab", damage = 10, cost = 0 });
            deck.Add(new CardData { cardName = "Magic Missle", damage = 15, cost = 10 });
            deck.Add(new CardData { cardName = "Heal", damage = 15, cost = 10 });
            deck.Add(new CardData { cardName = "Defense", damage = 5, cost = 10 });
            deck.Add(new CardData { cardName = "Restore Mana", damage = 0, cost = 0 });
            deck.Add(new CardData { cardName = "Wild Card", damage = 0, cost = 15 });
        }
    }

    public void DrawCards()
    {
        // Vyčisti staré karty z ruky
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        hand.Clear();
        var uniqueDeck = new List<CardData>();
        var seenNames = new HashSet<string>();
        foreach (var card in deck)
        {
            if (card == null)
            {
                continue;
            }

            var name = card.cardName ?? "";
            if (seenNames.Add(name))
            {
                uniqueDeck.Add(card);
            }
        }

        List<CardData> tempDeck = new List<CardData>(uniqueDeck);

        int drawCount = Mathf.Clamp(handSize, 0, 3);
        for (int i = 0; i < drawCount && tempDeck.Count > 0; i++)
        {
            int rand = Random.Range(0, tempDeck.Count);
            CardData drawn = tempDeck[rand];
            hand.Add(drawn);
            tempDeck.RemoveAt(rand);

            GameObject card = Instantiate(cardPrefab, handPanel);
            card.transform.localScale = Vector3.one;

            var rect = card.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }

            var layout = card.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = card.AddComponent<LayoutElement>();
            }
            layout.preferredWidth = cardPreferredSize.x;
            layout.preferredHeight = cardPreferredSize.y;

            card.GetComponent<CardUI>().Setup(drawn);
        }
    }
}

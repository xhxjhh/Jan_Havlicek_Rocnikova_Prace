using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> deck; // celý balíček
    public List<CardData> hand = new List<CardData>();

    public GameObject cardPrefab;
    public Transform handPanel;
    public int handSize = 3;

    public void DrawCards()
    {
        // Vyčisti staré karty z ruky
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        hand.Clear();
        List<CardData> tempDeck = new List<CardData>(deck);

        for (int i = 0; i < handSize && tempDeck.Count > 0; i++)
        {
            int rand = Random.Range(0, tempDeck.Count);
            CardData drawn = tempDeck[rand];
            hand.Add(drawn);
            tempDeck.RemoveAt(rand);

            GameObject card = Instantiate(cardPrefab, handPanel);
            card.GetComponent<CardUI>().Setup(drawn);
        }
    }
}

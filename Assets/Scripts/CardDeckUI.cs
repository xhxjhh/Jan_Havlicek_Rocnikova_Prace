using UnityEngine;

public class CardDeckUI : MonoBehaviour
{
    public GameObject deckPanel;

    public void ToggleDeck()
    {
        deckPanel.SetActive(!deckPanel.activeSelf);
    }
}
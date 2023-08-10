using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public SO_Deck deck;
    public List<SO_Card> drawPile = new List<SO_Card>();
    public List<SO_Card> discardPile = new List<SO_Card>();

    public Transform cardSpawnLocation;

    private void Awake()
    {
        InitializeDeck();
    }

    public SO_Card DrawCard()
    {
        if (drawPile.Count == 0)
        {
            drawPile = discardPile;
            discardPile.Clear();
            ShuffleDrawPile();
        }
        SO_Card cardDrawn = drawPile[0];
        drawPile.RemoveAt(0);

        return cardDrawn;
    }

    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        foreach (SO_Card card in deck.deckList)
        {
            drawPile.Add(card);
        }
        ShuffleDrawPile();
    }

    public void ShuffleDrawPile()
    {
        for(int i = 0; i < deck.deckList.Count - 1; i++)
        {
            SO_Card temp = deck.deckList[i];
            int rand = Random.Range(i, deck.deckList.Count);
            deck.deckList[i] = deck.deckList[rand];
            deck.deckList[rand] = temp;
        }
    }
}

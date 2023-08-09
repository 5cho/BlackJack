using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "MyScriptableObjects/Deck")]
public class SO_Deck : ScriptableObject
{
    public List<SO_Card> deckList = new List<SO_Card>();
}

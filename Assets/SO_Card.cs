using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "MyScriptableObjects/Card")]
public class SO_Card : ScriptableObject
{
    public Suit suit;
    public CardType cardType;
    public int cardValue;
    public GameObject cardPrefab;
}

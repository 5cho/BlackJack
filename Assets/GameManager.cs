using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Deck deck;
    public List<SO_Card> playerHand;
    public List<SO_Card> dealerHand;

    public Transform playerHandLocation;
    public Transform dealerHandLocation;
    private Vector3 startingPlayerHandLocation = new Vector3();
    private Vector3 startingDealerHandLocation = new Vector3();
    private Vector3 cardOffset = new Vector3(0.02f,0.01f,0f);


    public GameObject deckPrefab;
    public Transform deckSpawnLocation;
    public Transform cardSpawnLocation;

    public TMP_Text betAmmountText; 
    public TMP_Text playerHandText;
    public TMP_Text dealerHandText;
    public GameObject placeBetUI;
    public GameObject playTimeUI;
    public TMP_Text playerMoneyText;
    public GameObject playerHandCanvas;
    public GameObject dealerHandCanvas;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text drawText;
    public Button newGameButton;

    private int playerHandValue;
    private int dealerHandValue;

    private int playerMoney = 1000;
    private int betAmmount;

    private bool isBetPlaced;
    private bool isTurnOver;
    private bool isNewGameButtonPressed;

    private void Awake()
    {
        startingDealerHandLocation = dealerHandLocation.position;
        startingPlayerHandLocation = playerHandLocation.position;
    }
    private void Start()
    {
        Instantiate(deckPrefab, deckSpawnLocation.position, Quaternion.identity);
        NewHand();
    }
    private void Update()
    {
        playerMoneyText.text = playerMoney.ToString() + " $";
    }

    private async void NewHand()
    {
        deck.InitializeDeck();
        ResetHandLocations();
        await PlaceBet();
        await DealStartingHand();
        ShowHands();
        await PlayerTurn();
        await DealerTurn();
        DeclareWinner();
    }

    private void ResetHandLocations()
    {
        dealerHandLocation.position = startingDealerHandLocation;
        playerHandLocation.position = startingPlayerHandLocation;
    }

    private void DeclareWinner()
    {
        if(playerHandValue > 21)
        {
            PlayerLost();
        }
        else if(dealerHandValue > 21)
        {
            PlayerWon();
        } 
        else if(playerHandValue == dealerHandValue)
        {
            DrawGame();
        }
        else if(playerHandValue < dealerHandValue)
        {
            PlayerLost();
        }
        else
        {
            PlayerWon();
        }
    }
    public async Task NewGameSetup()
    {
        isNewGameButtonPressed = false;
        while (!isNewGameButtonPressed)
        {
            await Task.Yield();
        }
        loseText.gameObject.SetActive(false);
        winText.gameObject.SetActive(false);
        drawText.gameObject.SetActive(false);
        newGameButton.gameObject.SetActive(false);
        CleanScene();
        NewHand();
    }
    public void NewGameButtonPressed()
    {
        isNewGameButtonPressed = true;
    }

    private void CleanScene()
    {
        GameObject[] listOfCardsToRemove = GameObject.FindGameObjectsWithTag("Card");
        foreach(GameObject card in listOfCardsToRemove)
        {
            Destroy(card);
        }
        playerHandCanvas.SetActive(false);
        dealerHandCanvas.SetActive(false);
        playerHand.Clear();
        dealerHand.Clear();
    }
    private async void PlayerLost()
    {
        loseText.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
        await NewGameSetup();
    }

    private async void PlayerWon()
    {
        playerMoney = playerMoney + 2 * betAmmount;
        winText.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
        await NewGameSetup();
    }
    private async void DrawGame()
    {
        playerMoney = playerMoney + betAmmount;
        drawText.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
        await NewGameSetup();
    }

    private async Task DealerTurn()
    {
        bool isDealerTurnOver = false;
        while (!isDealerTurnOver)
        {
            if(dealerHandValue < 17)
            {
                await DealToDealer();
                CalculateDealerHand(dealerHand);
            }
            else
            {
                isDealerTurnOver = true;
            }
            if(dealerHandValue > 21)
            {
                isDealerTurnOver = true;
            }
        }
    }
    private async Task PlayerTurn()
    {
        isTurnOver = false;
        playTimeUI.SetActive(true);
        while (!isTurnOver)
        {
            CalculatePlayerHand(playerHand);
            if(playerHandValue > 21)
            {
                isTurnOver = true;
                playTimeUI.SetActive(false);
            }
            await Task.Yield();
        }
    }
    public async void HitButtonPressed()
    {
        await DealToPlayer();
        CalculatePlayerHand(playerHand);
    }
    public void StopButtonPressed()
    {
        isTurnOver = true;
        playTimeUI.SetActive(false);
    }

    private void ShowHands()
    {
        CalculatePlayerHand(playerHand);
        CalculateDealerHand(dealerHand);
        playerHandCanvas.SetActive(true);
        dealerHandCanvas.SetActive(true);
    }

    private void CalculatePlayerHand(List<SO_Card> hand)
    {
        int calculatedValue = 0;
        foreach(SO_Card card in hand)
        {
            calculatedValue += card.cardValue;
        }
        playerHandValue = calculatedValue;
        playerHandText.text = playerHandValue.ToString();
    }
    private void CalculateDealerHand(List<SO_Card> hand)
    {
        int calculatedValue = 0;
        foreach (SO_Card card in hand)
        {
            calculatedValue += card.cardValue;
        }
        dealerHandValue = calculatedValue;
        dealerHandText.text = dealerHandValue.ToString();
    }

    private async Task PlaceBet()
    {
        placeBetUI.SetActive(true);
        betAmmount = 0;
        isBetPlaced = false;
        while (!isBetPlaced)
        {
            betAmmountText.text = betAmmount.ToString() + " $";
            await Task.Yield();
        }
        playerMoney -= betAmmount;
        placeBetUI.SetActive(false);
        
    }

    public void RaiseBet()
    {
        betAmmount += 10;
    }
    public void LowerBet() 
    {
        betAmmount -= 10;
    }
    public void PlaceBetButtonPressed()
    {
        isBetPlaced = true;
    }

    private async Task DealStartingHand()
    {
        await DealToPlayer();  
        await DealToDealer();   
        await DealToPlayer();    
        await DealToDealer();
    }

    private async Task DealToPlayer()
    {
        SO_Card cardToDraw = deck.DrawCard();
        GameObject spawnedCard = Instantiate(cardToDraw.cardPrefab, cardSpawnLocation.position, cardToDraw.cardPrefab.transform.rotation * Quaternion.Euler(0f, 0f, 180f));
        await MoveCard(spawnedCard, playerHandLocation);
        playerHand.Add(cardToDraw);
    }
    private async Task DealToDealer()
    {
        SO_Card cardToDraw = deck.DrawCard();
        GameObject spawnedCard = Instantiate(cardToDraw.cardPrefab, cardSpawnLocation.position, cardToDraw.cardPrefab.transform.rotation * Quaternion.Euler(0f, 0f, 180f));
        await MoveCard(spawnedCard, dealerHandLocation);
        dealerHand.Add(cardToDraw);
    }

    private async Task MoveCard(GameObject card, Transform targetPosition)
    {
        Vector3 startPosition = card.transform.position;
        Vector3 goalPosition = targetPosition.position;
        float target = 1;
        float speed = 1f;
        float current = 0; 

        while (current < 1) 
        {
            current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
            card.transform.position = Vector3.Lerp(startPosition, goalPosition, current);
            await Task.Yield();
        }
        card.gameObject.GetComponent<Animation>().Play();
        targetPosition.position = targetPosition.position + cardOffset;
    }
    
}

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

    [SerializeField] private Transform playerHandLocation;
    [SerializeField] private Transform dealerHandLocation;
    private Vector3 startingPlayerHandLocation = new Vector3();
    private Vector3 startingDealerHandLocation = new Vector3();
    private Vector3 cardOffset = new Vector3(0.02f, 0.01f, 0f);


    [SerializeField] private GameObject deckPrefab;
    [SerializeField] private Transform deckSpawnLocation;
    [SerializeField] private Transform cardSpawnLocation;


    [SerializeField] private TMP_Text betAmmountText;
    [SerializeField] private TMP_Text playerHandText;
    [SerializeField] private TMP_Text dealerHandText;
    [SerializeField] private GameObject placeBetUI;
    [SerializeField] private GameObject playTimeUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private GameObject playerHandCanvas;
    [SerializeField] private GameObject dealerHandCanvas;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text loseText;
    [SerializeField] private TMP_Text drawText;
    [SerializeField] private TMP_Text blackjackText;
    [SerializeField] private Button newGameButton;

    private int playerHandValue;
    private int dealerHandValue;

    public int playerMoney = 1000;
    public int betAmmount;

    private bool isBetPlaced;
    private bool isTurnOver;
    private bool isNewGameButtonPressed;
    private bool isGameOver = false;
    private bool isPlayerLost;

    private GameObject dealerHiddenCard;
    private SO_Card dealerHiddenCardSO;

    private ChipsManager chipsManager;

    private void Awake()
    {
        startingDealerHandLocation = dealerHandLocation.position;
        startingPlayerHandLocation = playerHandLocation.position;
        chipsManager = GetComponent<ChipsManager>();
    }
    private void Start()
    {
        Instantiate(deckPrefab, deckSpawnLocation.position, Quaternion.identity);
        deck.InitializeDeck();
        NewHand();
    }
    private void Update()
    {
        playerMoneyText.text = playerMoney.ToString() + " $";
    }

    private async void NewHand()
    {
        ResetHandLocations();
        await PlaceBet();
        chipsManager.SpawnChipsBetAmmount();
        await DealStartingHand();
        ShowHands();
        if (playerHandValue == 21)
        {
            await PlayerBlackjack();
        } else
        {
            await PlayerTurn();
            if (!isPlayerLost)
            {
                await DealerTurn();
            }
            DeclareWinner();
        }
    }
    private async Task PlayerBlackjack()
    {
        chipsManager.SpawnPlayerWinninigs();
        playerMoney = playerMoney + 3 * betAmmount;
        winText.gameObject.SetActive(true);
        blackjackText.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
        await NewGameSetup();
    }
    private void ResetHandLocations()
    {
        dealerHandLocation.position = startingDealerHandLocation;
        playerHandLocation.position = startingPlayerHandLocation;
        isPlayerLost = false;
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
        blackjackText.gameObject.SetActive(false);
        newGameButton.gameObject.SetActive(false);
        chipsManager.ClearChipsBetAmmount();
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
        foreach(SO_Card card in playerHand)
        {
            deck.discardPile.Add(card);
        }
        foreach (SO_Card card in dealerHand)
        {
            deck.discardPile.Add(card);
        }
        playerHand.Clear();
        dealerHand.Clear();
    }
    private async void PlayerLost()
    {
        if(playerMoney > 0)
        {
            loseText.gameObject.SetActive(true);
            newGameButton.gameObject.SetActive(true);
        }
        if(playerMoney == 0)
        {
            isGameOver = true;
            gameOverUI.SetActive(true);
            await GameRestarted();
        } else
        {
            await NewGameSetup();
        }
    }
    private async Task GameRestarted()
    {
        while(isGameOver == true)
        {
            await Task.Yield();
        }
        playerMoney = 1000;
        gameOverUI.SetActive(false);
        NewHand();
    }
    public void RestartGameButtonPressed()
    {
        isGameOver = false;
    }

    private async void PlayerWon()
    {
        chipsManager.SpawnPlayerWinninigs();
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
        await RevealSecondCard();
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
                isPlayerLost = true;
                playTimeUI.SetActive(false);
            }
            await Task.Yield();
        }
    }
    public async void DoubleDownButtonPressed()
    {
        playerMoney -= betAmmount;
        betAmmount *= 2;
        chipsManager.ClearChipsBetAmmount();
        chipsManager.SpawnChipsBetAmmount();
        await DealToPlayer();
        CalculatePlayerHand(playerHand);
        playTimeUI.SetActive(false);
        isTurnOver = true;
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
        bool hasAce = false;
        int calculatedValue = 0;
        foreach(SO_Card card in hand)
        {
            if(card.cardType == CardType.Ace)
            {
                hasAce = true;
            }
            calculatedValue += card.cardValue;
        }
        if(hasAce && calculatedValue < 12)
        {
            calculatedValue += 10;
        }
        playerHandValue = calculatedValue;
        playerHandText.text = playerHandValue.ToString();
    }
    private void CalculateDealerHand(List<SO_Card> hand)
    {
        bool hasAce = false;
        int calculatedValue = 0;
        foreach (SO_Card card in hand)
        {
            if(card.cardType == CardType.Ace)
            {
                hasAce = true;
            }
            calculatedValue += card.cardValue;
        }
        if(hasAce && calculatedValue < 12)
        {
            calculatedValue += 10;
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
        if(betAmmount < playerMoney)
        {
            betAmmount += 10;
        }
    }
    public void LowerBet() 
    {
        if(betAmmount > 0)
        {
            betAmmount -= 10;
        }
    }
    public void PlaceBetButtonPressed()
    {
        if(betAmmount != 0)
        {
            isBetPlaced = true;
        }
    }

    private async Task DealStartingHand()
    {
        await DealToPlayer();  
        await DealToDealer();   
        await DealToPlayer();    
        await DealSecondCardToDealer();
    }
    private async Task RevealSecondCard()
    {
        Animation animation = dealerHiddenCard.GetComponent<Animation>();
        animation.Play();
        while (animation.IsPlaying("RotateAnimation"))
        {
            await Task.Yield();
        }
        dealerHand.Add(dealerHiddenCardSO);
        CalculateDealerHand(dealerHand);
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
    private async Task DealSecondCardToDealer()
    {
        SO_Card cardToDraw = deck.DrawCard();
        GameObject spawnedCard = Instantiate(cardToDraw.cardPrefab, cardSpawnLocation.position, cardToDraw.cardPrefab.transform.rotation * Quaternion.Euler(0f,0f,180f));
        dealerHiddenCard = spawnedCard;
        dealerHiddenCardSO = cardToDraw;
        await MoveHiddenCard(spawnedCard, dealerHandLocation);
    }

    private async Task MoveCard(GameObject card, Transform targetPosition)
    {
        Vector3 startPosition = card.transform.position;
        Vector3 goalPosition = targetPosition.position;
        float target = 1;
        float speed = 1.5f;
        float current = 0; 

        while (current < 1) 
        {
            current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
            card.transform.position = Vector3.Lerp(startPosition, goalPosition, current);
            await Task.Yield();
        }
        card.GetComponent<Animation>().Play();
        targetPosition.position = targetPosition.position + cardOffset;
    }
    private async Task MoveHiddenCard(GameObject card, Transform targetPosition)
    {
        Vector3 startPosition = card.transform.position;
        Vector3 goalPosition = targetPosition.position;
        float target = 1;
        float speed = 1.5f;
        float current = 0;

        while (current < 1)
        {
            current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
            card.transform.position = Vector3.Lerp(startPosition, goalPosition, current);
            await Task.Yield();
        }
        targetPosition.position = targetPosition.position + cardOffset;
    }
    
    
}

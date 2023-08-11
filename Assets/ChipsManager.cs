using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipsManager : MonoBehaviour
{
    public GameManager gameManger;
    [SerializeField] private GameObject chipsPrefab;
    [SerializeField] private Transform chipsSpawnLocation;
    [SerializeField] private Transform playerMoneySpawnLocation;
    [SerializeField] private Transform playerWinningsSpawnLocation;

    private Vector3 chipsSpawnOffset = new Vector3(0f, 0.005f, 0f);
    private Vector3 chipsSpawnStartingOffset = new Vector3(0f, 0.005f, 0f);
    private Vector3 stackSpawnOffset = new Vector3(-0.05f, 0f, 0f);
    private Vector3 stackSpawnStartingOffset = new Vector3(-0.05f, 0f, 0f);
    private List<GameObject> chipsSpawned = new List<GameObject>();
    private Vector3 chipsStartingSpawnLocation;
    private Vector3 playerWinningsSpawnStartingLocation;

    private int numberOfChipsSpawned;

    private void Start()
    {
        chipsStartingSpawnLocation = chipsSpawnLocation.position;
        playerWinningsSpawnStartingLocation = playerWinningsSpawnLocation.position;
    }

    public void SpawnChipsBetAmmount()
    {
        for (int i = 0; i < gameManger.betAmmount / 10; i++)
        {
            GameObject spawnedChip = Instantiate(chipsPrefab, chipsSpawnLocation.position + chipsSpawnOffset, Quaternion.identity);
            chipsSpawnOffset += chipsSpawnStartingOffset;
            chipsSpawned.Add(spawnedChip);
            numberOfChipsSpawned++;
            if(numberOfChipsSpawned % 5 == 0)
            {
                chipsSpawnLocation.position = chipsSpawnLocation.position + stackSpawnOffset;
                chipsSpawnOffset = chipsSpawnStartingOffset;
            }
        }
        ResetSpawnPositions();
    }
    private void ResetSpawnPositions()
    {
        chipsSpawnStartingOffset = new Vector3(0f, 0.005f, 0f);
        chipsSpawnLocation.position = chipsStartingSpawnLocation;
        chipsSpawnOffset = chipsSpawnStartingOffset;
        stackSpawnOffset = stackSpawnStartingOffset;
        playerWinningsSpawnLocation.position = playerWinningsSpawnStartingLocation;
        numberOfChipsSpawned = 0;
    }
    public void ClearChipsBetAmmount()
    {
        for(int i = 0; i < chipsSpawned.Count; i++)
        {
            Destroy(chipsSpawned[i]);
        }
        ResetSpawnPositions();
        chipsSpawned.Clear();
        numberOfChipsSpawned = 0;
    }
    
    public void SpawnPlayerWinninigs()
    {
        for (int i = 0; i < gameManger.betAmmount / 10; i++)
        {
            GameObject spawnedChip = Instantiate(chipsPrefab, playerWinningsSpawnLocation.position + chipsSpawnOffset, Quaternion.identity);
            chipsSpawnOffset += chipsSpawnStartingOffset;
            chipsSpawned.Add(spawnedChip);
            numberOfChipsSpawned++;
            if (numberOfChipsSpawned % 5 == 0)
            {
                playerWinningsSpawnLocation.position = playerWinningsSpawnLocation.position + stackSpawnOffset;
                chipsSpawnOffset = chipsSpawnStartingOffset;
            }
        }
        ResetSpawnPositions();
    }
}

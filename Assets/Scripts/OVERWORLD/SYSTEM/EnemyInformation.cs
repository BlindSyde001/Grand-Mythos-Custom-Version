using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : MonoBehaviour
{
    private GameManager GM;
    private EventManager EM;

    // VARIABLES
    // Enemies spawned in order
    [SerializeField]
    internal List<EnemyExtension> _Formation1;
    [SerializeField]
    internal List<EnemyExtension> _Formation2;
    [SerializeField]
    internal List<EnemyExtension> _Formation3;
    [SerializeField]
    internal List<EnemyExtension> _Formation4;

    [Space (30)]

    // Enemy Chance to Appear
    [SerializeField]
    internal float[] SpawnTable = new float[5];

    private static EnemyInformation _instance;
    // UPDATES
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        GM = FindObjectOfType<GameManager>();
        EM = FindObjectOfType<EventManager>();
    }
    // Determine which enemies to spawn
    internal void DetermineEnemyFormation()
    {
        float chance = Random.Range(0f, 100f);
        for(int i = 0; i < SpawnTable.Length; i++)
        {
            if (chance <= SpawnTable[i])
            {
                AssignEnemyFormation(i);
                StartBattle();
                return;
            }
            else
                chance = chance - SpawnTable[i];
        }
    }
    private void AssignEnemyFormation(int enemyChance)
    {
        switch (enemyChance)
        {
            case 0:
                GM._EnemyLineup.AddRange(_Formation1);
                break;

            case 1:
                GM._EnemyLineup.AddRange(_Formation2);
                break;

            case 2:
                GM._EnemyLineup.AddRange(_Formation3);
                break;

            case 3:
                GM._EnemyLineup.AddRange(_Formation4);
                break;
        }
    }
    private void StartBattle()
    {
        EM.ChangeFunction(GameState.BATTLE);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : MonoBehaviour
{
    private EventManager _EventManager;
    private GameManager _GameManager;

    // Enemies spawned in order
    [SerializeField]
    internal List<EnemyExtension> _Formation1;
    [SerializeField]
    internal List<EnemyExtension> _Formation2;
    [SerializeField]
    internal List<EnemyExtension> _Formation3;
    [SerializeField]
    internal List<EnemyExtension> _Formation4;
    [SerializeField]
    internal List<EnemyExtension> _Formation5;

    [Space (30)]

    // Enemy Chance to Appear
    [SerializeField]
    internal float[] SpawnTable = new float[5];

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        _EventManager = FindObjectOfType<EventManager>();
        _GameManager = FindObjectOfType<GameManager>();
    }

    // Determine which enemies to spawn
    internal void DetermineEnemyFormation()
    {
        float chance = Random.Range(0f, 100f);
        for(int i = 0; i < SpawnTable.Length; i++)
        {
            if (chance <= SpawnTable[i])
            {
                EnemyFormation(i);
                return;
            }
            else
                chance = chance - SpawnTable[i];
        }
    }
    private void EnemyFormation(int enemyChance)
    {
        switch (enemyChance)
        {
            case 0:
                _GameManager._EnemyLineup.AddRange(_Formation1);
                break;

            case 1:
                _GameManager._EnemyLineup.AddRange(_Formation2);
                break;

            case 2:
                _GameManager._EnemyLineup.AddRange(_Formation3);
                break;

            case 3:
                _GameManager._EnemyLineup.AddRange(_Formation4);
                break;

            case 4:
                _GameManager._EnemyLineup.AddRange(_Formation5);
                break;
        }
        _EventManager.ChangeInGameState(GameState.BATTLE);
    }
}

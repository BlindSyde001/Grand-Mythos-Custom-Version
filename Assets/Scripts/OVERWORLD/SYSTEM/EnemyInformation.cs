using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : MonoBehaviour
{
    private GameManager gameManager;

    // VARIABLES
    List<EnemyExtension> tempLineup = new();
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
    internal float[] SpawnTable = new float[4];
    public static EnemyInformation _instance;

    // UPDATES
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
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
                tempLineup.AddRange(_Formation1);
                break;

            case 1:
                tempLineup.AddRange(_Formation2);
                break;

            case 2:
                tempLineup.AddRange(_Formation3);
                break;

            case 3:
                tempLineup.AddRange(_Formation4);
                break;
        }
        CreateEnemyInstances();
    }
    private void CreateEnemyInstances()
    {
        for( int i = 0; i < tempLineup.Count; i++)
        {
            EnemyExtension instantiatedEnemyClass = Instantiate(tempLineup[i], gameManager.transform.Find("Enemies"));
            instantiatedEnemyClass.name = tempLineup[i].charName + "Data" + i;
            gameManager._EnemyLineup.Add(instantiatedEnemyClass);
        }
        EventManager._instance.SwitchNewScene(1);
    }
}
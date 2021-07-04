using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInformation : MonoBehaviour
{
    // VARIABLES
    private OverworldPlayerCircuit _PlayerCircuit;
    private EnemyInformation _EnemyInformation;

    [SerializeField]
    internal int _NextEncounter;                    // Number to 
    [SerializeField]
    internal int encounterRate = 5;

    private float t;
    private bool lockout;
    public bool TEST;

    // UPDATES
    private void Start()
    {
        _PlayerCircuit = FindObjectOfType<OverworldPlayerCircuit>();
        _EnemyInformation = FindObjectOfType<EnemyInformation>();
        _NextEncounter = Random.Range(6, 256);
    }
    private void FixedUpdate()
    {
        // Enounter rate, where t represents steps
        if (_PlayerCircuit.isMoving && TEST)
        {
            t += Time.deltaTime;
            if (t > .25f)
            {
                _NextEncounter -= encounterRate;
                t = 0;
            }
        }
        else
            t = 0;

        // Start encounter, where lockout prevents more than one instance from loading
        if(_NextEncounter < encounterRate & !lockout)
        {
            EnemiesEncountered();
            lockout = !lockout;
        }
    }

    // METHODS
    private void EnemiesEncountered()
    {
        // Start a battle with enemies
        _EnemyInformation.DetermineEnemyFormation();
    }
}

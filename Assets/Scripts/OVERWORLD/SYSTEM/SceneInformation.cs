using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneInformation : MonoBehaviour
{
    // VARIABLES

    [BoxGroup("Names")]
    public string areaName;
    [BoxGroup("Names")]
    public string zoneName;

    [SerializeField]
    internal GameObject _Player;
    private OverworldPlayerCircuit _PlayerCircuit;

    [SerializeField]
    internal int _NextEncounter;
    internal int encounterRate = 5;

    private float t;
    private bool lockout;

    public List<Transform> DoorwayPoints; // The different locations player can spawn b/c of a doorway

    // UPDATES
    private void Start()
    {
        CreateMovablePlayer(SceneChangeManager._instance.DoorwayToSpawn);
        _PlayerCircuit = FindObjectOfType<OverworldPlayerCircuit>();
        _NextEncounter = Random.Range(6, 256);
    }
    private void FixedUpdate()
    {
        // Enounter rate, where t represents steps
        if (_PlayerCircuit.isMoving)
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
    private void CreateMovablePlayer(int DoorwayToSpawn)
    {
        // If I move back to the same Scene, reload me at last position, otherwise, spawn me in a designated spot
        if(GameManager._instance._LastKnownScene == SceneManager.GetActiveScene().name)
        {
            Instantiate(_Player,
                        GameManager._instance._LastKnownPosition,
                        GameManager._instance._LastKnownRotation);
        }
        else
        {
            Instantiate(_Player, 
                        DoorwayPoints[DoorwayToSpawn].position, 
                        DoorwayPoints[DoorwayToSpawn].rotation);
        }
    }
    private void EnemiesEncountered()
    {
        // Prepare data for next battle
        FindObjectOfType<EnemyInformation>().DetermineEnemyFormation();
    }
}

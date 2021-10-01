using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInformation : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal GameObject _Player;
    private OverworldPlayerCircuit _PlayerCircuit;
    private EnemyInformation _EnemyInformation;

    [SerializeField]
    internal int _NextEncounter;
    internal int encounterRate = 5;

    private float t;
    private bool lockout;

    public List<Transform> DoorwayPoints;

    // UPDATES
    private void Start()
    {
        CreateMovablePlayer();
        _PlayerCircuit = FindObjectOfType<OverworldPlayerCircuit>();
        _EnemyInformation = FindObjectOfType<EnemyInformation>();
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
    private void CreateMovablePlayer()
    {
        GameManager x = FindObjectOfType<GameManager>();
        if(x._LastKnownScene == SceneManager.GetActiveScene().name)
        {
            Instantiate<GameObject>(_Player, 
                                    x._LastKnownPosition, 
                                    x._LastKnownRotation);
        }
        else
        {
            Instantiate<GameObject>(_Player, 
                                    new Vector3(0, 1, 0), 
                                    new Quaternion(0, 0, 0, 0));
        }
    }
    private void EnemiesEncountered()
    {
        // Start a battle with enemies
        _EnemyInformation.DetermineEnemyFormation();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal EventManager EM;

    #region OVERWORLD
    [SerializeField]
    internal Vector3 _LastKnownPosition;
    [SerializeField]
    internal Quaternion _LastKnownRotation;
    #endregion
    #region BATTLE
    [SerializeField]
    internal List<EnemyExtension> _EnemyLineup;
    #endregion
}

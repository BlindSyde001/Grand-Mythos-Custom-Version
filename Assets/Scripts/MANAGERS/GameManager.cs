using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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
    internal List<HeroExtension> _AllPartyMembers; // All in Party

    [SerializeField]
    internal List<HeroExtension> _PartyLineup;  // Who I've selected to be fighting
    [SerializeField]
    internal List<EnemyExtension> _EnemyLineup;
    #endregion
}

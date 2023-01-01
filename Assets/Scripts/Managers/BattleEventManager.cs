using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEventManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private BattleUIController battleUIController;
    [SerializeField]
    private BattleTargetting battleTargetting;

    public delegate void ChangeInChosenActionsList();
    public static ChangeInChosenActionsList OnChosenActionsListAdded;
    public static ChangeInChosenActionsList OnChosenActionsListRemoved;

    // UPDATES
    private void OnEnable()
    {
        OnChosenActionsListAdded += AddActionSegmentUI;
        OnChosenActionsListRemoved += RemoveActionSegmentUI;
    }
    private void OnDisable()
    {
        OnChosenActionsListAdded -= AddActionSegmentUI;
        OnChosenActionsListRemoved -= RemoveActionSegmentUI;
    }

    // METHODS
    private void AddActionSegmentUI()
    {

    }
    private void RemoveActionSegmentUI()
    {

    }
}

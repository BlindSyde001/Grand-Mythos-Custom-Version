using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class UIActionPreview : UIHelper
{
    [FormerlySerializedAs("Show"), InfoBox("Runs whenever a new preview component is created")]
    public UnityEvent<string> Created;
    [FormerlySerializedAs("Discard"), InfoBox("When this action is removed from the list")]
    public UnityEvent Discarded;

    [InfoBox("Runs whenever this is moved to a different spot in the list of actions")]
    public UnityEvent Moved;
    [InfoBox("If this action is a preview of the automated tactics")]
    public UnityEvent IsTactics;
    [InfoBox("If the player manually setup this action")]
    public UnityEvent IsOrder;
    [InfoBox("If the unit is running this action")]
    public UnityEvent IsExecution;
}
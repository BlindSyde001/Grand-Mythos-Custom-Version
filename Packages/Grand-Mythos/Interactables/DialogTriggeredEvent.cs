using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("DialogTriggered Event")]
[UnitCategory("Events")]
public class DialogTriggeredEvent : GameObjectEventUnit<GameObject> // Dummy gameobject type because event requires one
{
    public const string Key = "NodalogDialogTrigger";

    protected override string hookName => Key;

    public override Type MessageListenerType => null;

    protected override void Definition()
    {
        coroutine = true;
        base.Definition();
    }

    protected override void AssignArguments(Flow flow, GameObject gameObject)
    {
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourModule 
{
    public abstract void InitBehaviour();
    public abstract void TickBehaviour();
    public abstract void EndBehaviour();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcCircuit : MonoBehaviour
{
    [SerializeField]
    internal NpcMoveNode _NPCMoveNode;
    [SerializeField]
    internal NpcColliderNode _NPCColliderNode;
    [SerializeField]
    internal NpcTalkNode _NPCTalkNode;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrototypeUI : MonoBehaviour
{
    public List<Slider> actionSliders;
    PrototypeBattle pb;

    private void Awake()
    {
        pb = GetComponent<PrototypeBattle>();
    }

    private void Update()
    {
        for(int i = 0; i < pb.herolist.Count; i++)
        {
            actionSliders[i].value = pb.herolist[i]._ActionChargeAmount;
        }
    }
}

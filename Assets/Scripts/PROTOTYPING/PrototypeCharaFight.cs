using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeCharaFight : CharacterCircuit
{
    public GameObject bullet;
    internal PrototypeBattle pb;
    internal CharacterMovement cm;
    public bool isHero;
    public GameObject model;

    private void Awake()
    {
        pb = GetComponent<PrototypeBattle>();
        cm = model.GetComponent<CharacterMovement>();


        int z = isHero ? Random.Range(0, pb.enemylist.Count) : Random.Range(0, pb.herolist.Count);
        PrototypeCharaFight target = isHero ? pb.enemylist[z] : pb.herolist[z];
        cm.lookTarget = isHero ? pb.enemymodel[z].transform : pb.heromodel[z].transform;
    }
    internal override void Attack()
    {
        int z = isHero ? Random.Range(0, pb.enemylist.Count) : Random.Range(0, pb.herolist.Count);
        PrototypeCharaFight target = isHero ? pb.enemylist[z] : pb.herolist[z];

      cm.lookTarget = isHero ? pb.enemymodel[z].transform : pb.heromodel[z].transform;
        Instantiate(bullet, model.transform.position, model.transform.rotation);
    }
}

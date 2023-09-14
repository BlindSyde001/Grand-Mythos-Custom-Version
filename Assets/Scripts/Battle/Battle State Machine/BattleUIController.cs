using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleUIController : MonoBehaviour
{
    [Required]
    public BattleStateMachine Battle;

    public GameObject enemyUIPrefab;

    public List<HeroExtension> heroData;
    public List<HeroPrefabUIData> heroUIData;

    public List<EnemyExtension> enemyData;
    public List<EnemyPrefabUIData> enemyUIData;
}
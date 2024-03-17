using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/UIBinding/BattleUIController")]
public class BattleUIController : MonoBehaviour
{
    [Required]
    public BattleStateMachine Battle;

    public GameObject enemyUIPrefab;

    public List<HeroExtension> heroData;
    public List<HeroPrefabUIData> heroUIData;

    public List<CharacterTemplate> enemyData;
    public List<EnemyPrefabUIData> enemyUIData;
}
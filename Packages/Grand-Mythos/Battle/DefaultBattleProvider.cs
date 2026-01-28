using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Battle
{
    public class DefaultBattleProvider : MonoBehaviour
    {
        public CharacterTemplate[] Allies = Array.Empty<CharacterTemplate>();
        public CharacterTemplate[] Opponents = Array.Empty<CharacterTemplate>();
        public bool CarryAttributeChangesBetweenSessions = false;
        public uint Seed = (uint)new System.Random().Next(int.MinValue, int.MaxValue);

        private void Awake()
        {
            if (GameManager.Instance != null!)
            {
                gameObject.SetActive(false);
                Destroy(this);
                return;
            }

            var hostileControllers = new List<BattleCharacterController>();
            var alliesControllers = new List<BattleCharacterController>();

            for (int i = 0; i < Opponents.Length; i++)
            {
                var template = Instantiate(Opponents[i]);
                template.name = $"{Opponents[i].gameObject.name} Data {i}";

                var model = Instantiate(template.BattlePrefab, template.transform);
                model.name = $"{template.gameObject.name} Model {i}";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Profile = template;
                controller.Context.CombatSeed = Seed;
                controller.Context.Random = new Random(controller.Context.CombatSeed == 0 ? 1 : controller.Context.CombatSeed);
                hostileControllers.Add(controller);
            }

            foreach (var ally in Allies)
            {
                var model = Instantiate(ally.BattlePrefab);
                model.name = $"{ally.gameObject.name} Model";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Profile = CarryAttributeChangesBetweenSessions ? ally : Instantiate(ally);
                controller.Context.CombatSeed = Seed;
                controller.Context.Random = new Random(controller.Context.CombatSeed == 0 ? 1 : controller.Context.CombatSeed);
                alliesControllers.Add(controller);
            }

            (Vector3 pos, Quaternion rot)[] hostileSpawns;
            (Vector3 pos, Quaternion rot)[] alliesSpawns;
            if (FindObjectOfType<BattleStateMachine>() is { } bsm && bsm != null)
            {
                hostileSpawns = bsm.EnemySpawns.Select(x => (x.position, x.rotation)).ToArray();
                alliesSpawns = bsm.HeroSpawns.Select(x => (x.position, x.rotation)).ToArray();
            }
            else
            {
                hostileSpawns = new (Vector3, Quaternion)[] { (default, Quaternion.identity) };
                alliesSpawns = new (Vector3, Quaternion)[] { (default, Quaternion.identity) };
                Debug.LogWarning($"Could not find {nameof(BattleStateMachine)} when trying to set encounter");
            }

            for (int i = 0; i < hostileControllers.Count; i++)
            {
                hostileControllers[i].transform.SetPositionAndRotation(hostileSpawns[i % hostileSpawns.Length].pos, hostileSpawns[i % hostileSpawns.Length].rot);
            }

            for (int i = 0; i < alliesControllers.Count; i++)
            {
                alliesControllers[i].transform.SetPositionAndRotation(alliesSpawns[i % alliesSpawns.Length].pos, alliesSpawns[i % alliesSpawns.Length].rot);
            }
        }
    }
}
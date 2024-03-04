using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[Serializable]
public abstract class BaseEncounter : IEncounterDefinition
{
    static bool startingEncounter;

    public SceneReference Scene;

    public void Start(Transform hintSource, OverworldPlayerController player)
    {
        if (startingEncounter)
            return;

        startingEncounter = true;
        var battleTransition = new GameObject(nameof(EncounterState));
        Object.DontDestroyOnLoad(battleTransition);
        var encounterState = battleTransition.AddComponent<EncounterState>();
        foreach (var reserve in GameManager._instance.ReservesLineup)
            reserve.gameObject.SetActive(false);
        encounterState.StartCoroutine(OverworldToBattleTransition(Scene, FormationToSpawn()));
    }

    static IEnumerator OverworldToBattleTransition(SceneReference Scene, CharacterTemplate[] opponents)
    {
        List<GameObject> gameObjectsToReEnable = new List<GameObject>();

        bool ranToCompletion = false;
        try
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(Scene.Path, LoadSceneMode.Additive);
            loadOperation.allowSceneActivation = false;

#warning Both game manager and event switch scene should be rewritten, right now it's a bit too constrictive

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isLoaded == false)
                    continue;

                foreach (var gameObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (gameObject.activeSelf == false)
                        continue;
                    gameObject.SetActive(false);
                    gameObjectsToReEnable.Add(gameObject);
                }
            }


            // "If you set allowSceneActivation to false, progress is halted at 0.9 until it is set to true"
            // Do note that this will fail if you specify '0.9' instead of '0.9f'
            while (loadOperation.progress < 0.9f)
                yield return null;

            loadOperation.allowSceneActivation = true;

            var hostileController = new List<BattleCharacterController>();
            for (int i = 0; i < opponents.Length; i++)
            {
                var template = Object.Instantiate(opponents[i]);
                template.name = $"{opponents[i].gameObject.name} Data {i}";

                // Add Model into Battle
                var model = Object.Instantiate(template.BattlePrefab, template.transform);

                model.name = $"{template.gameObject.name} Model {i}";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Template = template;
                template.ActionsCharged = UnityEngine.Random.Range(0, template.ActionChargeMax);
                hostileController.Add(controller);
            }

            while (loadOperation.isDone == false)
                yield return null;

            (Vector3 pos, Quaternion rot)[] spawns;
            if (Object.FindObjectOfType<BattleStateMachine>() is { } bsm && bsm != null)
            {
                spawns = bsm.EnemySpawns.Select(x => (x.position, x.rotation)).ToArray();
            }
            else
            {
                spawns = new (Vector3, Quaternion)[] { (default, Quaternion.identity) };
                Debug.LogWarning($"Could not find {nameof(BattleStateMachine)} when trying to set encounter");
            }

            for (int i = 0; i < hostileController.Count; i++)
            {
                hostileController[i].Template.transform.parent = null;
                hostileController[i].transform.SetPositionAndRotation(spawns[i].pos, spawns[i].rot);
            }

            Scene runtimeScene = default;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).path == Scene.Path)
                    runtimeScene = SceneManager.GetSceneAt(i);
            }
            Debug.Assert(runtimeScene.IsValid());
            Scene previouslyActiveScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(runtimeScene);
            var unloader = new GameObject(nameof(BackToOverworldOnDestroy)).AddComponent<BackToOverworldOnDestroy>();
            unloader.ActiveScene = previouslyActiveScene;
            unloader.gameObjectsToReEnable = gameObjectsToReEnable;

            ranToCompletion = true;
        }
        finally
        {
            if (ranToCompletion == false)
            {
                foreach (var gameObject in gameObjectsToReEnable)
                    gameObject.SetActive(true);
            }

            startingEncounter = false;
        }
    }

    public bool IsValid(out string error)
    {
        if (Scene.IsValid() == false)
        {
            error = $"{nameof(Scene)} is null";
            return false;
        }

        return SubIsValid(out error);
    }

    protected abstract bool SubIsValid(out string error);
    protected abstract CharacterTemplate[] FormationToSpawn();

    public class BackToOverworldOnDestroy : MonoBehaviour
    {
        public List<GameObject> gameObjectsToReEnable = new List<GameObject>();
        public Scene ActiveScene;

        void OnDestroy()
        {
            foreach (var gameObject in gameObjectsToReEnable)
            {
                if (gameObject != null)
                    gameObject.SetActive(true);
            }

            if (ActiveScene.isLoaded)
                SceneManager.SetActiveScene(ActiveScene);
        }
    }
}
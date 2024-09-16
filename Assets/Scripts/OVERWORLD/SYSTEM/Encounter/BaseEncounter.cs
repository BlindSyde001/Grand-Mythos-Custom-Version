using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

[Serializable]
public abstract class BaseEncounter : IEncounterDefinition
{
    static bool startingEncounter;

    public SceneReference Scene;
    public BattlePointOfViewReference PointOfView;
    public AnimationClip IntroCamera, OutroCamera;

    public void Start(Transform hintSource, OverworldPlayerController player)
    {
        if (startingEncounter)
            return;

        startingEncounter = true;
        var battleTransition = new GameObject(nameof(EncounterState));
        Object.DontDestroyOnLoad(battleTransition);
        var encounterState = battleTransition.AddComponent<EncounterState>();
        foreach (var reserve in GameManager.Instance.ReservesLineup)
            reserve.gameObject.SetActive(false);
        encounterState.StartCoroutine(OverworldToBattleTransition(Scene, GameManager.Instance.PartyLineup));
    }

    IEnumerator OverworldToBattleTransition(SceneReference Scene, IEnumerable<CharacterTemplate> allies)
    {
        var opponents = FormationToSpawn();
        var gameObjectsToReEnable = new List<GameObject>();
        var hostileControllers = new List<BattleCharacterController>();
        var alliesControllers = new List<BattleCharacterController>();

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

            for (int i = 0; i < opponents.Length; i++)
            {
                var template = Object.Instantiate(opponents[i]);
                template.name = $"{opponents[i].gameObject.name} Data {i}";

                var model = Object.Instantiate(template.BattlePrefab, template.transform);
                model.name = $"{template.gameObject.name} Model {i}";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Profile = template;
                controller.Context.CombatSeed = GetSeedForCharacter(template);
                controller.Context.Random = new Random(controller.Context.CombatSeed == 0 ? 1 : controller.Context.CombatSeed);
                hostileControllers.Add(controller);
            }

            foreach (var ally in allies)
            {
                var model = Object.Instantiate(ally.BattlePrefab);
                model.name = $"{ally.gameObject.name} Model";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Profile = ally;
                controller.Context.CombatSeed = GetSeedForCharacter(ally);
                controller.Context.Random = new Random(controller.Context.CombatSeed == 0 ? 1 : controller.Context.CombatSeed);
                alliesControllers.Add(controller);
            }

            // We have to do this awful workaround to ensure our logic runs before the update loop of that scene
            SceneManager.sceneLoaded += OnLoad;

            void OnLoad(Scene runtimeScene, LoadSceneMode lsm)
            {
                SceneManager.sceneLoaded -= OnLoad;
                var rootGameobjects = runtimeScene.GetRootGameObjects();
                if (PointOfView)
                {
                    try
                    {
                        var camera = rootGameobjects
                            .Select(x => x.GetComponentInChildren<BattleCamera>())
                            .First(x => x != null);
                        if (BattlePointOfView.Instances.TryGetValue(PointOfView, out var povComp))
                            camera.TransitionTo(povComp);
                    }
                    catch (Exception e)
                    {
                        // Let's mostly ignore exceptions related to the camera, failing to do this still leads to a working battle
                        Debug.LogException(e);
                    }
                }
                
                (Vector3 pos, Quaternion rot)[] hostileSpawns;
                (Vector3 pos, Quaternion rot)[] alliesSpawns;
                if (rootGameobjects.Select(x => x.GetComponentInChildren<BattleStateMachine>()).FirstOrDefault(x => x != null) is { } bsm && bsm != null)
                {
                    if (IntroCamera)
                        bsm.Intro = IntroCamera;
                    if (OutroCamera)
                        bsm.Outro = OutroCamera;
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
                    SceneManager.MoveGameObjectToScene(hostileControllers[i].Profile.gameObject, runtimeScene);
                    hostileControllers[i].transform.SetPositionAndRotation(hostileSpawns[i % hostileSpawns.Length].pos, hostileSpawns[i % hostileSpawns.Length].rot);
                }

                for (int i = 0; i < alliesControllers.Count; i++)
                {
                    SceneManager.MoveGameObjectToScene(alliesControllers[i].gameObject, runtimeScene);
                    alliesControllers[i].transform.SetPositionAndRotation(alliesSpawns[i % alliesSpawns.Length].pos, alliesSpawns[i % alliesSpawns.Length].rot);
                }

                Scene previouslyActiveScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(runtimeScene);
                var unloader = new GameObject(nameof(BackToOverworldOnDestroy)).AddComponent<BackToOverworldOnDestroy>();
                unloader.ActiveScene = previouslyActiveScene;
                unloader.gameObjectsToReEnable = gameObjectsToReEnable;

                ranToCompletion = true;
            }

            while (loadOperation.isDone == false)
                yield return new WaitForEndOfFrame();
        }
        finally
        {
            if (ranToCompletion == false)
            {
                foreach (var gameObject in gameObjectsToReEnable)
                    gameObject.SetActive(true);
                foreach (var controller in hostileControllers)
                    Object.Destroy(controller.Profile);
                foreach (var controller in hostileControllers)
                    Object.Destroy(controller);
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
    protected abstract uint GetSeedForCharacter(CharacterTemplate character);

    public class BackToOverworldOnDestroy : MonoBehaviour
    {
        public List<GameObject> gameObjectsToReEnable = new List<GameObject>();
        public Scene ActiveScene;

        void OnDestroy()
        {
            if (DomainReloadHelper.LastState == DomainReloadHelper.LastPlayModeState.ExitingPlayMode)
                return;

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
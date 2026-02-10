using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Battle;
using Cysharp.Threading.Tasks;
using Screenplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

[Serializable]
public abstract class BaseEncounter : IEncounterDefinition
{
    static bool startingEncounter;

    public SceneReference Scene;
    public BattlePointOfViewReference? PointOfView;
    public AnimationClip? IntroCamera, OutroCamera;

    public UniTask<BattleStateMachine> Start(CancellationToken cts)
    {
        if (startingEncounter)
            throw new Exception("Trying to start an encounter while another one is already running");

        startingEncounter = true;
        var battleTransition = new GameObject(nameof(EncounterState));
        Object.DontDestroyOnLoad(battleTransition);
        var encounterState = battleTransition.AddComponent<EncounterState>();
        return OverworldToBattleTransition(Scene, GameManager.Instance.PartyLineup, cts);
    }

    private async UniTask<BattleStateMachine> OverworldToBattleTransition(SceneReference scene, IEnumerable<CharacterTemplate> allies, CancellationToken cts)
    {
        var gameObjectsToReEnable = new List<GameObject>();
        var hostileControllers = new List<BattleCharacterController>();
        var alliesControllers = new List<BattleCharacterController>();
        var previouslyActiveScene = SceneManager.GetActiveScene();
        try
        {
            var opponents = FormationToSpawn();

            var loadOperation = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            if (loadOperation == null)
                throw new Exception($"Could not find scene '{scene.Path}'");

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
                await UniTask.NextFrame(cancellationToken:cts, cancelImmediately: true);

            for (int i = 0; i < opponents.Length; i++)
            {
                var template = Object.Instantiate(opponents[i]);
                template.name = $"{opponents[i].name} Data {i}";

                var model = Object.Instantiate(template.BattlePrefab);
                model.name = $"{template.name} Model {i}";

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
                model.name = $"{ally.name} Model";

                // Attach Relevant References
                var controller = model.GetComponent<BattleCharacterController>();
                controller.Profile = ally;
                controller.Context.CombatSeed = GetSeedForCharacter(ally);
                controller.Context.Random = new Random(controller.Context.CombatSeed == 0 ? 1 : controller.Context.CombatSeed);
                alliesControllers.Add(controller);
            }

            loadOperation.allowSceneActivation = true;
            await loadOperation.ToUniTask(cancellationToken: cts);

            var runtimeScene = SceneManager.GetSceneByPath(scene.Path);

            var rootGameObjects = runtimeScene.GetRootGameObjects();

            var bsm = rootGameObjects.Select(x => x.GetComponentInChildren<BattleStateMachine>()).FirstOrDefault(x => x != null);
            if (bsm == null)
                throw new Exception($"Could not find {nameof(BattleStateMachine)} in scene '{scene.Path}'");

            if (IntroCamera != null)
                bsm.Intro = IntroCamera;
            if (OutroCamera != null)
                bsm.Outro = OutroCamera;
            (Vector3 pos, Quaternion rot)[] hostileSpawns = bsm.EnemySpawns.Select(x => (x.position, x.rotation)).ToArray();
            (Vector3 pos, Quaternion rot)[] alliesSpawns = bsm.HeroSpawns.Select(x => (x.position, x.rotation)).ToArray();

            if (PointOfView is not null)
            {
                try
                {
                    var camera = rootGameObjects
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

            for (int i = 0; i < hostileControllers.Count; i++)
            {
                SceneManager.MoveGameObjectToScene(hostileControllers[i].gameObject, runtimeScene);
                hostileControllers[i].transform.SetPositionAndRotation(hostileSpawns[i % hostileSpawns.Length].pos, hostileSpawns[i % hostileSpawns.Length].rot);
            }

            for (int i = 0; i < alliesControllers.Count; i++)
            {
                SceneManager.MoveGameObjectToScene(alliesControllers[i].gameObject, runtimeScene);
                alliesControllers[i].transform.SetPositionAndRotation(alliesSpawns[i % alliesSpawns.Length].pos, alliesSpawns[i % alliesSpawns.Length].rot);
            }

            SceneManager.SetActiveScene(runtimeScene);
            var unloader = new GameObject(nameof(BackToOverworldOnDestroy)).AddComponent<BackToOverworldOnDestroy>();
            unloader.ActiveScene = previouslyActiveScene;
            unloader.gameObjectsToReEnable = gameObjectsToReEnable;
            return bsm;
        }
        catch(Exception e)
        {
            Debug.LogException(e);

            foreach (var gameObject in gameObjectsToReEnable)
                gameObject.SetActive(true);
            foreach (var controller in hostileControllers)
                Object.Destroy(controller.Profile);
            foreach (var controller in hostileControllers)
                Object.Destroy(controller);

            if (previouslyActiveScene.IsValid() && previouslyActiveScene.isLoaded)
                SceneManager.SetActiveScene(previouslyActiveScene);

            var runtimeScene = SceneManager.GetSceneByPath(scene.Path);
            if (runtimeScene.IsValid())
            {
                var v = SceneManager.UnloadSceneAsync(runtimeScene);
                if (v != null)
                    await v.ToUniTask(cancellationToken: CancellationToken.None);
            }

            throw;
        }
        finally
        {
            startingEncounter = false;
        }
    }

    public bool IsValid([MaybeNullWhen(true)] out string error)
    {
        if (Scene.IsValid() == false)
        {
            error = $"{nameof(Scene)} is null";
            return false;
        }

        return SubIsValid(out error);
    }

    protected abstract bool SubIsValid([MaybeNullWhen(true)] out string error);
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
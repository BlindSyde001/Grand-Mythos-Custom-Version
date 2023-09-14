using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public abstract class BaseEncounter : IEncounterDefinition
{
    static bool startingEncounter;

    public SceneReference Scene;

    public void Start(Transform hintSource, OverworldPlayerControlsNode player)
    {
        if (startingEncounter)
            return;

        startingEncounter = true;
        var battleTransition = new GameObject(nameof(EncounterState));
        UnityEngine.Object.DontDestroyOnLoad(battleTransition);
        var encounterState = battleTransition.AddComponent<EncounterState>();
        encounterState.StartCoroutine(OverworldToBattleTransition(Scene, FormationToSpawn()));
    }

    static IEnumerator OverworldToBattleTransition(SceneReference Scene, EnemyExtension[] opponents)
    {
        List<GameObject> gameObjectsToReEnable = new List<GameObject>();

        bool ranToCompletion = false;
        try
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(Scene.Path, LoadSceneMode.Additive);
            loadOperation.allowSceneActivation = false;

#warning Both game manager and event switch scene should be rewritten, right now it's a bit too constrictive
            var gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            gameManager.LastKnownScene = SceneManager.GetActiveScene().name;

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
#warning this has to go, very restrictive too
                var enemy = UnityEngine.Object.Instantiate(opponents[i], gameManager.transform);
                enemy.name = opponents[i].charName + "Data" + i;
                gameManager._EnemyLineup.Add(enemy);
            }

            while (loadOperation.isDone == false)
                yield return null;

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
    protected abstract EnemyExtension[] FormationToSpawn();

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
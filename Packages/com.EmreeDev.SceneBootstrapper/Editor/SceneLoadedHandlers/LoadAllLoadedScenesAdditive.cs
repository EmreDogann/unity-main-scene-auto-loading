using System;
using System.Collections;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    [Serializable]
    public class LoadAllLoadedScenesAdditive : ISceneLoadedHandler
    {
        public void OnMainSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            SceneSetup activeScene =
                bootstrapperData.SceneSetups.FirstOrDefault(s => s.isActive && s.path != SceneManager.GetActiveScene().path);
            if (activeScene != null)
            {
                SceneManager.LoadScene(activeScene.path, LoadSceneMode.Additive);
            }

            foreach (SceneSetup sceneSetup in bootstrapperData.SceneSetups.Where(scene =>
                         scene.isLoaded && !scene.isActive && scene.path != SceneManager.GetActiveScene().path))
            {
                SceneManager.LoadScene(sceneSetup.path, LoadSceneMode.Additive);
            }

            if (SceneBootstrapper.Settings.KeepActiveSceneAsActive)
            {
                StartSetActiveSceneCoroutine(bootstrapperData);
            }
        }

        private static EditorCoroutine StartSetActiveSceneCoroutine(SceneBootstrapperData bootstrapperData)
        {
            bool playmodeState = Application.isPlaying;
            return EditorCoroutineUtility.StartCoroutineOwnerless(SetActiveSceneEnumerator(bootstrapperData, playmodeState));
        }

        private static IEnumerator SetActiveSceneEnumerator(SceneBootstrapperData bootstrapperData, bool playmodeState)
        {
            while (!IsActiveSceneLoaded(bootstrapperData.SceneSetups))
            {
                yield return null;
                if (Application.isPlaying != playmodeState)
                {
                    Debug.Log("Playmode state was changed, stopped Set Active Scene coroutine.");
                    yield break;
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(bootstrapperData.SceneSetups.First(s => s.isActive).path));
        }

        private static bool IsActiveSceneLoaded(SceneSetup[] sceneSetups)
        {
            return sceneSetups
                .Any(s => s.isActive && SceneManager.GetSceneByPath(s.path).isLoaded);
        }

        [CustomPropertyDrawer(typeof(LoadAllLoadedScenesAdditive))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Loads all scene that was loaded in hierarchy before entering playmode.";
        }
    }
}
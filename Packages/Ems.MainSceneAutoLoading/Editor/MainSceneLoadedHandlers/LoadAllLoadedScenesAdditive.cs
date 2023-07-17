using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ems.MainSceneAutoLoading.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ems.MainSceneAutoLoading.MainSceneLoadedHandlers
{
    public class LoadAllLoadedScenesAdditive : IMainSceneLoadedHandler
    {
        public void OnMainSceneLoaded(LoadMainSceneArgs args)
        {
            SceneSetup activeScene = args.SceneSetups.FirstOrDefault(s => s.isActive && s.path != SceneManager.GetActiveScene().path);
            if (activeScene != null)
            {
                SceneManager.LoadScene(activeScene.path, LoadSceneMode.Additive);
            }
            
            foreach (var sceneSetup in args.SceneSetups.Where(scene => scene.isLoaded && !scene.isActive && scene.path != SceneManager.GetActiveScene().path))
            {
                SceneManager.LoadScene(sceneSetup.path, LoadSceneMode.Additive);
            }

            if (MainSceneAutoLoader.Settings.KeepActiveSceneAsActive)
            {
                StartSetActiveSceneCoroutine(args);
            }
            
            SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(args);
        }
        
        private static EditorCoroutine StartSetActiveSceneCoroutine(LoadMainSceneArgs args)
        {
            var playmodeState = Application.isPlaying;
            return EditorCoroutineUtility.StartCoroutineOwnerless(SetActiveSceneEnumerator(args, playmodeState));
        }

        private static IEnumerator SetActiveSceneEnumerator(LoadMainSceneArgs args, bool playmodeState)
        {
            while (!IsActiveSceneLoaded(args.SceneSetups))
            {
                yield return null;
                if (Application.isPlaying != playmodeState)
                {
                    Debug.Log("Playmode state was changed, stopped Set Active Scene coroutine.");
                    yield break;
                }
            }
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(args.SceneSetups.First(s => s.isActive).path));
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
                $"Loads all scene that was loaded in hierarchy before entering playmode.";
        }
    }
}
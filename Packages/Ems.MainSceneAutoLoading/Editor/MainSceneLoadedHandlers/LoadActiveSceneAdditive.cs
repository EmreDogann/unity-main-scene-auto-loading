using System.Linq;
using Ems.MainSceneAutoLoading.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Ems.MainSceneAutoLoading.MainSceneLoadedHandlers
{
    public class LoadActiveSceneAdditive : IMainSceneLoadedHandler
    {
        public void OnMainSceneLoaded(LoadMainSceneArgs args)
        {
            SceneSetup activeScene = args.SceneSetups.First(s => s.isActive);
            SceneManager.LoadScene(activeScene.path, LoadSceneMode.Additive);

            if (MainSceneAutoLoader.Settings.KeepActiveSceneAsActive)
            {
                void SceneLoadDelegate(Scene scene, LoadSceneMode loadedSceneMode)
                {
                    if (scene.path == activeScene.path)
                    {
                        SceneManager.SetActiveScene(SceneManager.GetSceneByPath(activeScene.path));
                        SceneManager.sceneLoaded -= SceneLoadDelegate;
                    }
                }

                SceneManager.sceneLoaded += SceneLoadDelegate;
            }

            SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(args);
        }
        
        [CustomPropertyDrawer(typeof(LoadActiveSceneAdditive))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                $"Loads only one scene that was active(with bold name) in hierarchy before entering playmode.";
        }
    }
}
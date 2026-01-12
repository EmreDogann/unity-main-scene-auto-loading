using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    [Serializable]
    public class LoadActiveSceneAdditive : ISceneLoadedHandler
    {
        public void OnSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            SceneSetup activeScene = bootstrapperData.SceneSetups.First(s => s.isActive);
            SceneManager.LoadScene(activeScene.path, LoadSceneMode.Additive);

            if (SceneBootstrapper.Settings.KeepActiveSceneAsActive)
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
        }

        [CustomPropertyDrawer(typeof(LoadActiveSceneAdditive))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Additively loads only the active scene in the hierarchy.";
        }
    }
}
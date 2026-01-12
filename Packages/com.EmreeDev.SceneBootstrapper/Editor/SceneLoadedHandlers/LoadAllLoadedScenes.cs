using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    [Serializable]
    public class LoadAllLoadedScenes : ISceneLoadedHandler
    {
        public void OnSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            SceneManager.LoadScene(bootstrapperData.SceneSetups.First(s => s.isActive).path);
            foreach (SceneSetup sceneSetup in bootstrapperData.SceneSetups.Where(scene => scene.isLoaded && !scene.isActive))
            {
                SceneManager.LoadScene(sceneSetup.path, LoadSceneMode.Additive);
            }
        }

        [CustomPropertyDrawer(typeof(LoadAllLoadedScenes))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Non-additively loads all scenes that were loaded in the hierarchy before entering playmode. Loads the active scene first.";
        }
    }
}
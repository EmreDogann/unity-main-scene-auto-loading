using System;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    [Serializable]
    public class LoadActiveScene : ISceneLoadedHandler
    {
        public void OnSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            string path = bootstrapperData.SceneSetups.First(scene => scene.isActive).path;
            SceneManager.LoadScene(path);
        }

        [CustomPropertyDrawer(typeof(LoadActiveScene))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Non-Additively loads only the active scene in the hierarchy.";
        }
    }
}
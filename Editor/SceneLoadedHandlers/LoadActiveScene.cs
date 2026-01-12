using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    public class LoadActiveScene : ISceneLoadedHandler
    {
        public void OnMainSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            string path = bootstrapperData.SceneSetups.First(scene => scene.isActive).path;
            SceneManager.LoadScene(path);
        }

        [CustomPropertyDrawer(typeof(LoadActiveScene))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Loads only one scene that was active(with bold name) in hierarchy before entering playmode.";
        }
    }
}
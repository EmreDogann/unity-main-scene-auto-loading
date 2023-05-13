using System.Linq;
using Ems.MainSceneAutoLoading.Utilities;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Ems.MainSceneAutoLoading.MainSceneLoadedHandlers
{
    public class LoadActiveSceneAdditive : IMainSceneLoadedHandler
    {
        public void OnMainSceneLoaded(LoadMainSceneArgs args)
        {
            var path = args.SceneSetups.First(scene => scene.isActive).path;
            SceneManager.LoadScene(path, LoadSceneMode.Additive);
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
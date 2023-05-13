using System.Linq;
using Ems.MainSceneAutoLoading.Utilities;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Ems.MainSceneAutoLoading.MainSceneLoadedHandlers
{
    public class LoadAllLoadedScenesAdditive : IMainSceneLoadedHandler
    {
        public void OnMainSceneLoaded(LoadMainSceneArgs args)
        {
            SceneManager.LoadScene(args.SceneSetups.First(s => s.isActive).path, LoadSceneMode.Additive);
            foreach (var sceneSetup in args.SceneSetups.Where(scene => scene.isLoaded && !scene.isActive))
            {
                SceneManager.LoadScene(sceneSetup.path, LoadSceneMode.Additive);
            }
            
            SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(args);
        }
        
        [CustomPropertyDrawer(typeof(LoadAllLoadedScenesAdditive))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                $"Loads all scene that was loaded in hierarchy before entering playmode.";
        }
    }
}